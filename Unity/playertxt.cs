using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class playertxt : MonoBehaviour
{
    // Start is called before the first frame update
    Animator animator;
    private int frame = 0;
    private Vector3 initPos;
    private int playRatio = 20;
    private List<List<Vector3>> pose3D = new List<List<Vector3>>();
    private Transform root, spine, chest,neck, head, leye, reye, lshoulder, lelbow, lhand, lthumb2, lmid1, rshoulder, relbow, rhand, rthumb2, rmid1, lhip, lknee, lfoot, ltoe, rhip, rknee, rfoot, rtoe;
    private Quaternion midRoot, midChest,midSpine, midNeck, midHead, midLshoulder, midLelbow, midLhand, midRshoulder, midRelbow, midRhand, midLhip, midLknee, midLfoot, midRhip, midRknee, midRfoot;
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
                //Debug.Log(num);
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
                posvec.Add(new Vector3(-10*tmp[i * 3], -10*tmp[i * 3 + 1], -10*tmp[i * 3 + 2]));
            }
            pose3D.Add(posvec);
        }


        // 动画相关
        animator = this.GetComponent<Animator>();
        /////////////////////////////////////////////////// 骨骼定义 ///////////////////////////////////////////////////
        //躯干
        root = animator.GetBoneTransform(HumanBodyBones.Hips);
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        //chest= animator.GetBoneTransform(HumanBodyBones.Chest);
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        //leye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
        //reye = animator.GetBoneTransform(HumanBodyBones.RightEye);
        //左臂
        lshoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        lelbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        lhand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        lthumb2 = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        lmid1 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        //右臂
        rshoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        relbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rhand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        rthumb2 = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        rmid1 = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
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

        initPos = root.position;
        initPos = new Vector3(root.position.x, 0, root.position.z);
        /////////////////////////////////////////////////// 骨骼中间变换矩阵 ///////////////////////////////////////////////////
        // 当前旋转 = lookforward * 中间矩阵
        // 对于初始姿态，当前旋转就是初始旋转，结合骨骼方向和人体方向，求解各关节的中间矩阵
        Vector3 forward = TriangleNormal(root.position, rhip.position, lhip.position);
        // midLshoulder, midLelbow, midLhand, midRscapular, midRshoulder, midRelbow, midRhand, midLhip, midLknee, midLfoot, midLtoe, midRhip, midRknee, midRfoot, midRtoe        
        // Root
        if (forward != Vector3.zero)
        {
            midRoot = Quaternion.Inverse(root.rotation) * Quaternion.LookRotation(forward);
        }
        else
        {
            Debug.Log("root.position " + root.position);
            Debug.Log("lhip.position " + lhip.position);
            Debug.Log("rhip.position " + rhip.position);
        }
       
        // 躯干
        midSpine = Quaternion.Inverse(spine.rotation) * Quaternion.LookRotation(spine.position - neck.position, forward);
        //midChest = Quaternion.Inverse(chest.rotation) * Quaternion.LookRotation(chest.position - neck.position, forward);
        midNeck = Quaternion.Inverse(neck.rotation) * Quaternion.LookRotation(neck.position - head.position, forward);
        // 头部
        //midHead = Quaternion.Inverse(head.rotation) * Quaternion.LookRotation(nose.position-head.position);
        // 左臂
        midLshoulder = Quaternion.Inverse(lshoulder.rotation) * Quaternion.LookRotation(lshoulder.position - lelbow.position, forward);
        midLelbow = Quaternion.Inverse(lelbow.rotation) * Quaternion.LookRotation(lelbow.position - lhand.position, forward);
        //midLhand = Quaternion.Inverse(lhand.rotation) * Quaternion.LookRotation(
        //    lthumb2.position - lmid1.position,
        //    TriangleNormal(lhand.position, lthumb2.position, lmid1.position)
        //    );
        // 右臂
        midRshoulder = Quaternion.Inverse(rshoulder.rotation) * Quaternion.LookRotation(rshoulder.position - relbow.position, forward);
        midRelbow = Quaternion.Inverse(relbow.rotation) * Quaternion.LookRotation(relbow.position - rhand.position, forward);
        //midRhand = Quaternion.Inverse(rhand.rotation) * Quaternion.LookRotation(
        //    rthumb2.position - rmid1.position,
        //    TriangleNormal(rhand.position, rthumb2.position, rmid1.position)
        //    );
        // 左腿
        midLhip = Quaternion.Inverse(lhip.rotation) * Quaternion.LookRotation(lhip.position - lknee.position, forward);
        midLknee = Quaternion.Inverse(lknee.rotation) * Quaternion.LookRotation(lknee.position - lfoot.position, forward);
        //midLfoot = Quaternion.Inverse(lfoot.rotation) * Quaternion.LookRotation(lfoot.position - ltoe.position, lknee.position-lfoot.position);
        // 右腿
        midRhip = Quaternion.Inverse(rhip.rotation) * Quaternion.LookRotation(rhip.position - rknee.position, forward);
        midRknee = Quaternion.Inverse(rknee.rotation) * Quaternion.LookRotation(rknee.position - rfoot.position, forward);
        //midRfoot = Quaternion.Inverse(rfoot.rotation) * Quaternion.LookRotation(rfoot.position - rtoe.position, rknee.position-rfoot.position);
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
                    Debug.DrawLine(skelpos[i], skelpos[parent[i]], Color.red);
                }
                else
                {
                    Debug.DrawLine(skelpos[i], skelpos[parent[i]], Color.yellow);
                }
            }
        }
    }
    Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }
    // Update is called once per frame
    void updatePose(List<Vector3> pred3D)
    {
        /// rShldrBend 0, rForearmBend 1, rHand 2, rThumb2 3, rMid1 4,
        /// lShldrBend 5, lForearmBend 6, lHand 7, lThumb2 8, lMid1 9,
        /// lEar 10, lEye 11, rEar 12, rEye 13, Nose 14,
        /// rThighBend 15, rShin 16, rFoot 17, rToe 18,
        /// lThighBend 19, lShin 20, lFoot 21, lToe 22,    
        /// abdomenUpper 23,
        /// hip 24, head 25, neck 26, spine 27
        if (frame == 0)
        {
            initPos = new Vector3(pred3D[0].x, -1*pred3D[0].y-(float)0.6, pred3D[0].z);
        }
        /// 
        //////////////////////  更新位置 //////////////////////
        float tallShin = (Vector3.Distance(pred3D[2], pred3D[3]) + Vector3.Distance(pred3D[5], pred3D[6])) / 2.0f;
        float tallThigh = (Vector3.Distance(pred3D[1], pred3D[2]) + Vector3.Distance(pred3D[4], pred3D[5])) / 2.0f;
        float tallUnity = (Vector3.Distance(lhip.position, lknee.position) + Vector3.Distance(lknee.position, lfoot.position)) / 2.0f +
            (Vector3.Distance(rhip.position, rknee.position) + Vector3.Distance(rknee.position, rfoot.position));
        root.position = (pred3D[0] - initPos) * (tallUnity / (tallThigh + tallShin));
        //Debug.Break();

        ////////////////////////  更新旋转 //////////////////////
        //Vector3 forward = TriangleNormal(pred3D[0], pred3D[4], pred3D[1]);
        //// Root
        //root.rotation = Quaternion.LookRotation(forward) * Quaternion.Inverse(midRoot);
        //// 躯干
        //spine.rotation = Quaternion.LookRotation(pred3D[7] - pred3D[9], forward) * Quaternion.Inverse(midSpine);
        ////chest.rotation = Quaternion.LookRotation(pred3D[8] - pred3D[9], forward) * Quaternion.Inverse(midChest);
        //neck.rotation = Quaternion.LookRotation(pred3D[9] - pred3D[10], forward) * Quaternion.Inverse(midNeck);
        //// 头部
        ////head.rotation = Quaternion.LookRotation(pred3D[14] - pred3D[25], TriangleNormal(pred3D[14], pred3D[12], pred3D[10])) * Quaternion.Inverse(midHead);
        //// 左臂
        //lshoulder.rotation = Quaternion.LookRotation(pred3D[11] - pred3D[12], forward) * Quaternion.Inverse(midLshoulder);
        //lelbow.rotation = Quaternion.LookRotation(pred3D[12] - pred3D[13], forward) * Quaternion.Inverse(midLelbow);
        ////lhand.rotation = Quaternion.LookRotation(
        ////    pred3D[8] - pred3D[9]
        ////    TriangleNormal(pred3D[7], pred3D[8], pred3D[9])) * Quaternion.Inverse(midLhand);
        //// 右臂
        //rshoulder.rotation = Quaternion.LookRotation(pred3D[14] - pred3D[15], forward) * Quaternion.Inverse(midRshoulder);
        //relbow.rotation = Quaternion.LookRotation(pred3D[15] - pred3D[16], forward) * Quaternion.Inverse(midRelbow);
        ////rhand.rotation = Quaternion.LookRotation(
        ////    pred3D[3] - pred3D[4],
        ////    TriangleNormal(pred3D[2], pred3D[3], pred3D[4])) * Quaternion.Inverse(midRhand);
        //// 左腿
        //lhip.rotation = Quaternion.LookRotation(pred3D[4] - pred3D[5], forward) * Quaternion.Inverse(midLhip);
        //lknee.rotation = Quaternion.LookRotation(pred3D[5] - pred3D[6], forward) * Quaternion.Inverse(midLknee);
        ////lfoot.rotation = Quaternion.LookRotation(pred3D[21] - pred3D[22], pred3D[20] - pred3D[21]) * Quaternion.Inverse(midLfoot);
        //// 右腿
        //rhip.rotation = Quaternion.LookRotation(pred3D[1] - pred3D[2], forward) * Quaternion.Inverse(midRhip);
        //rknee.rotation = Quaternion.LookRotation(pred3D[2] - pred3D[3], forward) * Quaternion.Inverse(midRknee);
        ////rfoot.rotation = Quaternion.LookRotation(pred3D[17] - pred3D[18], pred3D[16] - pred3D[17]) * Quaternion.Inverse(midRfoot);
        ///

        //////////////////////  更新旋转 //////////////////////
        Vector3 forward = TriangleNormal(pred3D[0], pred3D[1], pred3D[4]);
        // Root
        if (forward != Vector3.zero)
        {
            root.rotation = Quaternion.LookRotation(forward) * Quaternion.Inverse(midRoot);
        }
        else
        {
            Debug.Log("pred3D0 = " + pred3D[0]);
            Debug.Log("pred3d4 = " + pred3D[4]);
            Debug.Log("pred3d1 = " + pred3D[1]);
        }
        
        // 躯干
        spine.rotation = Quaternion.LookRotation(pred3D[7] - pred3D[9], forward) * Quaternion.Inverse(midSpine);
        //chest.rotation = Quaternion.LookRotation(pred3D[8] - pred3D[9], forward) * Quaternion.Inverse(midChest);
        neck.rotation = Quaternion.LookRotation(pred3D[9] - pred3D[10], forward) * Quaternion.Inverse(midNeck);
        // 头部
        //head.rotation = Quaternion.LookRotation(pred3D[14] - pred3D[25], TriangleNormal(pred3D[14], pred3D[12], pred3D[10])) * Quaternion.Inverse(midHead);
        // 左臂
        lshoulder.rotation = Quaternion.LookRotation(pred3D[11] - pred3D[12], forward) * Quaternion.Inverse(midLshoulder);
        lelbow.rotation = Quaternion.LookRotation(pred3D[12] - pred3D[13], forward) * Quaternion.Inverse(midLelbow);
        //lhand.rotation = Quaternion.LookRotation(
        //    pred3D[8] - pred3D[9]
        //    TriangleNormal(pred3D[7], pred3D[8], pred3D[9])) * Quaternion.Inverse(midLhand);
        // 右臂
        rshoulder.rotation = Quaternion.LookRotation(pred3D[14] - pred3D[15], forward) * Quaternion.Inverse(midRshoulder);
        relbow.rotation = Quaternion.LookRotation(pred3D[15] - pred3D[16], forward) * Quaternion.Inverse(midRelbow);
        //rhand.rotation = Quaternion.LookRotation(
        //    pred3D[3] - pred3D[4],
        //    TriangleNormal(pred3D[2], pred3D[3], pred3D[4])) * Quaternion.Inverse(midRhand);
        // 左腿
        lhip.rotation = Quaternion.LookRotation(pred3D[4] - pred3D[5], forward) * Quaternion.Inverse(midLhip);
        lknee.rotation = Quaternion.LookRotation(pred3D[5] - pred3D[6], forward) * Quaternion.Inverse(midLknee);
        //lfoot.rotation = Quaternion.LookRotation(pred3D[21] - pred3D[22], pred3D[20] - pred3D[21]) * Quaternion.Inverse(midLfoot);
        // 右腿
        rhip.rotation = Quaternion.LookRotation(pred3D[1] - pred3D[2], forward) * Quaternion.Inverse(midRhip);
        rknee.rotation = Quaternion.LookRotation(pred3D[2] - pred3D[3], forward) * Quaternion.Inverse(midRknee);
        //rfoot.rotation = Quaternion.LookRotation(pred3D[17] - pred3D[18], pred3D[16] - pred3D[17]) * Quaternion.Inverse(midRfoot);

    }

    void Update()
    {
        List<Vector3> curpos = pose3D[frame / playRatio];
        if (frame % playRatio == 0)
        {
            updatePose(curpos);
        }
       
        if (frame / playRatio < pose3D.Count-1)
            frame = frame + 1;
        else
            frame = 0;
        drawSkel(curpos);
    }
}
