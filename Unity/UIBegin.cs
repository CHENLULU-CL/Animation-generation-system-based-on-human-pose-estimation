using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class UIBegin : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject videoplayer;
    private VideoPlayer vp;
    private RawImage rawimage;
    public GameObject pln_BgeinUI;
    public GameObject pln_PlayingUI;
    public Slider slider3d;
    public GameObject textLoading;
    public GameObject iconLoaded;
    public GameObject iconMain;
    //private bool isloaded=false;
    //private float percet = 0;//需要从tryonnx获得,进度
    void Start()
    {
        vp = videoplayer.GetComponent<VideoPlayer>();
        rawimage = videoplayer.GetComponent<RawImage>();
        pln_BgeinUI.SetActive(false);
        pln_PlayingUI.SetActive(false);
      
        
    }
    void Update()
    {
        //slider2d.value = tryonnx.processing2d;
        slider3d.value = tryonnx.processing3d;
        rawimage.texture = vp.texture;


        //if (tryonnx.isloaded)
        //{
        //    //SceneManager.LoadScene("SampleScene");
        //    textLoading.SetActive(false);
        //    iconLoaded.SetActive(true);
        //    //StartCoroutine("OnWait",5);
        //    //pln_BgeinUI.SetActive(false);
        //    //pln_PlayingUI.SetActive(true);
        //}
        if (S1UI.needonnx)
        {
            //SceneManager.LoadScene("SampleScene");
            textLoading.SetActive(true);
            iconLoaded.SetActive(false);
            iconMain.SetActive(false);
            //StartCoroutine("OnWait",5);
            //pln_BgeinUI.SetActive(false);
            //pln_PlayingUI.SetActive(true);
        }
        else
        {
            textLoading.SetActive(false);
            iconLoaded.SetActive(true);
            iconMain.SetActive(true);
        }

    }
    public void BtnNext()
    {
        //SceneManager.LoadScene("Main");
        pln_BgeinUI.SetActive(false);
        pln_PlayingUI.SetActive(true);
    }

    public void BtnNextMain()
    {
        SceneManager.LoadScene("Main");
        //pln_BgeinUI.SetActive(false);
        //pln_PlayingUI.SetActive(true);
    }
}
