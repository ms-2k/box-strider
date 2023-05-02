using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //list of player game objects
    private GameObject[] players;

    //how much to multiply the it's size by (default 1.25)
    public float itScale = 1.25f;

    //how much faster the it should be (default 1.25)
    public float itSpeed = 1.25f;

    //base player speed
    public float baseSpeed = 5.0f;

    //base divider for obstacle collision
    public float obstacleMult = 5.0f;

    //the total number of players (default 4)
    public int numPlayers = 4;

    //width and height of map grid
    public int width = 26;
    public int height = 16;

    //minimum starting distance between players
    public float playerDistance = 8;

    //inverse chance of obstacles in the middle of the map
    public int wallChance = 20;

    //it indicator
    public GameObject itIndicator;

    //game duration
    public float gameDuration = 30f;

    //keep track of how long each player has been it
    public float[] timeAsIt;

    //game over check
    public bool gameOver = false;

    void Start()
    {
        //find player game objects
        players = GameObject.FindGameObjectsWithTag("Player");

        //initialize it time tracker
        timeAsIt = new float[numPlayers];

        //randomly select one it
        int it = Random.Range(0, numPlayers);
        players[it].GetComponent<PlayerController>().BecomeIt(itScale, itSpeed);

        //set indicator target
        itIndicator.GetComponent<IndicatorController>().ChangeTarget(players[it]);

        //disable players beyond player count
        for (int i = numPlayers; i < 4; i++)
        {
            players[i].gameObject.SetActive(false);
        }

        //generate obstacles
        GetComponent<MapGenerator>().GenerateObstacles(players, numPlayers, width, height, playerDistance, wallChance);
    }

    private void Update()
    {
        gameDuration -= Time.deltaTime;
        if (gameDuration <= 0)
            gameOver = true;
    }

}
