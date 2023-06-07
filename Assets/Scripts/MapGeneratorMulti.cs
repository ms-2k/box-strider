using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGeneratorMulti : NetworkBehaviour
{

    //game objects to spawn
    public GameObject wall;
    public GameObject wallWide;
    public GameObject wallTall;
    public GameObject wallLarge;

    //parent game object for obstacles
    public GameObject wallParent;

    //list of walls generated
    private List<GameObject> wallList;

    //the ground object
    public Transform Ground;

    //store obstacle data array
    private int[,] obstacles;

    //the game controller
    private GameControllerMulti gameController;

    private void Awake()
    {
        //acquire game controller to access variables
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControllerMulti>();
    }

    //destroy all obstacles
    public void ResetObstacles()
    {
        foreach (GameObject wall in wallList)
        {
            Destroy(wall);
        }
    }

    //reset player positions based on current obstacle list
    [ServerRpc]
    public void ResetPlayerPositionsServerRpc(int width, int height, int maxTries = 1000, int playerDistance = 4)
    {
        //is the player placed far enough from other players
        bool distanceCheck;

        //acquire number of players (should be 4)
        int playerCount = gameController.playerCount();

        //temporarily store player coordinates
        int[,] coords = new int[playerCount, 2];

        //initialize values;
        for (int i = 0; i < playerCount; i++)
        {
            coords[i, 0] = width / 2;
            coords[i, 0] = height / 2;
        }

        //temporary coordinate
        int[] temp;

        //keep track of failed attempts
        List<int[]> available = new List<int[]>();

        //check through available slots
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                try
                {
                    if (obstacles[x, y] == 8)
                    {
                        available.Add(new int[] { x, y });
                    }
                }
                catch (IndexOutOfRangeException ex){
                    Debug.Log(ex);
                }
            }
        }

        //number of attemps and a random index
        int numTries, randIndex;

        //helper x and y start values for calculation
        float x_start = width / 2;
        float y_start = height / 2;

        for (int i = 0; i < playerCount; i++)
        {

            //try up to max number of tries
            for (numTries = 0; numTries < maxTries; i++)
            {
                //acquire a random coordinate
                randIndex = UnityEngine.Random.Range(0, available.Count);
                temp = available[randIndex];

                //remove it
                available.RemoveAt(randIndex);

                //initialize distance check
                distanceCheck = true;

                //for every other player
                for (int j = 0; j < i; j++)
                {
                    //skip if coordinates are the same (never will happen) or if it's the same player
                    if (i == j || (coords[j, 0] == temp[0] && coords[j, 1] == temp[1]))
                    {
                        continue;
                    }
                    //check if euclidean distance is greater than playerDistance
                    if (Mathf.Sqrt(Mathf.Pow(temp[0] - coords[j, 0], 2) + Mathf.Pow(temp[1] - coords[j, 1], 2)) < playerDistance)
                    {
                        distanceCheck = false;
                        break;
                    }
                }

                //if distance check succeeds, set new coordinates
                if (distanceCheck)
                {
                    coords[i, 0] = temp[0];
                    coords[i, 1] = temp[1];
                    break;
                }
            }

            //if max number of tries didn't reach
            if (numTries < maxTries) {
                Debug.Log(coords[i, 0] + ", " + coords[i, 1]);
                Debug.Log((coords[i, 0] - x_start + 0.5f).ToString() + ", " + (-(coords[i, 1] - y_start + 0.5f)).ToString());
                //set player coordinate to new coordinate
                gameController.GetPlayer(i).transform.position = new Vector3(
                    coords[i, 0] - x_start + 0.5f,
                    0.1f,
                    -(coords[i, 1] - y_start + 0.5f)
                );

                //reset rotation too just in case
                gameController.GetPlayer(i).transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    //helper method to generate the map
    [ServerRpc]
    public void GenerateObstaclesServerRpc(int width, int height, int wallChance = 20)
    {
        //set ground scale to game controller set width and height
        Ground.localScale = new Vector3(width / 10f, 1f, height / 10f);

        //create a new list of gameobjects to keep track of walls
        wallList = new List<GameObject>();

        //initialize array to store wall data
        obstacles = new int[width, height];

        //initialize to 0
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                obstacles[i, j] = 0;

        //initialize the outer walls
        for (int x = 0; x < width; x++)
        {
            obstacles[x, 0] = 1;
            obstacles[x, height - 1] = 1;
        }
        for (int y = 0; y < height; y++)
        {
            obstacles[0, y] = 1;
            obstacles[width - 1, y] = 1;
        }

        /*
         * temporary variable for storing wall type in obstacles
         * numbers 1 to 4 will point towards the top left corner (head)
         * 1 = small
         * 2 = wide (2x1)
         * 3 = tall (1x2)
         * 4 = large (2x2)
         * 5 = head left
         * 6 = head above
         * 7 = head top left
         * 8 = reachable from center
         * 9 = traversed from unreachable, to be used for connecting unreachable zones
         */
        int wallType;

        //generate internal obstacles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //check if it has been taken up by other wall types
                if (obstacles[x, y] > 1 || UnityEngine.Random.Range(0, wallChance + 1) > 1)
                {
                    continue;
                }
                if ((x == width / 2 || x == width / 2 + 1) && (y == height / 2 || y == height / 2 + 1))
                {
                    Debug.Log("middle cleared");
                    continue;
                }

                //check for boundaries
                //walls will be generated from top left to bottom right
                if (x > width - 2)
                {
                    if (y > height - 2)
                    {
                        //at bottom right
                        wallType = 1;
                    }
                    else
                    {
                        //at right
                        wallType = UnityEngine.Random.Range(1, 3);
                        wallType = wallType == 2 ? 3 : wallType;
                    }
                }
                else if (y > height - 2)
                {
                    //at bottom
                    wallType = UnityEngine.Random.Range(1, 3);
                }
                else
                {
                    //otherwise all directions ok
                    wallType = UnityEngine.Random.Range(1, 5);
                }
                //Debug.Log(x.ToString() + " " + y.ToString() + " " + wallType.ToString());

                //fill in obstacle matrix
                switch (wallType)
                {
                    //wide wall
                    case 2:
                        //ensure that the current tile is not already filled (by a bigger wall from previous iterations)
                        if (obstacles[x + 1, y] > 1)
                        {
                            continue;
                        }
                        //fill in the tile
                        obstacles[x, y] = wallType;
                        obstacles[x + 1, y] = wallType + 3;
                        break;

                    //tall wall
                    case 3:
                        if (obstacles[x, y + 1] > 1)
                        {
                            continue;
                        }
                        obstacles[x, y] = wallType;
                        obstacles[x, y + 1] = wallType + 3;
                        break;

                    //large wall
                    case 4:
                        if (obstacles[x + 1, y + 1] > 1 || obstacles[x, y + 1] > 1 || obstacles[x + 1, y] > 1)
                        {
                            continue;
                        }
                        obstacles[x, y] = wallType;
                        obstacles[x + 1, y] = wallType + 1;
                        obstacles[x, y + 1] = wallType + 2;
                        obstacles[x + 1, y + 1] = wallType + 3;
                        break;

                    //small wall
                    case 1:
                        obstacles[x, y] = 1;
                        break;
                }

            }
        }

        //mark all reachable tiles
        FloodFill(obstacles, width / 2, height / 2, width, height);

        //now check for all unreachable tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (obstacles[x, y] == 0)
                {

                    //create a path from it to the center for each
                    MakePath(obstacles, x, y, width / 2, height / 2, width, height);

                    //recalculate reachability
                    FloodFill(obstacles, x, y, width, height);
                }
            }
        }

        //regenerate lost outer walls (they could have been deleted multi tile boxes)
        for (int x = 0; x < width; x++)
        {
            if (obstacles[x, 0] >= 8)
                obstacles[x, 0] = 1;
            if (obstacles[x, height - 1] >= 8)
                obstacles[x, height - 1] = 1;
        }
        for (int y = 0; y < height; y++)
        {
            if (obstacles[0, y] >= 8)
                obstacles[0, y] = 1;
            if (obstacles[width - 1, y] >= 8)
                obstacles[width - 1, y] = 1;
        }

        //temporary variable for new wall
        GameObject newWall;

        //spawn objects
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //randomly rotate half
                bool rotate90 = UnityEngine.Random.Range(0, 2) == 1;
                float x_start = width / 2;
                float y_start = height / 2;

                //check which type to spawn
                switch (obstacles[x, y])
                {

                    //small wall
                    case 1:
                        if (rotate90)
                            newWall = Instantiate(wall, new Vector3(x - x_start + 0.5f, 0, -(y - y_start + 0.5f)), Quaternion.Euler(0, 90, 0));
                        else
                            newWall = Instantiate(wall, new Vector3(x - x_start + 0.5f, 0, -(y - y_start + 0.5f)), new Quaternion(0, 0, 0, 0));
                        newWall.GetComponent<NetworkObject>().Spawn();
                        newWall.transform.parent = wallParent.transform;
                        wallList.Add(newWall);
                        break;

                    //wide wall
                    case 2:
                        if (rotate90)
                            newWall = Instantiate(wallTall, new Vector3(x - x_start + 1f, 0, -(y - y_start + 0.5f)), Quaternion.Euler(0, 90, 0));
                        else
                            newWall = Instantiate(wallWide, new Vector3(x - x_start + 1f, 0, -(y - y_start + 0.5f)), new Quaternion(0, 0, 0, 0));
                        newWall.GetComponent<NetworkObject>().Spawn();
                        newWall.transform.parent = wallParent.transform;
                        wallList.Add(newWall);
                        break;

                    //tall wall
                    case 3:
                        if (rotate90)
                            newWall = Instantiate(wallWide, new Vector3(x - x_start + 0.5f, 0, -(y - y_start + 1f)), Quaternion.Euler(0, 90, 0));
                        else
                            newWall = Instantiate(wallTall, new Vector3(x - x_start + 0.5f, 0, -(y - y_start + 1f)), new Quaternion(0, 0, 0, 0));
                        newWall.GetComponent<NetworkObject>().Spawn();
                        newWall.transform.parent = wallParent.transform;
                        wallList.Add(newWall);
                        break;

                    //large wall
                    case 4:
                        if (rotate90)
                            newWall = Instantiate(wallLarge, new Vector3(x - x_start + 1f, 0, -(y - y_start + 1f)), Quaternion.Euler(0, 90, 0));
                        else
                            newWall = Instantiate(wallLarge, new Vector3(x - x_start + 1f, 0, -(y - y_start + 1f)), new Quaternion(0, 0, 0, 0));
                        newWall.GetComponent<NetworkObject>().Spawn();
                        newWall.transform.parent = wallParent.transform;
                        wallList.Add(newWall);
                        break;

                    //default case (basically players)
                    default:
                        //TODO
                        break;
                }
            }
        }
    }

    //use flood fill algorithm to check for all reachable tiles from the center
    void FloodFill(int[,] obstacles, int x, int y, int width, int height)
    {
        //check if matrix boundary was reached
        if (x < 0 || x == width - 1)
        {
            return;
        }
        if (y < 0 || y == height - 1)
        {
            return;
        }

        //check if the tile is blocked or not
        //and if it has been traversed (while clearing unreachable tiles)
        if (obstacles[x, y] > 0 && !(obstacles[x, y] == 9))
        {
            return;
        }

        //set tile to reachable as it is not blocked by an obstacle
        if (obstacles[x, y] == 0 || obstacles[x, y] == 9)
        {
            obstacles[x, y] = 8;
        }

        //recursively call for adjacent tiles
        FloodFill(obstacles, x - 1, y, width, height);
        FloodFill(obstacles, x + 1, y, width, height);
        FloodFill(obstacles, x, y - 1, width, height);
        FloodFill(obstacles, x, y + 1, width, height);
    }

    //generate path towards the center of the map from a given coordinate
    void MakePath(int[,] obstacles, int x, int y, int xTarget, int yTarget, int width, int height)
    {
        //check distance from the target
        int xDist = xTarget - x;
        int yDist = yTarget - y;

        //loop while coordinates are within bounds
        while (x > 0 && x < width && y > 0 && y < height)
        {
            //Debug.Log(obstacles[x, y].ToString() + " " + x.ToString() + " " + y.ToString());
            switch (obstacles[x, y])
            {
                //break out of program if a reachable tile is reached
                case 8:
                    return;

                //mark tile as traversed if it's empty
                case 0:
                    obstacles[x, y] = 9;
                    break;

                //empty the tile if it is a small box
                case 1:
                    obstacles[x, y] = 9;
                    break;

                //empty current and right tile if wide box
                case 2:
                    obstacles[x, y] = 9;
                    obstacles[x + 1, y] = 9;
                    break;

                //empty current and below tile if tall box
                case 3:
                    obstacles[x, y] = 9;
                    obstacles[x, y + 1] = 9;
                    break;

                //remove current, right, below, and bottom right tiles if large box
                case 4:
                    obstacles[x, y] = 9;
                    obstacles[x + 1, y] = 9;
                    obstacles[x, y + 1] = 9;
                    obstacles[x + 1, y + 1] = 9;
                    break;

                //could be right tile of wide or top right of large
                case 5:

                    //check if it's large
                    if (obstacles[x - 1, y] == 4)
                    {
                        obstacles[x - 1, y] = 9;
                        obstacles[x, y] = 9;
                        obstacles[x - 1, y + 1] = 9;
                        obstacles[x, y + 1] = 9;
                    }
                    //otherwise it's wide
                    else
                    {
                        obstacles[x - 1, y] = 9;
                        obstacles[x, y] = 9;
                    }
                    break;
                //could be bottom tile of tall or bottom left of large
                case 6:

                    //large wall
                    if (obstacles[x, y - 1] == 4)
                    {
                        obstacles[x, y - 1] = 9;
                        obstacles[x, y] = 9;
                        obstacles[x + 1, y - 1] = 9;
                        obstacles[x + 1, y] = 9;
                    }
                    //tall wall
                    else
                    {
                        obstacles[x, y - 1] = 9;
                        obstacles[x, y] = 9;
                    }
                    break;

                //bottom right of large
                case 7:
                    obstacles[x - 1, y - 1] = 9;
                    obstacles[x - 1, y] = 9;
                    obstacles[x, y - 1] = 9;
                    obstacles[x, y] = 9;
                    break;
            }

            //traverse to the next tile
            //priority to whichever axis is further
            if (Mathf.Abs(xDist) > Mathf.Abs(yDist))
            {
                x = xDist > 0 ? x + 1 : x - 1;
            }
            else
            {
                y = yDist > 0 ? y + 1 : y - 1;
            }
        }
    }
}
