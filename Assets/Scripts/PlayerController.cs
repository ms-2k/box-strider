using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //is the player the it
    public bool it = false;

    //player speed multiplier (default 5)
    public float speed = 5f;

    //how much to slow the player down by on wall collisioni (default 5)
    public float obstacleMult = 5f;

    //control keys (default WASD)
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    //the rigid body for collision
    private Rigidbody rb;

    private void Start()
    {
        //get rigid body
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //move up
        if (Input.GetKey(upKey))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, speed);
        }
        //move down
        else if (Input.GetKey(downKey))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, -speed);
        }
        //stop moving up/down if neither up/down keys are pressed
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
        }

        //move left
        if (Input.GetKey(leftKey))
        {
            rb.velocity = new Vector3(-speed, 0f, rb.velocity.z);
        }
        //move right
        else if (Input.GetKey(rightKey))
        {
            rb.velocity = new Vector3(speed, 0f, rb.velocity.z);
        }
        //stop moving left/right if neither left/right keys are pressed
        else
        {
            rb.velocity = new Vector3(0f, 0f, rb.velocity.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //check if the player collided with an obstacle (wall)
        if (collision.gameObject.tag == "Wall")
        {
            //make the player slower if so
            speed /= obstacleMult;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //reverse collision slow down
        if (collision.gameObject.tag == "Wall")
        {
            speed *= obstacleMult;
        }
    }
}
