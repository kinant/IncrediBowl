using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class handles the large screen movie player
// It plays movies when a strike or spare is scored
public class LargeScreenMoviePlayer : MonoBehaviour {

    public string strikeVideoFileName; // reference to the strike video
    public string spareVideoFileName; // reference to the spare video
    
    // an enum that can be used to choose what video to play
    public enum VideoType {
        Strike, Spare
    }

    // reference to the Easy Movie Texture media player control script
    private MediaPlayerCtrl mediaPlayerCtrl;

    // a flag that tells us if a video should be played (once a video is ready)
    private bool shouldPlayVideo = false;

    // set the event references + handlers
    private void OnEnable()
    {
        mediaPlayerCtrl.OnReady += HandleMediaPlayerReady;
        mediaPlayerCtrl.OnEnd += HandleMediaPlayerEnd;
    }

    // disable the event references + handlers
    private void OnDisable()
    {
        mediaPlayerCtrl.OnReady -= HandleMediaPlayerReady;
        mediaPlayerCtrl.OnEnd -= HandleMediaPlayerEnd;

    }

    // This function will be called in the event that the media player is ready to play a video (once it has been loaded)
    private void HandleMediaPlayerReady()
    {
        // check if we should play a video
        if (shouldPlayVideo)
        {
            // if so, tell the media player controller to play it
            mediaPlayerCtrl.Play();
        }
    }

    // This function is called in the event that a video finishes playing
    private void HandleMediaPlayerEnd()
    {
        // stop the media player
        mediaPlayerCtrl.Stop();

        // set the flag so that we do not play a video again
        shouldPlayVideo = false;
    }

    private void Awake()
    {
        // get the media player controller component
        mediaPlayerCtrl = GetComponent<MediaPlayerCtrl>();
    }

    // public function that is called by other scripts to instruct the movie player to play a video
    public void PlayVideo(VideoType type) {

        // we set the flag to tell the media player controller to play once the video is loaded
        shouldPlayVideo = true;

        // we stop any currently playing video
        mediaPlayerCtrl.Stop();

        // check what type of video we want to play, and then load it to the media player controller
        if (type == VideoType.Strike)
        {
            mediaPlayerCtrl.Load(strikeVideoFileName);
        }
        else {
            mediaPlayerCtrl.Load(spareVideoFileName);
        }

    }
}
