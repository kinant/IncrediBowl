using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingBall : MonoBehaviour {

    private Rigidbody m_rb;

    public float throwForce = 75f;

    private bool hitPin = false;
    private bool start = false;

    private float travelTime = 0.0f;

	// Use this for initialization
	void Start () {
        m_rb = GetComponent<Rigidbody>();
        m_rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        
    }
	
	// Update is called once per frame
	void Update () {
        if (!start) {
            Debug.Log("Ball start velocity: " + m_rb.velocity);
            start = true;
        }
        travelTime += Time.deltaTime;
        // Debug.Log("Ball velocity: " + m_rb.velocity);
	}

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ball approach velocity: " + m_rb.velocity);
        Debug.Log("Travel time: " + travelTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pin")) {
            if (!hitPin)
            {
                Debug.Log("Ball hit velocity: " + m_rb.velocity);
                Debug.Log("Travel time: " + travelTime);
                hitPin = true;
            }
        }
    }

}
