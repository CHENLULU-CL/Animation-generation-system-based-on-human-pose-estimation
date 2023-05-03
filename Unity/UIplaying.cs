using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using UnityEngine.Video;


public class UIplaying : MonoBehaviour
{
    // Start is called before the first frame update
    //public GameObject pln_FinishUI;
    //public GameObject pln_PlayingUI;

    public GameObject pln_record;
    public Button btn_exit;
    public Button btn_record;
    public GameObject pln_loading;
    public GameObject pln_playing;
    
    static bool isrecord;
    public Button btn_recored;

    //public GameObject m_videoPlayer;
    private AudioSource m_audioSource;
    private Camera m_recordCamera;

    private MP4Recorder recorder;
    private IClock clock;
    private CameraInput m_cameraInput;
    private AudioInput m_audioInput;
    public Sprite m_cord;
    public Sprite m_dcord;
    private string m_resultPath;

    //private Color32[] pixelBuffer;
    ColorBlock cb;

    //public RawImage video;
    //VideoPlayer vplayer;

    void Start()
    {
        pln_record.SetActive(false);
           //pln_FinishUI.SetActive(false);
           //pln_PlayingUI.SetActive(false);
           isrecord = false;
        //m_audioSource = m_videoPlayer.GetComponent<AudioSource>();
        btn_record.onClick.AddListener(Fun_Record);
        btn_exit.onClick.AddListener(Fun_Exit);
        cb = new ColorBlock();
        pln_loading.SetActive(false);
        //vplayer = video.GetComponent<VideoPlayer>();
        m_recordCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        btn_recored.onClick.AddListener(Fun_disp);
        
    }
    public void Fun_disp()
    {
        pln_record.SetActive(false);
    }

    // Update is called once per frame
    public void Fun_Exit()
    {
        pln_loading.SetActive(true);
        pln_record.SetActive(false);
        pln_playing.SetActive(false);

        //pln_PlayingUI.SetActive(false);
        //pln_FinishUI.SetActive(true);
        //vplayer.Stop();
    }
    public void Fun_Record()
    {
        if (isrecord)
        {
            btn_record.GetComponent<Image>().sprite = m_dcord;
            isrecord = false;
            
            //结束录像
            StopRecording();
            pln_record.SetActive(true);
            btn_recored.onClick.AddListener(Fun_disp);
        }
        else
        {
            btn_record.GetComponent<Image>().sprite = m_cord;
            //开始录像
            isrecord = true;
            StartRecording();
        }
    }
    void StartRecording()
    {
        cb.normalColor = Color.red;
        cb.highlightedColor = Color.yellow;
        cb.selectedColor = Color.red;
        cb.pressedColor = Color.yellow;
        cb.colorMultiplier = 1;
        btn_record.colors = cb;


        clock = new RealtimeClock();
        recorder = new MP4Recorder(1024, 768, 30,  48000, 2);
        m_cameraInput = new CameraInput(recorder, clock, m_recordCamera);
        //m_audioInput = new AudioInput(recorder, clock, m_audioSource);
    }

    async void StopRecording()
    {
       // Stop recording
        cb.normalColor = Color.yellow;
        cb.pressedColor = Color.red;
        cb.highlightedColor = Color.red;
        cb.selectedColor = Color.yellow;
        cb.colorMultiplier = 1;
        btn_record.colors = cb;

        //m_audioInput.Dispose();
        m_cameraInput.Dispose();
        var path = await recorder.FinishWriting();
        // Playback recording
        //Debug.Log(m_recordCamera.pixelWidth);
        Debug.Log($"Saved recording to: {path}");
        Handheld.PlayFullScreenMovie($"file://{path}");
    }
}
