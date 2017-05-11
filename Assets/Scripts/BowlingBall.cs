using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingBall : MonoBehaviour {

    private Rigidbody m_rb;

    public float throwForce = 75f;

    private bool hitPin = false;
    private bool start = false;
    private bool isMoving = false;

    private float travelTime = 0.0f;

    private Vector3 startPos;
    private Quaternion startRot;

    private void OnEnable()
    {
        BaseEventManager.OnBallReachedPit += new EventHandler(ResetBall);
    }

    private void OnDisable()
    {
        BaseEventManager.OnBallReachedPit -= new EventHandler(ResetBall);
    }

    // Use this for initialization
    void Start () {
        m_rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void ResetBall() {
        transform.position = startPos;
        transform.rotation = startRot;

        isMoving = false;
        m_rb.velocity = Vector3.zero;
        m_rb.angularVelocity = Vector3.zero;
    }

    private void FireBall() {
        m_rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        isMoving = true;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetMouseButtonDown(0)) {
            if (!isMoving)
            {
                FireBall();
            }
        }
    }
}
