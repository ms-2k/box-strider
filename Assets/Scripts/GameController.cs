using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    //list of player game objects
    private GameObject[] players;

    //Time remaining display
    public GameObject timeRemaining;

    //pre game start UI
    public GameObject gameStarter;

    //it indicator
    public GameObject itIndicator;

    //game over menu
    public GameObject gameOverUI;

    //game over score text
    public GameObject gameOverScore;

    //it ID
    public int itId = -1;

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

    //game duration
    public float gameDuration = 300f;

    //game countdown (game stops when it reaches 0)
    private float gameTimer = 300f;

    //keep track of how long each player has been it
    public float[] timeAsIt;

    //game over check
    public bool gameOver = false;

    //game start check
    public bool gameStart = false;

    void Start()
    {
        //find player game objects
        players = GameObject.FindGameObjectsWithTag("Player");

        //initialize countdown
        gameTimer = gameDuration;

        //initialize it time tracker
        timeAsIt = new float[numPlayers];

        //randomly select one it
        itId = UnityEngine.Random.Range(0, numPlayers);

        //make it it and make indicator follow
        players[itId].GetComponent<PlayerController>().BecomeIt(itScale, itSpeed);
        itIndicator.GetComponent<IndicatorController>().ChangeTarget(players[itId]);

        //apply small bonus score for starting it
        timeAsIt[itId] = -gameDuration / 10f;

        //disable players beyond player count
        for (int i = numPlayers; i < 4; i++)
        {
            players[i].SetActive(false);
        }

        for (int i = 0; i < numPlayers; i++)
        {
            players[i].GetComponent<PlayerController>().SetID(i);
        }

        //generate obstacles
        GetComponent<MapGenerator>().GenerateObstacles(players, numPlayers, width, height, playerDistance, wallChance);
    }

    private void Update()
    {
        if (!gameStart)
            return;

        if (Input.GetKey(KeyCode.Escape))
        {
            RestartGame();
        }

        if (!gameOver)
        {
            //accumulate time as it while game is running
            timeAsIt[itId] += Time.deltaTime;

            //update time remaining until game ends
            timeRemaining.GetComponent<TextMeshProUGUI>().SetText(Mathf.RoundToInt(gameTimer).ToString());
        }

        //decrement game timer counter
        gameTimer -= Time.deltaTime;


        if (gameTimer <= 0 && !gameOver)
        {
            //set game over bool on
            gameOver = true;

            //set game over UI active
            gameOverUI.SetActive(true);

            //ensure that the time remaining text displays 0
            timeRemaining.GetComponent<TextMeshProUGUI>().SetText("0");

            string scoreText = "Scores:\n";
            for(int i = 0; i < numPlayers; i++)
            {
                scoreText += "\nPlayer" + (i + 1).ToString() + ": " + Mathf.RoundToInt(gameDuration - timeAsIt[i]).ToString();
            }

            gameOverScore.GetComponent<TextMeshProUGUI>().SetText(scoreText);
        }
    }

    //restarts the game
    public void RestartGame()
    {
        //reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //starts the game
    public void StartGame()
    {
        //set game start condition
        gameStart = true;

        //deactivate game starter UI
        gameStarter.SetActive(false);

        //reset it indicator (dunno why it breaks without)
        for (int i = 0; i < numPlayers; i++)
        {
            if (players[i].GetComponent<PlayerController>().it)
            {
                itIndicator.GetComponent<IndicatorController>().ChangeTarget(players[i]);
            }
        }
    }

    //regenerates the map
    public void Regenerate()
    {
        //reset all obstacles
        GetComponent<MapGenerator>().ResetObstacles();

        //generate again
        GetComponent<MapGenerator>().GenerateObstacles(players, numPlayers, width, height, playerDistance, wallChance);
    }

    //exits the game
    public void StopGame()
    {
        Application.Quit();
    }
}
