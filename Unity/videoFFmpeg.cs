using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class videoFFmpeg : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool isffmpeged=false;
    void Start()
    {
        string mp4FilePath = @"/videos/kunkun_cutcut.mp4";
        mp4FilePath = OpenFile.path;
        UnityEngine.Debug.Log("mp4file " + mp4FilePath);
        ff(mp4FilePath);
        isffmpeged = false;
        UnityEngine.Debug.Log("mp4 success");
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    void ff(string filepath)
    {
        Process startInfo = new Process();
        startInfo.StartInfo.FileName = @"D:\AppInstall\ffmpeg\bin\ffmpeg.exe";
        startInfo.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.StartInfo.Arguments = @" -i "+filepath
                             + " -r 5"
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
        isffmpeged = true;
       
    }
}
