using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour {

    private GameObject playButton;
    private bool startVideo;
    private enum States { STANDBY, PLAYING, PAUSED };
    private States state;
    private VideoPlayer videoPlayer; 

    void Start () {
        state = States.STANDBY;
        videoPlayer = GetComponent<VideoPlayer>();

        playButton = GetComponentInChildren<SpriteRenderer>().gameObject;

        videoPlayer.errorReceived += VideoPlayer_errorReceived;
        videoPlayer.loopPointReached += VideoPlayer_loopPointReached;
        videoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;
        videoPlayer.started += VideoPlayer_started;

        setupVideoState();
    }

    private void VideoPlayer_started(VideoPlayer source)
    {
        Debug.Log("VideoPlayer_started");
        playButton.SetActive(false);
    }

    void Update()
    {
        //if (videoPlayer.isPlaying)
        //{
        //    state = States.PLAYING;
        //    playButton.SetActive(false);
        //} else
        //{
        //    state = States.PAUSED;
        //    playButton.SetActive(true);
        //}
    }

    private void VideoPlayer_prepareCompleted(VideoPlayer source)
    {
        Debug.Log("VideoPlayer_prepareCompleted");
        //if (startVideo)
        //{
        //    videoPlayer.Play();
        //    startVideo = false;
        //}
    }

    private void VideoPlayer_loopPointReached(VideoPlayer source)
    {
        Debug.Log("VideoPlayer_loopPointReached");
        if (!videoPlayer.isLooping)
        {
            playButton.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("play") as Sprite;
            playButton.SetActive(true);
            state = States.STANDBY;
        }
    }

    private void VideoPlayer_errorReceived(VideoPlayer source, string message)
    {
        Debug.Log("VideoPlayer_errorReceived");
        playButton.SetActive(true);
        playButton.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("error") as Sprite;
    }

    public void OnClick()
    {
        Debug.Log("VideoControl_OnClick state="+ state);
        if (state == States.PLAYING)
        {
            videoPlayer.Pause();
            state = States.PAUSED;
            playButton.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("play") as Sprite;
            playButton.SetActive(true);
        }
        else if (state == States.PAUSED || state == States.STANDBY)
        {
            StartVideo();
        }
    }

    public void StartVideo()
    {
        if (state == States.PLAYING)
            return;
        videoPlayer.Play();
        state = States.PLAYING;
        playButton.SetActive(false);
    }

    public void setupVideoState()
    {
        if (videoPlayer == null)
            return;
        if (videoPlayer.playOnAwake)
        {
            state = States.PLAYING;
            playButton.SetActive(false);
        }
        else
        {
            state = States.STANDBY;
            playButton.SetActive(true);
        }
    }

    public void OnMarkerFound()
    {
        setupVideoState();
    }
}
