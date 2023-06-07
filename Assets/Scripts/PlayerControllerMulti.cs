using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class PlayerControllerMulti : NetworkBehaviour
{
    //player ID
    public int id = -1;

    //control keys (default WASD)
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode fastKey = KeyCode.LeftShift;

    //the game controller object to pull data from
    public GameControllerMulti gameController;

    void Awake()
    {
        //get game controller
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerMulti>();

        //add this to player list
        gameController.AddPlayer(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //check if it is the owner
        //otherwise don't update it
        if (!IsOwner)
            return;

        //send server RPC to move the player upon player movement
        if (Input.GetKey(upKey) ||  Input.GetKey(downKey) || Input.GetKey(leftKey) || Input.GetKey(rightKey) || Input.GetKey(fastKey))
        {
            gameController.MovementServerRpc(id, Input.GetKey(upKey), Input.GetKey(downKey), Input.GetKey(leftKey), Input.GetKey(rightKey), Input.GetKey(fastKey));
        }
        //stop player movement if no keys are held
        else {
            gameController.StopMovementServerRpc(id);
        }
    }

    //detect collision and call appropriate server rpc
    private void OnCollisionEnter(Collision collision)
    {
        //wall collision
        if (collision.gameObject.CompareTag("Wall"))
            gameController.WallCollisionServerRpc(id);

        //player collision
        if (collision.gameObject.CompareTag("Player"))
            gameController.PlayerCollisionServerRpc(collision.gameObject.GetComponent<PlayerControllerMulti>().id, id);
    }


    private void OnCollisionExit(Collision collision)
    {
        //wall collision needs an onexit to restore player speed
        if (collision.gameObject.CompareTag("Wall"))
            gameController.WallExitServerRpc(id);
    }

}
