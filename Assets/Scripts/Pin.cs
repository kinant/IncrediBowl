using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour {
    private Vector3 startPos;
    public bool isHeldByPinSetter = false;

    private void Start()
    {
        startPos = transform.position;
    }

    public void ResetPin() {
        transform.position = startPos;
    }
}
