using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeScreenMoviePlayer : MonoBehaviour {

    public string strikeVideoFileName;
    public string spareVideoFileName;
    
    public enum VideoType {
        Strike, Spare
    }

    private MediaPlayerCtrl mediaPlayerCtrl;

    private bool shouldPlayVideo = false;

    private void OnEnable()
    {
        mediaPlayerCtrl.OnReady += HandleMediaPlayerReady;
        mediaPlayerCtrl.OnEnd += HandleMediaPlayerEnd;
    }

    private void OnDisable()
    {
        mediaPlayerCtrl.OnReady -= HandleMediaPlayerReady;
        mediaPlayerCtrl.OnEnd -= HandleMediaPlayerEnd;

    }

    private void HandleMediaPlayerReady()
    {
        if (shouldPlayVideo)
        {
            mediaPlayerCtrl.Play();
        }
    }

    private void HandleMediaPlayerEnd()
    {
        mediaPlayerCtrl.Stop();
        shouldPlayVideo = false;
    }

    private void Awake()
    {
        mediaPlayerCtrl = GetComponent<MediaPlayerCtrl>();
    }

    public void PlayVideo(VideoType type) {

        shouldPlayVideo = true;

        mediaPlayerCtrl.Stop();

        if (type == VideoType.Strike)
        {
            mediaPlayerCtrl.Load(strikeVideoFileName);
        }
        else {
            mediaPlayerCtrl.Load(spareVideoFileName);
        }

    }
}
