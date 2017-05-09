using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour {

    private Rigidbody m_rb;

	// Use this for initialization
	void Awake () {

	}

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_rb.isKinematic = false;

    }

    // Update is called once per frame
    void Update () {
        m_rb.velocity = Vector3.zero;
        m_rb.angularVelocity = Vector3.zero;
    }
}
