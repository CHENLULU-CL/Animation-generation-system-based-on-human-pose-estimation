# Animation-generation-system-based-on-human-pose-estimation
**Use 3D human pose estimation to animate characters in Unity3D**

There are typically two main approaches to using human pose estimation to create animations. One approach involves directly using algorithms to generate humanoid animations based on the obtained body pose data. However, these animations often lack aesthetic design, have limited scalability, and are inconvenient for previewing the final animation results. 

The other approach involves using a pre-designed skeleton model of the human body, and controlling its joint movements based on pose data to generate animations. This approach combines the advantages of pose estimation technology and artistic design, allowing users to directly experience the effects of the final animation. It is more efficient than the previous approach, and changing the skeleton model is more convenient, making it more suitable for animation production. 

In current research, human skeleton models and pose estimation models are usually implemented on different platforms and languages. The methods mostly rely on TCP communication or local file sharing to transfer the output data of the pose estimation model to the skeleton model, making it impossible to generate animations directly on a single platform.

This work aims to apply the current state-of-the-art 3D pose estimation model directly to the skeleton model, resulting in an integrated system that generates animations from video without the need for communication between separate processes. Specifically, we use an off-the-shelf 3D human pose estimation model trained by PyTorch, and export it to Unity using model migration. By estimating the changes of each human joint position in the video and calculating the rotation vector, we control the generation of character animations in Unity.

## Overview 


## 3D Human Pose Estimation
We employ simple but effective technologies for human pose estimation, namely SimpleBaseline and VideoPose3D. SimpleBaseline estimates 2D poses from images, while VideoPose3D generates 3D poses from the 2D ones. These technologies work separately but in tandem to achieve accurate pose estimation.

## Porting Model to Unity
This work utils the ONNX to export trained machine learning models from pytorch and import it into Unity for greater flexibility and efficiency in animation generation. We also tried OpencvSharp and Barracuda, but failed becuause these technologis were not mature enough at that time.
