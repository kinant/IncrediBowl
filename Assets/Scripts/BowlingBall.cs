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

    public Transform ballStartTransform;

    private Vector3 startPos;
    private Quaternion startRot;

    private bool ballInPit = false;
    private bool ballInReturn = false;

    private void OnEnable()
    {
        BaseEventManager.OnBallReachedPit += new EventHandler(HandleBallReachPit);
    }

    private void OnDisable()
    {
        BaseEventManager.OnBallReachedPit -= new EventHandler(HandleBallReachPit);
    }

    // Use this for initialization
    void Start () {
        m_rb = GetComponent<Rigidbody>();
        startPos = ballStartTransform.position;
        startRot = ballStartTransform.rotation;
        ballInReturn = true;
    }

    private void HandleBallReachPit() {
        ballInPit = true;
        ballInReturn = false;
        ResetBall();
    }

    private void ResetBall() {
        transform.position = startPos;
        transform.rotation = startRot;

        isMoving = false;
        m_rb.velocity = Vector3.zero;
        m_rb.angularVelocity = Vector3.zero;
        m_rb.AddForce(transform.forward * 0.05f, ForceMode.Impulse);

        ballInReturn = true;
    }

    public void FireBall() {
        // m_rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        isMoving = true;
        ballInPit = false;
        ballInReturn = false;
    }

    // Update is called once per frame
    void Update () {

        if (ballInReturn) {
            return;
        }

        if (ballInPit) {
            return;
        }

        if (m_rb.velocity.magnitude < 0.1f) {
            ResetBall();
        }
        /*
        if (Input.GetMouseButtonDown(0)) {
            if (!isMoving)
            {
                FireBall();
            }
        }
        */
    }
}
