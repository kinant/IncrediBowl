using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles the pin setter
// the pin setter is the machine that picks up and drops pins and also sets new pins 
public class PinSetter : MonoBehaviour {
    public Transform pinsParentTransform; // the parent transform for the pins

    private Animator animator; // cache the animator
    private bool isFullyDown = false; // a flag to tell us if the pin setter is fully down
    private bool isHoldingPins = false; // a flag to tell us if the pin setter is holding pins

    private List<GameObject> pickUpPins; // a list of pins to pick up

    // pin pickup is handled by a trigger attached to the pin setter
    // if the pin enters the trigger, it is added to the list of pins to be picked up later on
    private void OnTriggerEnter(Collider other)
    {
        // we check if a pin has entered the trigger
        if (other.gameObject.CompareTag("Pin")) {
            // add it to the list of pins to pick up
            pickUpPins.Add(other.gameObject);
        }
    }

    // handle objects leaving the trigger
    private void OnTriggerExit(Collider other)
    {
        // if a pin has exited the trigger, then we remove it from the list
        // this usually occurs when the pin is knocked down
        if (other.gameObject.CompareTag("Pin"))
        {
            pickUpPins.Remove(other.gameObject);
        }
    }

    // activates the trigger for the pin setter animation
    public void ActivateSetter() {
        // activate the "activate" trigger which makes the pin setter drop down, wait, and go back up
        animator.SetTrigger("activate");
    }

    // this function is called from the animation (as an animation event) when the pin setter is fully down. 
    public void OnPinSetterDown() {
        // if the pin setter is not holding pins, then that means we have to pick them up
        if (!isHoldingPins)
        {
            PickUpPins();
        }
        // else, the pin setter is holding pins, so we drop them...
        else
        {
            ReleasePins();
        }
    }

    // this function is called to tell the pin setter to pick up the pins
    public void PickUpPins() {

        // we iterate through each pin in the list of pins that are set to be picked up
        foreach (GameObject pin in pickUpPins) {

            // we set the pins rigidbody is kinematic to true
            pin.GetComponent<Rigidbody>().isKinematic = true;

            // set the pins parent to this object's transform. That way, when the pin setter moves, the pins
            // move along with it
            pin.transform.parent = transform;
        }

        // set the flag
        isHoldingPins = true;
    }

    // this function is called when the pin setter has to release the pins that it is holding
    private void ReleasePins() {

        // iterate over each pin that was picked up
        foreach (GameObject pin in pickUpPins)
        {
            // we set pin rigidbody is kinematic to false, and re-enable use gravity
            pin.GetComponent<Rigidbody>().isKinematic = false;
            pin.GetComponent<Rigidbody>().useGravity = true;

            // we reset the pins parent
            pin.transform.parent = pinsParentTransform;
        }

        // set up the flag
        isHoldingPins = false;

        // clear the list of pins to pick up
        pickUpPins.Clear();
    }

    void Start () {
        // get animator component and init the list
        animator = GetComponent<Animator>();
        pickUpPins = new List<GameObject>();
	}
}
