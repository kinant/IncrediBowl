using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSetter : MonoBehaviour {

    private Animator animator;
    private bool isFullyDown = false;
    private bool isHoldingPins = false;
    private int _pinsBeingHeld = 0;

    [HideInInspector]
    public int pinsHeld {
        get { return _pinsBeingHeld; }
    }

    public Transform pinsParentTransform;
    public Transform pinUpInitialPosition;
    public GameObject pinsPrefab;

    private List<GameObject> pickUpPins;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pin")) {
            pickUpPins.Add(other.gameObject);
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

    private void PickUpPins() {
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
            pin.transform.parent = pinsParentTransform;
        }
        isHoldingPins = false;
        pickUpPins.Clear();
    }

    public void OnPinSetterUp() {
        // count the number of pins...
        _pinsBeingHeld = transform.childCount;
        Debug.Log("num pins being held: " + _pinsBeingHeld);
    }

    public void InitNewFrame() {
        isHoldingPins = true;
        isFullyDown = false;
    }

    void Start () {
        animator = GetComponent<Animator>();
        pickUpPins = new List<GameObject>();
	}
}
