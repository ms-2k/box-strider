using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IndicatorControllerMulti : NetworkBehaviour
{
    //it rigidbody
    public Rigidbody itRigidbody;

    //get the game controller;
    public GameControllerMulti gameController;

    //indicator rotation speed (rotates this many degrees per second)
    public float rotationSpeed = 180f;

    private void Awake()
    {
        //acquire game controller
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerMulti>();
    }

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

    [ServerRpc]
    public void ChangeTargetServerRpc(int id)
    {
        //set target gameobject to one stored in players
        GameObject it = gameController.GetPlayer(id);

        //acquire the it's rigidbody
        itRigidbody = it.GetComponent<Rigidbody>();
    }
}
