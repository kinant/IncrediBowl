using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script is used to handle pins
public class Pin : MonoBehaviour {
    private Rigidbody rb; // cache for the rigidbody
    private Collider col; // cache for the collider
    private Quaternion startRot; // the pins starting rotation
    private AudioSource audioSource; // an audio source for the pin

    public AudioClip hitSound; // sound to play when the pin is hit by the ball

    private void Start()
    {
        // cache components and set variables
        rb = GetComponent<Rigidbody>();
        col = GetComponent<MeshCollider>();
        audioSource = GetComponent<AudioSource>();
        startRot = transform.rotation;
    }

    // called to reset the pin, the pin is reset on the PIN SETTER and not on the ground,
    // the pin setter will be in charge of setting the pins to the ground after they are reset. 
    public void ResetPin() {

        // since the pin setter is holding the pin, we disable gravity and enable is kinematic on the rigid body
        rb.useGravity = false;
        rb.isKinematic = true;

        // we reset the pins rotation (fixes a bug)
        transform.rotation = startRot;
    }

    // handle collisions
    private void OnCollisionEnter(Collision collision)
    {
        // if the ball collides with a pin, we play the hit sound for it
        if (collision.gameObject.CompareTag("Ball"))
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
}
