import os
import time

from common.arguments import parse_args
from common.camera import *
from common.generators import UnchunkedGenerator
from common.loss import *
from common.model import *
from common.utils import Timer, add_path
import torch
import onnx
import torchvision
from common.arguments import parse_args
from torch.autograd import Variable

######### convert temporal model to onnx
args = parse_args()
onnx_pose_path='./checkpoint/ONNX/cpn_pt_243_static_11.onnx'
onnx_traj_path='./checkpoint/ONNX/243_h36m_detectron_coco_wtraj_static_11.onnx'
model_pos = TemporalModel(17, 2, 17, filter_widths=[3, 3, 3, 3, 3], causal=args.causal, dropout=args.dropout,
                          channels=args.channels,
                          dense=args.dense)
model_traj = TemporalModel(17, 2, 1,
                           filter_widths=[3, 3, 3, 3, 3], causal=args.causal, dropout=args.dropout,
                           channels=args.channels,
                           dense=args.dense)

#
pose_filename = os.path.join('checkpoint', 'cpn-pt-243.bin')
traj_filename= os.path.join('checkpoint', 'pretrained_243_h36m_detectron_coco_wtraj.bin')
# print('Loading checkpoint', pose_filename)
# print('Loading checkpoint-traj', traj_filename)
checkpoint_pose = torch.load(pose_filename, map_location=lambda storage, loc: storage)  # 把loc映射到storage
checkpoint_traj = torch.load(traj_filename, map_location=lambda storage, loc: storage)  # 把loc映射到storage
model_pos.load_state_dict(checkpoint_pose['model_pos'])
model_traj.load_state_dict((checkpoint_traj['model_traj']))
#
model_traj.eval()
model_pos.eval()
framenums=243
input_pose=torch.randn(1,243,17,2)
torch.onnx.export(model_pos,
                  input_pose,
                  onnx_pose_path,
                  export_params=True,
                  opset_version=11,
                  do_constant_folding=True,
                  input_names=['inputs'],
                  output_names=['outputs'],
                  # dynamic_axes={'inputs':{1:'framenums_added'},
                  #               'outputs':{1:'framenums'}},
                  )
print('convert pose successfully')
input_traj=torch.randn(1,243,17,2)
torch.onnx.export(model_traj,
                  input_traj,
                  onnx_traj_path,
                  export_params=True,
                  opset_version=11,
                  do_constant_folding=True,
                  input_names=['inputs'],
                  output_names=['outputs'],
                  # dynamic_axes={'inputs': {1:'framenums_added'},
                  #               'outputs': {1:'framenums'}},
                  )
print('convert traj successfully')
