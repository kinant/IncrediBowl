using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void EventHandler();

public class BaseEventManager : MonoBehaviour {

    public static event EventHandler OnBallReachedPit;
    public static event EventHandler OnPinReachedPit;
    public static event EventHandler OnSweeperCompleteSweep;
    public static event EventHandler OnPinsPickedUp;

    public static void BallReachPit() {
        if (OnBallReachedPit != null) {
            OnBallReachedPit();
        }
    }

    public static void SweepComplete() {
        if (OnSweeperCompleteSweep != null) {
            OnSweeperCompleteSweep();
        }
    }

    public static void PinsPickedUp() {
        if (OnPinsPickedUp != null) {
            OnPinsPickedUp();
        }
    }
}
