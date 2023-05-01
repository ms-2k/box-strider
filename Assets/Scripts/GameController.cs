using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    private int[,] walls;
    private GameObject[] players;
    private GameObject it;
    public float itScale = 1.5f;
    public int numPlayers = 4;
    public int width = 26, height = 16;

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        it = players[Random.Range(0, numPlayers - 1)];
        it.GetComponent<PlayerController>().it = true;
        it.GetComponent<PlayerController>().speed *= 1.25f;
        it.transform.localScale = new Vector3(itScale * it.transform.localScale.x, itScale * it.transform.localScale.y, itScale * it.transform.localScale.z);
        for(int i = numPlayers; i < 4; i++)
        {
            players[i].gameObject.SetActive(false);
        }

        walls = new int[width, height];
        for (int i = 0; i < width; i++)
        {
            walls[i, 0] = Random.Range(1, 3);
            walls[i, height-1] = Random.Range(1, 3);
        }
        for (int i = 0; i < height; i++)
        {
            walls[0, i] = Random.Range(1, 3);
            walls[width-1, i] = Random.Range(1, 3);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
