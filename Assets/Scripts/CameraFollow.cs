using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraFollow : MonoBehaviour
{
    //target player transform
    public Rigidbody player;

    //camera velocity
    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        //don't follow before player is set
        if (player == null)
            return;

        //update player position with smoothdamp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            player.worldCenterOfMass + new Vector3(0, 8, -3),
            ref velocity,
            0.1f
        );
    }
}
