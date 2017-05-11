using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour {
    private Rigidbody rb;
    private Collider col;
    private Quaternion startRot;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<MeshCollider>();
        startRot = transform.rotation;
    }

    public void ResetPin() {
        // col.enabled = false;
        rb.useGravity = false;
        rb.isKinematic = true;
        transform.rotation = startRot;
    }
}
