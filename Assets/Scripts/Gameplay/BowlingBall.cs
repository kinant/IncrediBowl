using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used to handle the bowling ball
public class BowlingBall : MonoBehaviour {

    private Rigidbody m_rb; // reference to the rigid body

    public float throwForce = 75f; // throw force, used for PC debugging

    private bool hitPin = false; // a flag that is set when the ball hits a pin
    private bool isMoving = false; // a flag that is set when the ball is moving

    public Transform ballStartTransform; // a transform to indicate the start position of the ball
    public bool PCDebug = false; // a flag that is used to allow us to test the ball using the PC (no VR)

    private Vector3 startPos; // to cache the balls start position
    private Quaternion startRot; // to cache the balls starting rotation

    private bool ballInPit = false; // a flag to check if the ball is in the pit
    private bool ballInReturn = false; // a flag to check if the ball is in the return machine

    public AudioClip throwSound; // audio sfx to play when the ball is moving
    private AudioSource audioSource; // cache for the audiosource

    // subscribe to events
    private void OnEnable()
    {
        // handle the event of the ball reaching the pit
        BaseEventManager.OnBallReachedPit += new EventHandler(HandleBallReachPit);
    }

    // unsubscribe from events
    private void OnDisable()
    {
        BaseEventManager.OnBallReachedPit -= new EventHandler(HandleBallReachPit);
    }

    // Use this for initialization
    void Start () {
        // set the inital variables and get components
        m_rb = GetComponent<Rigidbody>();
        startPos = ballStartTransform.position;
        startRot = ballStartTransform.rotation;
        ballInReturn = true;
        audioSource = GetComponent<AudioSource>();
    }

    // called in the event that the ball reaches the pit at the end of the lane
    private void HandleBallReachPit() {
        // stop any audio, set the flags for the ball and reset the ball
        audioSource.Stop();
        ballInPit = true;
        ballInReturn = false;
        ResetBall();
    }

    // resets the ball
    private void ResetBall() {
        // reset position and rotation
        transform.position = startPos;
        transform.rotation = startRot;

        // reset movement flag and velocities
        isMoving = false;
        m_rb.velocity = Vector3.zero;
        m_rb.angularVelocity = Vector3.zero;

        // 
        // m_rb.AddForce(transform.forward * 0.05f, ForceMode.Impulse);

        // set the flag since the ball is in the return area
        ballInReturn = true;
    }

    // called when the ball is fired
    public void FireBall() {

        // if we are debugging in PC, then we can click to fire the ball, so we add a force
        if (PCDebug)
        {
            m_rb.AddForce(-transform.forward * throwForce, ForceMode.Impulse);
        }

        // set the flags
        isMoving = true;
        ballInPit = false;
        ballInReturn = false;

        // play the ball thrown sound
        audioSource.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Lane")) {
            audioSource.PlayOneShot(throwSound);
        }
    }

    // Update is called once per frame
    void Update () {

        // do nothing if the ball is in the return area
        if (ballInReturn) {
            return;
        }

        // do nothing if the ball is in the pit
        if (ballInPit) {
            return;
        }

        // if the ball is anywhere but the return area or the pit, and it is going very slowly or it has stopped, then
        // we have a ball that was not thrown strong enough and will just stay in the lane, so we reset it
        if (m_rb.velocity.magnitude < 0.1f) {
            ResetBall();
        }

        // if we are using a PC to debug, with no VR, then we can fire the ball when we click the mouse
        if (PCDebug)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // check that the ball is not already moving, if not, we fire it
                if (!isMoving)
                {
                    FireBall();
                }
            }
        }
        
    }
}
