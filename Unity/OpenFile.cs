using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.IO;
using System.Diagnostics;

public class OpenFile : MonoBehaviour
{
    public static string path;
    //VideoPlayer vplayer;
    public GameObject pln_loadVUI;
    public GameObject pln_beginUI;
    public static bool isselected=false;
    public Text txt_filename;
    public static bool isffmpeged = false;
    string mp4FilePath;
    void Start()
    {
        pln_loadVUI.SetActive(true);
        pln_beginUI.SetActive(false);

       mp4FilePath = @"/videos/kunkun_cutcut.mp4";
        
    }

    public void Openfile()
    {
        path = EditorUtility.OpenFilePanel("Load video", "", "mp4");
        UnityEngine.Debug.Log(path);
        if (path.Length != 0)
        {
            string filename = Path.GetFileName(path);
            txt_filename.text = filename;
            //vplayer.source = VideoSource.Url;
            //vplayer.url = path;
            mp4FilePath = path;
            UnityEngine.Debug.Log("mp4file " + mp4FilePath);
            ff(mp4FilePath);
            isffmpeged = false;
            UnityEngine.Debug.Log("mp4 success");


        }
        else
        {
            isselected = false;
        }
    }
    public void Next()
    {
       
        pln_loadVUI.SetActive(false);
        pln_beginUI.SetActive(true);
        isselected = true;
        isffmpeged = true;
        S1UI.needonnx = true;
    }
    void ff(string filepath)
    {
        Process startInfo = new Process();
        startInfo.StartInfo.FileName = @"D:\AppInstall\ffmpeg\bin\ffmpeg.exe";
        startInfo.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.StartInfo.Arguments = @" -i " + filepath
                             //+ " -r 1"
                             + @" E:\SavedPics\" + Path.GetFileNameWithoutExtension(filepath) + "-%d.png";

        UnityEngine.Debug.Log(startInfo.StartInfo.Arguments);
        startInfo.StartInfo.UseShellExecute = false;
        startInfo.StartInfo.RedirectStandardOutput = true;
        startInfo.StartInfo.RedirectStandardError = true;
        startInfo.StartInfo.CreateNoWindow = true;

        startInfo.Start();
        startInfo.WaitForExit();
        startInfo.Close();
        startInfo.Dispose();
       

    }

}
