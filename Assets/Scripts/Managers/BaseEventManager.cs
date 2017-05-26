using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void EventHandler();

// this class is used as a general event manager
// intially my game worked with many events, but later on I improved the code and removed all but one of the events
public class BaseEventManager : MonoBehaviour {

    // an event handler for when the ball reaches the pit
    public static event EventHandler OnBallReachedPit;

    // fire the the event
    public static void BallReachPit() {
        if (OnBallReachedPit != null) {
            OnBallReachedPit();
        }
    }
}
