using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            Invoke("Trigger", 1.0f);
        }
        else if (other.gameObject.CompareTag("Pin"))
        {
            GameManager.Instance.PinDown();
            other.gameObject.SetActive(false);
        }
    }

    private void Trigger() {
        BaseEventManager.BallReachPit();
    }
}
