using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameControllerMulti : NetworkBehaviour
{
    //width and height of map grid
    public int Width { get; private set; }
    public int Height { get; private set; }

    //inverse chance of obstacles in the middle of the map
    public int wallChance = 20;

    //it ID
    public int itId = -1;

    //how much to multiply the it's size by (default 1.25)
    public float itScale = 1.25f;

    //how much faster the it should be (default 1.25)
    public float itSpeed = 1.25f;

    //base player speed
    public float baseSpeed = 4.0f;

    //base divider for obstacle collision
    public float obstacleMult = 5.0f;

    //game duration
    public float gameDuration = 300f;

    //game countdown (game stops when it reaches 0)
    private float gameTimer = 300f;

    //comparison game timer
    private int prevSec = 300;

    //center of body level at which the object is no longer "on the ground"
    private float baseOnGroundThreshold = 0.37f;

    //keep track of how long each player has been it
    public float[] timeAsIt;

    //game over check
    public bool gameOver = false;

    //has the game started
    public bool gameStart = false;

    //is the player object touching a wall
    private bool[] isTouchingWall;

    //player it transfer cooldown
    private float[] cooldown;

    //list of player game objects
    private List<GameObject> players;

    //it indicator
    private GameObject itIndicator;

    //it indicator prefab
    public GameObject itIndicatorPrefab;

    //waiting for players... UI object
    public GameObject waitingUI;

    //the sound controller
    private SoundController soundController;

    //camer follower script
    private CameraFollow cameraFollow;

    //time remaining text display
    public GameObject timeRemaining;

    //game over score display UI element
    public GameObject gameOverScore;

    //game over UI container
    public GameObject gameOverUI;

    //game over return to menu button
    public Button returnButton;

    private void Awake()
    {
        //create new list for players
        players = new List<GameObject>();

        //initialize cooldown and wall touch check
        isTouchingWall = new bool[4];
        cooldown = new float[4];
        for (int i = 0; i < 4; i++)
        {
            isTouchingWall[i] = false;
            cooldown[i] = 0f;
        }

        //return to main menu on return button click
        returnButton.onClick.AddListener(() => {
            DisconnectServerRpc();
            Disconnect();
        });

        //synchronize initial game timer and prevSec
        prevSec = (int)gameTimer;

        //get the sound controller
        soundController = GameObject.FindGameObjectWithTag("SFX").GetComponent<SoundController>();

        //get camera follow class
        cameraFollow = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
    }

    private void Update()
    {
        //return to menu if Escape is clicked
        if (Input.GetKey(KeyCode.Escape))
        {
            DisconnectServerRpc();
            Disconnect();
        }

        //wait until there are 4 players
        if (!gameStart && players.Count == 4)
        {
            //start game if it does
            StartGame();
            StartGameClientRpc();
            SetCameraClientRpc();
        }

        //decrement cooldown for each player if they are greater than 0
        for (int i = 0; i < 4; i++)
        {
            if (cooldown[i] > 0f)
            {
                cooldown[i] -= Time.deltaTime;
                StopMovementServerRpc(i);
            }
        }

        //decrement game timer if it is greater than zero
        if (gameTimer > 0f && gameStart)
            gameTimer -= Time.deltaTime;

        //if it hits zero, set gameOver on
        else if (gameTimer <= 0f)
        {
            gameOver = true;
            GameOverClientRpc(timeAsIt);
        }

        //send client rpc to update game timer display every second
        if ((int)gameTimer != prevSec)
        {
            prevSec = (int)gameTimer;
            UpdateTimerClientRpc(prevSec);
        }
    }

    //return the number of players connected
    public int playerCount()
    {
        //fetch it from players
        if (players.Count <= 4)
            return players.Count;
        else
            return 4; //no idea why I need this but I do
    }


    private void Disconnect()
    {
        //play click sound
        soundController.playClick();

        //sign out of authentication service
        AuthenticationService.Instance.SignOut();

        //shutdown network manager and destroy it
        NetworkManager.Singleton.Shutdown();
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton);
        }

        //reload main menu
        SceneManager.LoadScene("Main");
    }

    //disconnect player from server
    [ServerRpc]
    private void DisconnectServerRpc()
    {
        //temp solution
        //TODO: figure out removing specific player instead of disconnecting everyone
        DisconnectAllClientRpc();
    }

    //remove all players from the game
    [ClientRpc]
    private void DisconnectAllClientRpc()
    {
        //call disconnect in each client
        Disconnect();
    }

    //run on game over
    [ClientRpc]
    public void GameOverClientRpc(float[] timeAsIt)
    {
        //set game over UI active
        gameOverUI.SetActive(true);

        //ensure that the time remaining text displays 0
        timeRemaining.GetComponent<TextMeshProUGUI>().SetText("0");

        //generate score text (same as singleplayer)
        string scoreText = "Scores:\n";
        for (int i = 0; i < 4; i++)
        {
            scoreText += "\nPlayer" + (i + 1).ToString() + ": " + Mathf.RoundToInt(gameDuration - timeAsIt[i]).ToString();
        }

        //set score text
        gameOverScore.GetComponent<TextMeshProUGUI>().SetText(scoreText);
    }

    //what to do on clients on game start
    [ClientRpc]
    private void StartGameClientRpc()
    {
        //turn the waiting GUI thing off
        waitingUI.SetActive(false);
    }

    //set camera object for player
    [ClientRpc]
    public void SetCameraClientRpc()
    {
        Debug.Log("camera test");
        cameraFollow.player = NetworkManager.LocalClient.PlayerObject.gameObject.GetComponent<Rigidbody>();
    }

    //update time remaining UI text
    [ClientRpc]
    private void UpdateTimerClientRpc(int gameSec)
    {
        timeRemaining.GetComponent<TextMeshProUGUI>().SetText(gameSec.ToString());
    }

    //add a new player to player list
    public void AddPlayer(GameObject player)
    {
        Debug.Log("player added");
        player.GetComponent<PlayerControllerMulti>().id = players.Count;
        players.Add(player);
    }


    //add a new player to player list
    public GameObject GetPlayer(int id)
    {
        return players[id];
    }

    //called when 4 players join
    public void StartGame()
    {
        //make sure it's run from the host
        if (!NetworkManager.IsHost)
            return;

        //set gameStart to true
        gameStart = true;

        //instantiate an it indicator
        itIndicator = Instantiate(itIndicatorPrefab);
        itIndicator.GetComponent<NetworkObject>().Spawn(true);

        //initialize countdown
        gameTimer = gameDuration;

        //initialize it time tracker
        timeAsIt = new float[4];

        //randomly select one it
        itId = UnityEngine.Random.Range(0, players.Count);

        //set the initial it's scale
        players[itId].transform.localScale = new Vector3(
            itScale * 4.0f,
            itScale * 4.0f,
            itScale * 4.0f
        );

        //randomly set map size
        //both width and height need to be even
        Width = UnityEngine.Random.Range(26, 52) / 2 * 2;
        Height = UnityEngine.Random.Range(16, 32) / 2 * 2;

        //make it it and make indicator follow
        itIndicator.GetComponent<IndicatorControllerMulti>().ChangeTargetServerRpc(itId);

        //apply small bonus score for starting it
        timeAsIt[itId] = -gameDuration / 10f;

        //initialize map
        InitializeMap();

        //set player position
        gameObject.GetComponent<MapGeneratorMulti>().ResetPlayerPositionsServerRpc(Width, Height);
    }

    //call map generator to generate a map for us
    public void InitializeMap()
    {
        gameObject.GetComponent<MapGeneratorMulti>().GenerateObstaclesServerRpc(Width, Height, wallChance);
    }

    //handles player movement
    [ServerRpc (RequireOwnership = false)]
    public void MovementServerRpc(int id, bool up, bool down, bool left, bool right, bool fast)
    {
        //don't do anything if player is on cooldown or if the game hasn't started yet
        if (cooldown[id] > 0 || !gameStart)
            return;

        //acquire player rigidbody with id
        Rigidbody playerRigidbody = players[id].GetComponent<Rigidbody>();

        //set speed and ground threshold values depending on if they are it or not
        float speed = id == itId ? baseSpeed * itSpeed : baseSpeed;
        float onGroundThreshold = id == itId ? baseOnGroundThreshold * itScale : baseOnGroundThreshold;

        //make player move slower if they aren't "running"
        if (!fast)
            speed /= 1.5f;

        //decrement speed further if they are touching a wall
        if (isTouchingWall[id])
            speed *= 0.1f;

        //move up
        if (up && playerRigidbody.worldCenterOfMass.y < onGroundThreshold)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, speed);
        }
        //move down
        else if (down && playerRigidbody.worldCenterOfMass.y < onGroundThreshold)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, -speed);
        }
        //stop moving up/down if neither up/down keys are pressed
        else if (playerRigidbody.worldCenterOfMass.y < onGroundThreshold)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, 0f);
        }

        //move left
        if (left && playerRigidbody.worldCenterOfMass.y < onGroundThreshold)
        {
            playerRigidbody.velocity = new Vector3(-speed, 0f, playerRigidbody.velocity.z);
        }
        //move right
        else if (right && playerRigidbody.worldCenterOfMass.y < onGroundThreshold)
        {
            playerRigidbody.velocity = new Vector3(speed, 0f, playerRigidbody.velocity.z);
        }
        //stop moving left/right if neither left/right keys are pressed
        else if (playerRigidbody.worldCenterOfMass.y < onGroundThreshold)
        {
            playerRigidbody.velocity = new Vector3(0f, 0f, playerRigidbody.velocity.z);
        }
    }

    //called to stop player movement (no sliding!)
    [ServerRpc (RequireOwnership = false)]
    public void StopMovementServerRpc(int id)
    {
        players[id].GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    //called upon wall collision
    //sets isTouchingWall true for the corresponding player
    [ServerRpc(RequireOwnership = false)]
    public void WallCollisionServerRpc(int id)
    {
        isTouchingWall[id] = true;
    }

    //called upon collision exit on walls
    //does the opposite above above function
    [ServerRpc(RequireOwnership = false)]
    public void WallExitServerRpc(int id)
    {
        isTouchingWall[id] = false;
    }

    //called upon player to player collision
    [ServerRpc(RequireOwnership = false)]
    public void PlayerCollisionServerRpc(int collider, int collidee)
    {
        //Debug.Log(collider + ", " + collidee);
        //Debug.Log(players.Count);
        //skip doing anything if either are on cooldown
        if (cooldown[collider] > 0f || cooldown[collidee] > 0f)
            return;

        //termporarily store previous it's id
        int prevIt = -1;

        //now depending on the itId, set a new itId
        //in this case the collider was the it
        if (collider == itId)
        {
            //so make collidee the it
            itId = collidee;
            prevIt = collider;
        }
        //case for when collidee is it
        else if (collidee == itId)
        {
            //do the opposite
            itId = collider;
            prevIt = collidee;
        }
        else
            return;

        //upscale the new it
        players[itId].transform.localScale = new Vector3(
            itScale * 4.0f,
            itScale * 4.0f,
            itScale * 4.0f
        );
        //and downscale the old it
        players[prevIt].transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

        //set the new it's cooldown to 3 seconds so the it doesn't get transferred back immediately
        cooldown[itId] = 3.0f;

        //move the it indicator to the new it too
        itIndicator.GetComponent<IndicatorControllerMulti>().ChangeTargetServerRpc(itId);

        //play sound effect in clients
        PlaySwishSFXClientRpc();
    }

    //play the swish sound effect
    [ClientRpc]
    private void PlaySwishSFXClientRpc()
    {
        //swoosh
        soundController.playSwish();
    }
}
