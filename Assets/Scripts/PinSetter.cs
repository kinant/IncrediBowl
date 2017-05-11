using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSetter : MonoBehaviour {

    public Transform pinsParentTransform;

    private Animator animator;
    private bool isFullyDown = false;
    private bool isHoldingPins = false;

    private List<GameObject> pickUpPins;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pin")) {
            pickUpPins.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Pin"))
        {
            pickUpPins.Remove(other.gameObject);
        }
    }

    public void ActivateSetter() {
        animator.SetTrigger("activate");
    }

    public void OnPinSetterDown() {
        // pick up the pins...
        if (!isHoldingPins)
        {
            PickUpPins();
        }
        // or drop the pins...
        else
        {
            ReleasePins();
        }
    }

    public void PickUpPins() {
        foreach (GameObject pin in pickUpPins) {
            pin.GetComponent<Rigidbody>().isKinematic = true;
            pin.transform.parent = transform;
        }
        isHoldingPins = true;
    }

    private void ReleasePins() {
        foreach (GameObject pin in pickUpPins)
        {
            pin.GetComponent<Rigidbody>().isKinematic = false;
            pin.GetComponent<Rigidbody>().useGravity = true;
            pin.transform.parent = pinsParentTransform;
        }
        isHoldingPins = false;
        pickUpPins.Clear();
    }

    public void InitNewFrame() {
        // ActivateSetter();
    }

    void Start () {
        animator = GetComponent<Animator>();
        pickUpPins = new List<GameObject>();
	}
}
