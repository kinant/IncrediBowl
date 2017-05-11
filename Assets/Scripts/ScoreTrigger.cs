﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTrigger : MonoBehaviour {

    private int pinsInTrigger = 0;

    public int numPinsStanding {
        get {
            Debug.Log("Pins still standing: " + pinsInTrigger);
            return pinsInTrigger;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pin")) {
            // Debug.Log("PIN ENTERED");
            pinsInTrigger++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Pin"))
        {
            // Debug.Log("PIN EXIT!");
            pinsInTrigger--;
        }
    }
}

