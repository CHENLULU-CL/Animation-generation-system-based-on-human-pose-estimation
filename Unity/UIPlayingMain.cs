using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPlayingMain : MonoBehaviour
{
    //public GameObject pln_record;
    public Button btn_exit;
    //public Button btn_record;
    //public GameObject pln_loading;
    //public GameObject pln_playing;
    // Start is called before the first frame update
    void Start()
    {
        //pln_record.SetActive(false);
        //pln_FinishUI.SetActive(false);
        //pln_PlayingUI.SetActive(false);

        //m_audioSource = m_videoPlayer.GetComponent<AudioSource>();
        //btn_record.onClick.AddListener(Fun_Record);
        btn_exit.onClick.AddListener(Fun_Exit);
        //cb = new ColorBlock();
        //pln_loading.SetActive(false);
        //vplayer = video.GetComponent<VideoPlayer>();
        //m_recordCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        //btn_recored.onClick.AddListener(Fun_disp);

    }
    public void Fun_Exit()
    {
        //pln_loading.SetActive(true);
        //pln_record.SetActive(false);
        //pln_playing.SetActive(false);
        SceneManager.LoadScene("Scene1");

        //pln_PlayingUI.SetActive(false);
        //pln_FinishUI.SetActive(true);
        //vplayer.Stop();
    }
    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
