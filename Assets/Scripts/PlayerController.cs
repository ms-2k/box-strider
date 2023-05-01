using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool it = false;
    public float speed = 5f;
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKey(upKey))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, speed);
        }
        else if (Input.GetKey(downKey))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, -speed);
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
        }
        if (Input.GetKey(leftKey))
        {
            rb.velocity = new Vector3(-speed, 0f, rb.velocity.z);
        }
        else if (Input.GetKey(rightKey))
        {
            rb.velocity = new Vector3(speed, 0f, rb.velocity.z);
        }
        else
        {
            rb.velocity = new Vector3(0f, 0f, rb.velocity.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            speed /= 5;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            speed *= 5;
        }
    }
}
