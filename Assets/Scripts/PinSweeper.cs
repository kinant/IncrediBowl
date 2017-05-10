using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinSweeper : MonoBehaviour {

    private Animator animator;

    private void OnEnable()
    {
       BaseEventManager.OnPinsPickedUp += new EventHandler(ActivateSweeper);
    }

    private void OnDisable()
    {
        BaseEventManager.OnPinsPickedUp -= new EventHandler(ActivateSweeper);
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void ActivateSweeper()
    {
        StartCoroutine(AnimateSweeper());
    }

    IEnumerator AnimateSweeper() {
        // sweep
        animator.SetTrigger("sweep");
        yield return new WaitForSeconds(3.0f);
        // wait a little
        yield return new WaitForSeconds(0.25f);
        // unsweep
        animator.SetTrigger("unsweep");
        yield return new WaitForSeconds(3.0f);
        OnSweepComplete();
    }

    public void OnSweepComplete() {
        BaseEventManager.SweepComplete();
    }



}
