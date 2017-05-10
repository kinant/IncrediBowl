using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            //BaseEventManager.BallReachPit();
            Invoke("Trigger", 1.0f);
        }
    }

    private void Trigger() {
        BaseEventManager.BallReachPit();
    }
}
