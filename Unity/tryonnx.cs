using System;
using System.IO;
using System.Collections;
//using OpenCvSharp;
//using OpenCvSharp.Dnn;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Linq;
using System.Threading;


public class tryonnx : MonoBehaviour
{
    // model
    InferenceSession session2d;
    InferenceSession session3dTraj;
    InferenceSession session3dPose;

    public static Tensor<float> output3D;
    public static bool isloaded = false;
    public static float processing3d = 0;

    //video
    private int targetWidth2dInput, targetHeight2dInput;
    private int targetWidth2dOutput, targetHeight2dOutput;
    private int rawImageWidth, rawImageHeight;
    public static int imagesNum = 0;
    bool first = true;
    Thread predictThread;
    bool isabort = true;
    int w = 1000;
    int h = 1002;
    void Start()
    {
        if (S1UI.needonnx)
        {
            isloaded = false;
        }
        else
        {
            isloaded = true;
        }
        isloaded = false;
        targetWidth2dInput = 288;
        targetHeight2dInput = 384;
        targetWidth2dOutput = 72;
        targetHeight2dOutput = 96;
        string model3dPoseFilePath = Application.dataPath + @"\models\pt_243_dynamic.onnx";
        string model3dTrajFilePath = Application.dataPath + @"\models\243_traj_dynamic.onnx";
        string model2dFilePath = Application.dataPath + @"\models\pose_resnet_152_384x288.onnx";
        Debug.Log("loading onnx");
        session2d = new InferenceSession(model2dFilePath );
        session3dTraj = new InferenceSession(model3dTrajFilePath );
        session3dPose = new InferenceSession(model3dPoseFilePath);
        Debug.Log("onnx load success");
        predictThread = new Thread(new ThreadStart(Predict));
        Debug.Log("thread load success");
    }

    private void Update()
    {

            if (first)
            {
                if (OpenFile.isffmpeged && OpenFile.isselected && isabort == true)
                {
                    //Predict();
                    isabort = false;
                    predictThread.Start();
                    Debug.Log("thread start");
                    OpenFile.isffmpeged = false;
                    first = false;

                }
            }
            if (isloaded && isabort == false)
            {
                S1UI.needonnx = false;
                predictThread.Abort();
                Debug.Log("thread abort");
                isabort = true;
            }

    }


    public List<String> GetImagesPath(String folderName)
    {

        DirectoryInfo Folder;
        FileInfo[] Images;

        Folder = new DirectoryInfo(folderName);
        Images = Folder.GetFiles();
        List<String> imagesList = new List<String>();
        //Debug.Log("loading imagespaths");
        //for (int i = 0; i < Images.Length; i++)
        //{
        //    imagesList.Add(String.Format(@"{0}/{1}", folderName, Images[i].Name));
        //    // Console.WriteLine(String.Format(@"{0}/{1}", folderName, Images[i].Name));
        //    print(Images[i].Name);
        //}
        string filepath = OpenFile.path;
        string mp4name = Path.GetFileNameWithoutExtension(filepath);
        for (int i = 1; i <=Images.Length; i++)
        {
            imagesList.Add(String.Format(@"{0}/{1}-{2}.png",folderName,mp4name,i));
            //Debug.Log(String.Format(@"{0}/{1}-{2}.png", folderName, mp4name, i));
        }

        return imagesList;
    }
    void  Predict()
    {

        string imageFolder = @"E:\SavedPics";
        List<String> imagesPath = GetImagesPath(imageFolder);
        imagesNum = imagesPath.Count;
        Debug.Log("imagenum" + imagesNum);
        int inputs3dChannels = imagesNum + 242;
        Tensor<float> inputs3D = new DenseTensor<float>(new[] { 1, inputs3dChannels, 17, 2 });

        int padding = 121;
        var mean = new[] { 102.9801f, 115.9465f, 122.7717f };

        for (int i = 0; i < imagesNum; i++)
        {
            string imagePath = imagesPath[i];
            Image<Rgb24> image = SixLabors.ImageSharp.Image.Load<Rgb24>(imagePath);
            rawImageHeight = image.Height;
            rawImageWidth = image.Width;
            //resize
            image.Mutate(x => x.Resize(targetWidth2dInput, targetHeight2dInput));
            //input
            Tensor<float> inputs2Dimage = new DenseTensor<float>(new[] { 1, 3, targetHeight2dInput, targetWidth2dInput });

            for (int y = 0; y < targetHeight2dInput; y++)
            {
                Span<Rgb24> pixelSpan = image.GetPixelRowSpan(y);
                for (int x = 0; x < targetWidth2dInput; x++)
                {
                    inputs2Dimage[0, 0, y, x] = pixelSpan[x].B/(float)255.0;
                    inputs2Dimage[0, 1, y, x] = pixelSpan[x].G/(float)255.0;
                    inputs2Dimage[0, 2, y, x] = pixelSpan[x].R/(float)255.0;
                }
            }

            var input2d = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input.1", inputs2Dimage)
            };
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results2d = session2d.Run(input2d);
            var results2dArray = results2d.ToArray();
            var result2d = results2dArray.AsEnumerable().ElementAt(0).AsTensor<float>();
            for (int j = 0; j < 17; j++)
            {
                int maxX = 0, maxY = 0, X = 0, Y = 0;
                float prob = 0;
                for (int b = 0; b < targetHeight2dOutput; b++)
                {

                    for (int a = 0; a < targetWidth2dOutput; a++)
                    {
                        float probnow = result2d[0, j, b, a];
                        if (prob < probnow)
                        {
                            prob = probnow;
                            maxX = a;
                            maxY = b;
                           
                        }
                    }
                }
                X = (rawImageWidth * maxX) / targetWidth2dOutput;
                Y = (rawImageHeight * maxY) / targetHeight2dOutput;
                //find the max xy location
                inputs3D[0, padding + i, j, 0] = X/(float)500.0-(float)1;
                inputs3D[0, padding + i, j, 1] = Y/(float)500.0-(float)1.002;
                //Debug.Log("x: " + inputs3D[0, padding + i, j, 0] + ", " + "y: " + inputs3D[0, padding + i, j, 1]);
            }
            processing3d = i /(1.25f*imagesNum);
        }
        //predict 3d
        output3D = new DenseTensor<float>(new[] { 1, imagesNum, 17, 3 });
  
        var inputs3dPose = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("inputs",inputs3D)
        };
        IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results3dPose = session3dPose.Run(inputs3dPose);
        var results3dPoseArray = results3dPose.ToArray();
        var results3dPoseTensor = results3dPoseArray.AsEnumerable().ElementAt(0).AsTensor<float>();
        //traj
        var inputs3dTraj = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("inputs",inputs3D)
        };
        IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results3dTraj = session3dTraj.Run(inputs3dPose);
        var results3dTrajArray = results3dTraj.ToArray();
        var results3dTrajTensor = results3dTrajArray.AsEnumerable().ElementAt(0).AsTensor<float>();

        //add    results3dPoseTensor    results3dTrajTensor  to output3D
        for (int c = 0; c < imagesNum; c++)
        {
            for (int j = 0; j < 17; j++)
            {
                output3D[0, c, j, 0] = results3dPoseTensor[0, c, j, 0] + results3dTrajTensor[0, c, 0, 0];
                output3D[0, c, j, 1] = results3dPoseTensor[0, c, j, 1] + results3dTrajTensor[0, c, 0, 1];
                output3D[0, c, j, 2] = results3dPoseTensor[0, c, j, 2] + results3dTrajTensor[0, c, 0, 2];
                Debug.Log("x: " + output3D[0, c, j, 0] + ", " + "y: " + output3D[0, c, j, 1] + "z: " + output3D[0, c, j, 2]);
            }
        }

        //for (int c = 0; c < imagesNum; c++)
        //{
        //    for (int j = 0; j < 17; j++)
        //    {
        //        output3D[0, c, j, 0] = results3dPoseTensor[0, c, j, 0];
        //        output3D[0, c, j, 1] = results3dPoseTensor[0, c, j, 1];
        //        output3D[0, c, j, 2] = results3dPoseTensor[0, c, j, 2];
        //    }

        //}
        //save();
        processing3d = 1;
        isloaded = true;
    }

    void save()
    {
        string jsonData = JsonUtility.ToJson(output3D);
        PlayerPrefs.SetString("mp4", jsonData);
        PlayerPrefs.Save();
    }
    private void OnDestroy()
    {
        if(predictThread.IsAlive)
        {
            predictThread.Abort();
            Debug.Log("thread abort");
            isabort = true;
        }
    }
}



