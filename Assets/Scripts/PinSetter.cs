using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSetter : MonoBehaviour {

    private Animator animator;
    private bool isFullyDown = false;
    private bool isHoldingPins = false;

    public Transform pinsParentTransform;

    private void OnEnable()
    {
        BaseEventManager.OnBallReachedPit += new EventHandler(ActivateSetter);
        BaseEventManager.OnSweeperCompleteSweep += new EventHandler(ActivateSetter);
    }

    private void OnDisable()
    {
        BaseEventManager.OnBallReachedPit -= new EventHandler(ActivateSetter);
        BaseEventManager.OnSweeperCompleteSweep -= new EventHandler(ActivateSetter);
    }

    private void ActivateSetter() {
        animator.SetTrigger("activate");
    }

    public void OnPinSetterDown() {
        isFullyDown = true;
        isHoldingPins = !isHoldingPins;
    }

    public void OnPinSetterUp() {
        isFullyDown = false;

        if (isHoldingPins) {
            BaseEventManager.PinsPickedUp();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isFullyDown) {
            return;
        }

        if (other.gameObject.CompareTag("Pin")) {
            if (isHoldingPins)
            {
                other.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                other.gameObject.transform.parent = transform;
            }
            else
            {
                other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                other.gameObject.transform.parent = pinsParentTransform;
            }
        }
    }

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
	}
}
