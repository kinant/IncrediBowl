using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script handles the score trigger. 
// we use a box collider around the top of the region where the pins are to track the score. 
// when a pin is dropped, it leaves this trigger, and we keep track of this.
// The game manager can then use this information to know how many pins were dropped on each throw
public class ScoreTrigger : MonoBehaviour {

    // keep track of pins inside the trigger
    private int pinsInTrigger = 0;

    // we let other scripts access the number of pins still standing inside the trigger
    public int numPinsStanding {
        get {
            return pinsInTrigger;
        }
    }

    // called when a collider enters the trigger area
    private void OnTriggerEnter(Collider other)
    {
        // if we have a pin inside, then we increase the count
        if (other.gameObject.CompareTag("Pin")) {
            pinsInTrigger++;
        }
    }

    // called when a collider exits the trigger
    private void OnTriggerExit(Collider other)
    {
        // if a pin has left the trigger area (it was knocked down or sweeped away), we reduce the count
        if (other.gameObject.CompareTag("Pin"))
        {
            pinsInTrigger--;
        }
    }
}

