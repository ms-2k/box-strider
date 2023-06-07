using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyButton : MonoBehaviour
{
    //store target lobby name
    public string targetId = "null";

    //the lobby manager
    private LobbyManager lobbyManager;

    //the sound controller
    private SoundController soundController;

    private void Awake()
    {
        //get the sound controller
        soundController = GameObject.FindGameObjectWithTag("SFX").GetComponent<SoundController>();

        //get the lobby manager
        lobbyManager = GameObject.FindGameObjectWithTag("LobbyManager").GetComponent<LobbyManager>();
    }

    //run on click
    public void JoinLobby()
    {
        soundController.playClick();
        lobbyManager.JoinLobby(targetId);
    }

}
