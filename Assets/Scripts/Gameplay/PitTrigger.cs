using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script for the pit trigger. Basically the pit trigger is an area at the end of the bowling lane.
// We use it to detect when the ball has reached the end of the lane
public class PitTrigger : MonoBehaviour {

    // handle collisions with the trigger
    private void OnTriggerEnter(Collider other)
    {
        // check that the ball has entered the trigger
        if (other.gameObject.CompareTag("Ball"))
        {
            // if so, we wait a second and invoke the trigger function
            Invoke("Trigger", 1.0f);
        }
    }

    // this function is called a second after the ball enters the pit trigger
    // it basically just tells the base event manager that the ball has entered the pit
    private void Trigger() {
        BaseEventManager.BallReachPit();
    }
}
