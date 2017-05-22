using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeScreenMoviePlayer : MonoBehaviour {

    private MediaPlayerCtrl mediaPlayerCtrl;

	// Use this for initialization
	void Start () {

        mediaPlayerCtrl = GetComponent<MediaPlayerCtrl>();

        if (mediaPlayerCtrl != null) {
            mediaPlayerCtrl.Play();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
