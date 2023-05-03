import os
import time

from common.arguments import parse_args
from common.camera import *
from common.generators import UnchunkedGenerator
from common.loss import *
from common.model import *
from common.utils import Timer, add_path
from torch.utils.data import dataloader,dataset
from joints_detectors.Baseline.gene_npz_baseline import generate_kpts
import cv2
import onnx
import onnxruntime
import numpy


# from joints_detectors.openpose.main import generate_kpts as open_pose
# from joints_detectors.Alphapose.gene_npz import handle_video

os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"  # see issue #152
os.environ["CUDA_VISIBLE_DEVICES"] = "0"

metadata = {'layout_name': 'coco', 'num_joints': 17, 'keypoints_symmetry': [[1, 3, 5, 7, 9, 11, 13, 15], [2, 4, 6, 8, 10, 12, 14, 16]]}

add_path()


# record time
def ckpt_time(ckpt=None):
    if not ckpt:
        return time.time()
    else:
        return time.time() - float(ckpt), time.time()


time0 = ckpt_time()



# def detector_2d(detector_name):
#
#     def get_alpha_pose():
#         from joints_detectors.Alphapose.gene_npz import generate_kpts as alpha_pose
#         return alpha_pose
#     detector_map = {
#         'alpha_pose': get_alpha_pose,
#     }
#
#     return detector_map[detector_name]()


class Skeleton:
    def parents(self):
        return np.array([-1, 0, 1, 2, 0, 4, 5, 0, 7, 8, 9, 8, 11, 12, 8, 14, 15])

    def joints_right(self):
        return [1, 2, 3, 9, 10]


def main(args):

    # 2D kpts loads or generate
    if not args.input_npz:
        # detector_2d = get_detector_2d(args.detector_2d)
        # assert detector_2d, 'detector_2d should be in ({alpha, hr, open}_pose)'
        video_name = args.viz_video
        keypoints,framenums = generate_kpts(video_name)
    else:
        npz = np.load(args.input_npz)
        keypoints = npz['kpts']  # (N, 17, 2)

    keypoints_symmetry = metadata['keypoints_symmetry']
    kps_left, kps_right = list(keypoints_symmetry[0]), list(keypoints_symmetry[1])
    joints_left, joints_right = list([4, 5, 6, 11, 12, 13]), list([1, 2, 3, 14, 15, 16])

    # normlization keypoints  Suppose using the camera parameter
    keypoints = normalize_screen_coordinates(keypoints[..., :2], w=1000, h=1002)
    print("after normalize size{}".format(keypoints.shape))

    ckpt, time1 = ckpt_time(time0)
    print('-------------- load data spends {:.2f} seconds'.format(ckpt))

    ckpt, time2 = ckpt_time(time1)
    model_traj_path='./checkpoint/ONNX/243_h36m_detectron_coco_wtraj_changed.onnx'
    model_pose_path='./checkpoint/ONNX/cpn_pt_243_changed.onnx'
    model_traj=onnx.load(model_traj_path)
    model_pos=onnx.load(model_pose_path)
    onnx.checker.check_model(model_pos)
    onnx.checker.check_model(model_traj)

    ort_session_pose=onnxruntime.InferenceSession(model_pose_path)
    ort_session_traj = onnxruntime.InferenceSession(model_traj_path)

    print('-------------- load 3D model spends {:.2f} seconds'.format(ckpt))

    #  Receptive field: 243 frames for args.arc [3, 3, 3, 3, 3]
    receptive_field = 243
    pad = (receptive_field - 1) // 2  # Padding on each side
    print("pading ={}".format(pad))
    causal_shift = 0

    def evaluate(test_generator, action=None, return_predictions=False, use_trajectory_model=False):
        """
        Inference the 3d positions from 2d position.
        :type test_generator: UnchunkedGenerator
        :param test_generator:
        :param model_pos: 3d pose model
        :param return_predictions: return predictions if true
        :return:
        """
        joints_left, joints_right = list([4, 5, 6, 11, 12, 13]), list([1, 2, 3, 14, 15, 16])
        with torch.no_grad():
            N = 0
            for _, batch, batch_2d in test_generator.next_epoch():
                inputs_2d = torch.from_numpy(batch_2d.astype('float32'))
                if torch.cuda.is_available():
                    inputs_2d = inputs_2d
                # Positional model
                if not use_trajectory_model:
                    print("pose model input shape !!!!{}".format(inputs_2d.cpu().numpy().shape))
                    inputs={ort_session_pose.get_inputs()[0].name:inputs_2d.cpu().numpy()}
                    predicted_3d_pos = ort_session_pose.run(None,inputs)
                    predicted_3d_pos=predicted_3d_pos[0]
                    # print(np.array(predicted_3d_pos).shape)
                    predicted_3d_pos=torch.Tensor(predicted_3d_pos)
                    # predicted_3d_pos = model_pos(inputs_2d)
                    print("pose model output shape !!!!{}".format(predicted_3d_pos.shape))
                else:
                    print("traj model input shape !!!!{}".format(inputs_2d.shape))
                    inputs = {ort_session_traj.get_inputs()[0].name: inputs_2d.cpu().numpy()}
                    predicted_3d_pos = ort_session_traj.run(None, inputs)
                    predicted_3d_pos = predicted_3d_pos[0]
                    predicted_3d_pos = torch.Tensor(predicted_3d_pos)
                    # predicted_3d_pos = model_traj(inputs_2d)
                    print("traj model output shape !!!!{}".format(predicted_3d_pos.shape))
                if test_generator.augment_enabled():
                    # Undo flipping and take average with non-flipped version
                    predicted_3d_pos[1, :, :, 0] *= -1
                    if not use_trajectory_model:
                        predicted_3d_pos[1, :, joints_left + joints_right] = predicted_3d_pos[1, :,
                                                                             joints_right + joints_left]
                    predicted_3d_pos = torch.mean(predicted_3d_pos, dim=0, keepdim=True)
                if return_predictions:
                    # predicted_3d_pos=np.array(predicted_3d_pos)
                    return predicted_3d_pos.squeeze(0).cpu().numpy()
                    # return predicted_3d_pos

    print('Rendering...')
    input_keypoints = keypoints.copy()
    gen = UnchunkedGenerator(None, None, [input_keypoints],
                             pad=pad, causal_shift=causal_shift, augment=False,
                             kps_left=kps_left, kps_right=kps_right, joints_left=joints_left, joints_right=joints_right)
    prediction = evaluate(gen,return_predictions=True)
    prediction_traj = evaluate(gen, return_predictions=True, use_trajectory_model=True)
    prediction += prediction_traj
    # save 3D joint points
    np.save('outputs/baselineOut/test_3d_output.npy', prediction, allow_pickle=True)

    rot = np.array([0.14070565, -0.15007018, -0.7552408, 0.62232804], dtype=np.float32)
    prediction = camera_to_world(prediction, R=rot, t=0)

    # We don't have the trajectory, but at least we can rebase the height
    prediction[:, :, 2] -= np.min(prediction[:, :, 2])
    anim_output = {'Reconstruction': prediction}
    input_keypoints = image_coordinates(input_keypoints[..., :2], w=1000, h=1002)

    ckpt, time3 = ckpt_time(time2)
    print('-------------- generate reconstruction 3D data spends {:.2f} seconds'.format(ckpt))

    from common.visualization import render_animation
    render_animation(input_keypoints, anim_output,
                     Skeleton(), 25, args.viz_bitrate, np.array(70., dtype=np.float32), args.viz_output,
                     limit=args.viz_limit, downsample=args.viz_downsample, size=args.viz_size,
                     input_video_path=args.viz_video, viewport=(1000, 1002),
                     input_video_skip=args.viz_skip)

    ckpt, time4 = ckpt_time(time3)
    print('total spend {:2f} second'.format(ckpt))


def inference_video(video_path):
    """
    Do image -> 2d points -> 3d points to video.
    :return: None
    """
    args = parse_args()

    dir_name = os.path.dirname(video_path)
    basename = os.path.basename(video_path)
    video_name = basename[:basename.rfind('.')]
    args.viz_video = video_path
    # args.casual=True
    args.viz_output = f'{dir_name}/onnx_baseline222_{video_name}.mp4'
    # args.viz_limit = 20
    args.input_npz = 'outputs/baselineOut/baseline_kunkun_cut/kunkun_cut.npz'

    # args.evaluate = 'cpn-pt-243.bin'
    args.casual=True

    with Timer(video_path):
        main(args)

#
if __name__ == '__main__':
    inference_video('video_input/kunkun_cut.mp4')
