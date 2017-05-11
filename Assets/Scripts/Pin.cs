using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour {
    private Vector3 startPos;
    private Rigidbody rb;
    private Collider col;

    private void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<MeshCollider>();
    }

    public void ResetPin() {
        //rb.enabled = false;
        col.enabled = false;
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
       
        // rb.velocity = Vector3.zero;
        //rb.angularVelocity = Vector3.zero;
       // rb.isKinematic = true;
    }
}
