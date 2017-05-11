using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSweeper : MonoBehaviour {

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SweeperUp() {
        SetAnimatorTrigger("up");
    }

    public void SweeperSweep() {
        SetAnimatorTrigger("sweep");
    }

    public void SweeperUnsweep() {
        SetAnimatorTrigger("unsweep");
    }

    public void SweeperDown() {
        SetAnimatorTrigger("down");
    }

    private void SetAnimatorTrigger(string triggerName) {
        animator.SetTrigger(triggerName);
    }
}
