using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.IO;

public class Control : MonoBehaviour
{
    // Start is called before the first frame update
    int curf = 0;
    //public Transform trackDraw;
    protected Animator animator;
    private List<List<Vector3>> pose3D = new List<List<Vector3>>();
    float left_uparm_len, left_lowarm_len, right_uparm_len, right_lowarm_len, left_upleg_len, left_lowleg_len, right_upleg_len, right_lowleg_len, low_spine, up_spine;
    private Transform root, spine, chest, neck, lscapular, lshoulder, lelbow, lhand, rscapular, rshoulder, relbow, rhand, lhip, lknee, lfoot, ltoe, rhip, rknee, rfoot, rtoe;
    Quaternion root_offset, spine_offset, chest_offset, lscapular_offset, lshoulder_offset, lelbow_offset, lhand_offset, rscapular_offset, rshoulder_offset, relbow_offset, rhand_offset, lhip_offset, lknee_offset, lfoot_offset, ltoe_offset, rhip_offset, rknee_offset, rfoot_offset, rtoe_offset;
    private Vector3 left_uparm_down, left_lowarm_down, left_hand_down, right_uparm_down, right_lowarm_down, right_hand_down;
    private Vector3 left_upleg_forward, left_lowleg_forward, left_foot_forward, right_upleg_forward, right_lowleg_forward, right_foot_forward;
    private Vector3 body_init_dir, spine_forward, chest_forward;
    private Tensor<float> outputs;
    private int frame = 0;
    private int playRatio = 20;
    //public Transform trackDraw;
    int rate = 10;
    // Start is called before the first frame update
    void Start()
    {
        StreamReader sr = new StreamReader(@"F:\UnityWorks\BSSeries\Final\Resource\record_camera.txt", System.Text.Encoding.Default);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            float[] tmp = new float[51];
            int count = 0;
            foreach (string num in line.Split(' '))
            {
                Debug.Log(num);
                tmp[count] = float.Parse(num);
                count = count + 1;
                if (count >= 51)
                {
                    break;
                }
            }
            List<Vector3> posvec = new List<Vector3>();
            for (int i = 0; i < 17; i++)
            {
                posvec.Add(new Vector3(-1 * tmp[i * 3], -1 * tmp[i * 3 + 1], -1 * tmp[i * 3 + 2]));
            }
            pose3D.Add(posvec);
        }
        animator = GetComponent<Animator>();
        initbone();
    }
    int ff = 0;
    // Update is called once per frame
    void Update()
    {
        List<Vector3> curpos = pose3D[frame / playRatio];
        updatePose(curpos);
        drawSkel(curpos);

        if (frame / playRatio < pose3D.Count - 1)
            frame = frame + 1;
        else
            frame = 0;
        //updatePose(bone_pos);
        //if (frame / rate < tryonnx.imagesNum - 1)
        //{
        //    frame++;
        //}
        //else
        //{
        //    frame = 0;
        //}

    }
    void initbone()
    {
        //躯干
        root = animator.GetBoneTransform(HumanBodyBones.Hips);
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        //左臂
        lscapular = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        lshoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        lelbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        lhand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        //右臂
        rscapular = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
        rshoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        relbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rhand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        //左腿
        lhip = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        lknee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        lfoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        ltoe = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        //右腿
        rhip = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        rknee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        rfoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        rtoe = animator.GetBoneTransform(HumanBodyBones.RightToes);
        /////////////////////////////////////////////////// 计算中间变换量////////////////////////////////////////
        //躯干
        root_offset = Quaternion.FromToRotation(root.rotation * Vector3.forward, Vector3.forward);
        spine_offset = Quaternion.FromToRotation(chest.localPosition, Vector3.forward);
        chest_offset = Quaternion.FromToRotation(neck.localPosition, Vector3.forward);
        // 左臂
        lshoulder_offset = Quaternion.FromToRotation(lelbow.localPosition, Vector3.forward);
        lelbow_offset = Quaternion.FromToRotation(lhand.localPosition, Vector3.forward);

        // 右臂
        rshoulder_offset = Quaternion.FromToRotation(relbow.localPosition, Vector3.forward);
        relbow_offset = Quaternion.FromToRotation(rhand.localPosition, Vector3.forward);

        // 左腿
        lhip_offset = Quaternion.FromToRotation(lknee.localPosition, Vector3.forward);
        lknee_offset = Quaternion.FromToRotation(lfoot.localPosition, Vector3.forward);
        lfoot_offset = Quaternion.FromToRotation(ltoe.localPosition, Vector3.forward);

        //右腿
        rhip_offset = Quaternion.FromToRotation(rknee.localPosition, Vector3.forward);
        rknee_offset = Quaternion.FromToRotation(rfoot.localPosition, Vector3.forward);
        rfoot_offset = Quaternion.FromToRotation(rtoe.localPosition, Vector3.forward);

        // 指示信息
        body_init_dir = Vector3.Cross(rhip.position - lhip.position, Vector3.up);
        Vector3 spine_root = chest.position - spine.position;
        Vector3.OrthoNormalize(ref spine_root, ref body_init_dir);
        spine_forward = Quaternion.Inverse(spine.rotation) * body_init_dir;
        Vector3 chest_spine = neck.position - chest.position;
        Vector3.OrthoNormalize(ref chest_spine, ref body_init_dir);
        chest_forward = Quaternion.Inverse(chest.rotation) * body_init_dir;
        Vector3 tmp1, tmp2, tmp3;
        // 左臂
        tmp1 = lelbow.position - lshoulder.position;
        tmp2 = lhand.position - lelbow.position;
        tmp3 = Vector3.Cross(animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal).position - lhand.position, animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal).position - lhand.position);
        left_hand_down = Quaternion.Inverse(lhand.rotation) * tmp3;
        Vector3.OrthoNormalize(ref tmp1, ref tmp3);
        left_uparm_down = Quaternion.Inverse(lshoulder.rotation) * tmp3;
        Vector3.OrthoNormalize(ref tmp2, ref tmp3);
        left_lowarm_down = Quaternion.Inverse(lelbow.rotation) * tmp3;
        //右臂
        tmp1 = relbow.position - rshoulder.position;
        tmp2 = rhand.position - relbow.position;
        tmp3 = Vector3.Cross(animator.GetBoneTransform(HumanBodyBones.RightLittleProximal).position - rhand.position, animator.GetBoneTransform(HumanBodyBones.RightThumbProximal).position - rhand.position);
        right_hand_down = Quaternion.Inverse(rhand.rotation) * tmp3;
        Vector3.OrthoNormalize(ref tmp1, ref tmp3);
        right_uparm_down = Quaternion.Inverse(rshoulder.rotation) * tmp3;
        Vector3.OrthoNormalize(ref tmp2, ref tmp3);
        right_lowarm_down = Quaternion.Inverse(relbow.rotation) * tmp3;
        //左腿
        tmp1 = lknee.position - lhip.position;
        tmp2 = lfoot.position - lknee.position;
        tmp3 = ltoe.position - lfoot.position;
        left_foot_forward = Quaternion.Inverse(lfoot.rotation) * tmp3;
        Vector3.OrthoNormalize(ref tmp1, ref tmp3);
        left_upleg_forward = Quaternion.Inverse(lhip.rotation) * tmp3;
        Vector3.OrthoNormalize(ref tmp2, ref tmp3);
        left_lowleg_forward = Quaternion.Inverse(lknee.rotation) * tmp3;
        //右腿
        tmp1 = rknee.position - rhip.position;
        tmp2 = rfoot.position - rknee.position;
        tmp3 = rtoe.position - rfoot.position;
        right_foot_forward = Quaternion.Inverse(rfoot.rotation) * tmp3;
        Vector3.OrthoNormalize(ref tmp1, ref tmp3);
        right_upleg_forward = Quaternion.Inverse(rhip.rotation) * tmp3;
        Vector3.OrthoNormalize(ref tmp2, ref tmp3);
        right_lowleg_forward = Quaternion.Inverse(rknee.rotation) * tmp3;

        /////////////////////////////////////////////////// 骨骼长度 ///////////////////////////////////////////////////
        float ratio = (animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position - animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position).magnitude / animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).localPosition.magnitude;
        // spine 
        low_spine = animator.GetBoneTransform(HumanBodyBones.Spine).localPosition.magnitude * ratio;
        up_spine = animator.GetBoneTransform(HumanBodyBones.Chest).localPosition.magnitude * ratio;
        // arms
        left_uparm_len = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).localPosition.magnitude * ratio;
        left_lowarm_len = animator.GetBoneTransform(HumanBodyBones.LeftHand).localPosition.magnitude * ratio;
        right_uparm_len = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).localPosition.magnitude * ratio;
        right_lowarm_len = animator.GetBoneTransform(HumanBodyBones.RightHand).localPosition.magnitude * ratio;
        //legs 
        left_upleg_len = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localPosition.magnitude * ratio;
        left_lowleg_len = animator.GetBoneTransform(HumanBodyBones.LeftFoot).localPosition.magnitude * ratio;
        right_upleg_len = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localPosition.magnitude * ratio;
        right_lowleg_len = animator.GetBoneTransform(HumanBodyBones.RightFoot).localPosition.magnitude * ratio;
    }
    Vector3 recal_pos(Vector3 a, Vector3 b, float len)
    {
        return a + (b - a).normalized * len;
    }

    void drawSkel(List<Vector3> skelpos)
    {
        //trackDraw.position = skelpos[0];
        int[] parent = new int[] { -1, 0, 1, 2, 0, 4, 5, 0, 7, 8, 9, 8, 11, 12, 8, 14, 15 };
        for (int i = 0; i < 17; i++)
        {
            if (parent[i] != -1)
            {
                if (i == 4 || i == 5 || i == 6 || i == 11 || i == 12 || i == 13)
                {
                    Debug.DrawLine(skelpos[i] * 50, skelpos[parent[i]] * 50, Color.red);
                }
                else
                {
                    Debug.DrawLine(skelpos[i] * 50, skelpos[parent[i]] * 50, Color.yellow);
                }
            }
        }
    }
    void updatePose(List<Vector3> bone_pos)
    {


        /*{0-'hip', 1-'RHip', 2-'RKnee', 3-'RFoot', 4-'LHip',5-'LKnee', 6-'LFoot', 
        7-'Spine', 
        8-'Neck', 9-'Head', 10-'LShoulder', 11-'LElbow', 12-'LWrist', 
        13-'RShoulder', 14-'RElbow', 15-'RWrist'}*/

        // update root   
        Vector3 body_dir = Vector3.Cross(bone_pos[1] - bone_pos[4], Vector3.up);
        Debug.DrawLine(root.position, root.position + body_dir * 100, Color.red);
        root.LookAt(root.position + body_dir, Vector3.up);
        Quaternion update_rot = Quaternion.FromToRotation(body_dir, body_init_dir);

        Vector3 chest_dir = bone_pos[8] - bone_pos[7];
        Vector3 spine_dir = bone_pos[7] - bone_pos[0];

        Vector3 shoulder_dir = Vector3.Cross(bone_pos[14] - bone_pos[11], Vector3.up);

        /// update spine 
        // look at position       
        //Debug.Log(Vector3.Angle(spine.rotation * spine_forward, chest.position - spine.position));
        spine.LookAt(spine.position + spine_dir, Vector3.zero);
        spine.rotation = spine.rotation * spine_offset;
        //Debug.Log(Vector3.Angle(spine_dir, chest.position - spine.position));
        // twist
        Vector3 spine_forward_new = body_dir.normalized + shoulder_dir.normalized;
        Vector3.OrthoNormalize(ref (spine_dir), ref (spine_forward_new));
        Vector3 spine_forward_old = spine.rotation * spine_forward;
        Quaternion spine_add_rot = Quaternion.FromToRotation(spine_forward_old, spine_forward_new);
        spine.rotation = spine_add_rot * spine.rotation;

        /// update chest      
        // look at position
        chest.LookAt(chest.position + chest_dir, Vector3.zero);
        chest.rotation = chest.rotation * chest_offset;
        //twist
        Vector3.OrthoNormalize(ref (chest_dir), ref (shoulder_dir));
        Vector3 chest_forward_old = chest.rotation * chest_forward;
        Quaternion chest_add_rot = Quaternion.FromToRotation(chest_forward_old, shoulder_dir);
        chest.rotation = chest_add_rot * chest.rotation;

        //////////////////////////////////////// left arm //////////////////////////////////////        
        Vector3 cur_left_shoulder = lshoulder.position;
        Vector3 left_up_arm_dir = bone_pos[12] - bone_pos[11];
        Vector3 left_low_arm_dir = bone_pos[13] - bone_pos[12];
        Vector3 left_arm_norm = Vector3.Cross(left_low_arm_dir, left_up_arm_dir);
        Vector3 left_hand_norm = left_arm_norm;// Vector3.Cross(bone_pos[3] - bone_pos[2], bone_pos[4] - bone_pos[2]);
        Vector3 left_elbow_norm = left_hand_norm;
        Vector3.OrthoNormalize(ref left_low_arm_dir, ref left_elbow_norm);

        /// update left elbow
        //look at position
        Vector3 new_lelbow_pos = recal_pos(cur_left_shoulder, cur_left_shoulder + left_up_arm_dir, left_uparm_len);
        lshoulder.LookAt(new_lelbow_pos, Vector3.zero);
        lshoulder.rotation = lshoulder.rotation * lshoulder_offset;
        //twist
        Vector3 l_uparm_twist_ori = lshoulder.rotation * left_uparm_down;
        Quaternion l_uparm_twist_rot = Quaternion.FromToRotation(l_uparm_twist_ori, left_arm_norm);
        lshoulder.rotation = l_uparm_twist_rot * lshoulder.rotation;

        /// update left wrist 
        // look at position
        Vector3 new_lhand_pos = recal_pos(new_lelbow_pos, new_lelbow_pos + left_low_arm_dir, left_lowarm_len);
        lelbow.LookAt(new_lhand_pos, Vector3.zero);
        lelbow.rotation = lelbow.rotation * lelbow_offset;
        // twist         
        Vector3 l_lowarm_twist_ori = lelbow.rotation * left_lowarm_down;
        if (Vector3.Angle(left_arm_norm, left_hand_norm) > 180)
        {
            left_hand_norm = -left_hand_norm;
        }
        Quaternion l_lowarm_twist_rot = Quaternion.FromToRotation(l_lowarm_twist_ori, left_elbow_norm);
        lelbow.rotation = l_lowarm_twist_rot * lelbow.rotation;

        /// update left hand direction        

        //////////////////////////////////////// right arm //////////////////////////////////////
        Vector3 cur_right_shoulder = rshoulder.position;
        Vector3 right_up_arm_dir = bone_pos[15] - bone_pos[14];
        Vector3 right_low_arm_dir = bone_pos[16] - bone_pos[15];
        Vector3 right_arm_norm = Vector3.Cross(right_up_arm_dir, right_low_arm_dir);
        Vector3 right_hand_norm = right_arm_norm;// Vector3.Cross(bone_pos[9] - bone_pos[7], bone_pos[8] - bone_pos[7]);
        Vector3 right_elbow_norm = right_hand_norm;
        Vector3.OrthoNormalize(ref right_low_arm_dir, ref right_elbow_norm);
        /// update right elbow 
        // look at position
        Vector3 new_relbow_pos = recal_pos(cur_right_shoulder, cur_right_shoulder + right_up_arm_dir, right_uparm_len);
        rshoulder.LookAt(new_relbow_pos, Vector3.zero);
        rshoulder.rotation = rshoulder.rotation * rshoulder_offset;
        //twist
        Vector3 r_uparm_twist_ori = rshoulder.rotation * right_uparm_down;
        Quaternion r_uparm_twist_rot = Quaternion.FromToRotation(r_uparm_twist_ori, right_arm_norm);
        rshoulder.rotation = r_uparm_twist_rot * rshoulder.rotation;

        /// update right wrist 
        // look at position        
        Vector3 new_rhand_pos = recal_pos(new_relbow_pos, new_relbow_pos + right_low_arm_dir, right_lowarm_len);
        relbow.LookAt(new_rhand_pos, Vector3.zero);
        relbow.rotation = relbow.rotation * relbow_offset;
        // twist 
        Vector3 r_lowarm_twist_ori = relbow.rotation * right_lowarm_down;
        if (Vector3.Angle(right_arm_norm, right_hand_norm) > 180)
        {
            right_hand_norm = -right_hand_norm;
        }
        Quaternion r_lowarm_twist_rot = Quaternion.FromToRotation(r_lowarm_twist_ori, right_elbow_norm);
        relbow.rotation = r_lowarm_twist_rot * relbow.rotation;
        // update right hand direction        


        //////////////////////////////////////// left leg //////////////////////////////////////
        Vector3 left_up_leg_dir = bone_pos[5] - bone_pos[4];
        Vector3 left_low_leg_dir = bone_pos[6] - bone_pos[5];
        Vector3 left_foot_dir = body_dir;// bone_pos[18] - bone_pos[17]; //暂定人体方向是脚的方向
        Vector3 left_leg_norm = Vector3.Cross(left_up_leg_dir, left_low_leg_dir);
        Vector3 left_foot_norm = Vector3.Cross(left_low_leg_dir, left_foot_dir);
        /// update left knee
        // look at position
        Vector3 cur_left_hip = lhip.position;
        Vector3 new_lknee_pos = recal_pos(cur_left_hip, cur_left_hip + left_up_leg_dir, left_upleg_len);
        lhip.LookAt(new_lknee_pos, Vector3.zero);
        lhip.rotation = lhip.rotation * lhip_offset;
        // twist
        Vector3 l_upleg_twist_ori = lhip.rotation * left_upleg_forward;
        Vector3 l_upleg_twist_new = lhip.rotation * left_upleg_forward;
        Vector3.OrthoNormalize(ref left_leg_norm, ref l_upleg_twist_new);
        if (Vector3.Angle(new Vector3(body_dir.x, 0, body_dir.z), new Vector3(l_upleg_twist_new.x, 0, l_upleg_twist_new.z)) > 90)
            l_upleg_twist_new = -l_upleg_twist_new;
        Quaternion l_upleg_twist_rot = Quaternion.FromToRotation(l_upleg_twist_ori, l_upleg_twist_new);
        lhip.rotation = l_upleg_twist_rot * lhip.rotation;


        ///update left foot
        // look at position
        Vector3 new_lfoot_pos = recal_pos(new_lknee_pos, new_lknee_pos + left_low_leg_dir, left_lowleg_len);
        lknee.LookAt(new_lfoot_pos, Vector3.zero);
        lknee.rotation = lknee.rotation * lknee_offset;
        // twist
        Vector3 l_lowleg_twist_ori = lknee.rotation * left_lowleg_forward;
        Vector3 l_lowleg_twist_new = lknee.rotation * left_lowleg_forward;
        Vector3.OrthoNormalize(ref left_foot_norm, ref l_lowleg_twist_new);
        if (Vector3.Angle(new Vector3(body_dir.x, 0, body_dir.z), new Vector3(l_lowleg_twist_new.x, 0, l_lowleg_twist_new.z)) > 90)
            l_lowleg_twist_new = -l_lowleg_twist_new;
        Quaternion l_lowleg_twist_rot = Quaternion.FromToRotation(l_lowleg_twist_ori, l_lowleg_twist_new);
        lknee.rotation = l_lowleg_twist_rot * lknee.rotation;

        //////////////////////////////////////// right leg //////////////////////////////////////
        Vector3 right_up_leg_dir = bone_pos[2] - bone_pos[1];
        Vector3 right_low_leg_dir = bone_pos[3] - bone_pos[2];
        Vector3 right_foot_dir = body_dir;// bone_pos[22] - bone_pos[21]; //暂定人体方向是脚的方向
        Vector3 right_leg_norm = Vector3.Cross(right_up_leg_dir, right_low_leg_dir);
        Vector3 right_foot_norm = Vector3.Cross(right_low_leg_dir, right_foot_dir);
        /// update right knee
        // look at position
        Vector3 cur_right_hip = rhip.position;
        Vector3 new_rknee_pos = recal_pos(cur_right_hip, cur_right_hip + right_up_leg_dir, right_upleg_len);
        rhip.LookAt(new_rknee_pos, Vector3.zero);
        rhip.rotation = rhip.rotation * rhip_offset;
        // twist
        Vector3 r_upleg_twist_ori = rhip.rotation * right_upleg_forward;
        Vector3 r_upleg_twist_new = rhip.rotation * right_upleg_forward;
        Vector3.OrthoNormalize(ref right_leg_norm, ref r_upleg_twist_new);
        if (Vector3.Angle(new Vector3(body_dir.x, 0, body_dir.z), new Vector3(r_upleg_twist_new.x, 0, r_upleg_twist_new.z)) > 90)
            r_upleg_twist_new = -r_upleg_twist_new;
        Quaternion r_upleg_twist_rot = Quaternion.FromToRotation(r_upleg_twist_ori, r_upleg_twist_new);
        rhip.rotation = r_upleg_twist_rot * rhip.rotation;

        /// update right foot
        // look at position
        Vector3 new_rfoot_pos = recal_pos(new_rknee_pos, new_rknee_pos + right_low_leg_dir, right_lowleg_len);
        rknee.LookAt(new_rfoot_pos, Vector3.zero);
        rknee.rotation = rknee.rotation * rknee_offset;
        // twist
        Vector3 r_lowleg_twist_ori = rknee.rotation * right_lowleg_forward;
        Vector3 r_lowleg_twist_new = rknee.rotation * right_lowleg_forward;
        Vector3.OrthoNormalize(ref right_foot_norm, ref r_lowleg_twist_new);
        if (Vector3.Angle(new Vector3(body_dir.x, 0, body_dir.z), new Vector3(r_lowleg_twist_new.x, 0, r_lowleg_twist_new.z)) > 90)
            r_lowleg_twist_new = -r_lowleg_twist_new;
        Quaternion r_lowleg_twist_rot = Quaternion.FromToRotation(r_lowleg_twist_ori, r_lowleg_twist_new);
        rknee.rotation = r_lowleg_twist_rot * rknee.rotation;
    }

}