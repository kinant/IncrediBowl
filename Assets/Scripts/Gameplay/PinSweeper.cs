using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is the class/scrip for the pin sweeper
// the pin sweeper is in charge of sweeping dropped pins or pins at the end of the frame into the pit
public class PinSweeper : MonoBehaviour {

    // we will cache the animator
    private Animator animator;

    private void Start()
    {
        // get the animator component
        animator = GetComponent<Animator>();
    }

    // this function is called when we want to sweeper to animate up
    public void SweeperUp() {
        SetAnimatorTrigger("up");
    }

    // this function is called when we want the sweeper to sweep
    public void SweeperSweep() {
        SetAnimatorTrigger("sweep");
    }

    // this function is called when we want the sweeper to unsweep
    public void SweeperUnsweep() {
        SetAnimatorTrigger("unsweep");
    }

    // this function is called when we want the sweeper to come down
    public void SweeperDown() {
        SetAnimatorTrigger("down");
    }

    // all the previous functions set the relevant animation trigger.
    // we use this function to do so.
    private void SetAnimatorTrigger(string triggerName) {
        // set the relevant trigger to trigger the sweeper animation
        animator.SetTrigger(triggerName);
    }
}
