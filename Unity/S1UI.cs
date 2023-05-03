using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class S1UI : MonoBehaviour
{
    public Button btn_enter;
    public float rotateSpeed;
    private VideoPlayer vplayer;
    private RawImage vshow;
    public GameObject video;
    public static bool  needonnx=false;
    // Start is called before the first frame update
    private void Awake()
    {
        needonnx = false;
    }
    void Start()
    {
        rotateSpeed = 0.3f;
        vplayer = video.GetComponent<VideoPlayer>();
        vshow = video.GetComponent<RawImage>();
    }
    private void Update()
    {
        btn_enter.transform.Rotate(Vector3.forward * rotateSpeed, Space.World);
        vshow.texture = vplayer.texture;
    }
}
