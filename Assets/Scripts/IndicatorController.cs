using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorController : MonoBehaviour
{
    //it rigidbody
    public Rigidbody itRigidbody;

    //indicator rotation speed (rotates this many degrees per second)
    public float rotationSpeed = 180f;

    void Update()
    {
        //don't show if there is no it (this shouldn't happen)
        if (itRigidbody == null)
            return;

        //follow it position
        //(use rigidbody because position is whacky from rotations)
        transform.position = itRigidbody.worldCenterOfMass + new Vector3(0f, 1.25f, 0f);
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    public void ChangeTarget(GameObject it)
    {
        //acquire the it's rigidbody
        itRigidbody = it.GetComponent<Rigidbody>();
    }
}
