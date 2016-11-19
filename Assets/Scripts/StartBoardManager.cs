using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class StartBoardManager : MonoBehaviour
{

    public int columns = 15;
    public int rows = 12;
    public int minRoomSize = 5;
    public GameObject[] wallTiles;
    public GameObject[] floorTiles;
    public GameObject[] verticalDoorTiles;
    public GameObject[] horizontalDoorTiles;

    public GameObject[,] floorLayerTiles;
    public GameObject[,] wallLayerTiles;
    private Transform boardHolder;
    private GameObject RandomObject(GameObject[] tile)
    {
        return tile[Random.Range(0, tile.Length)];
    }
    void BoardSetup()
    {
        GameObject[,] floorLayerPrefabTiles = new GameObject[columns, rows];
        GameObject[,] wallLayerPrefabTiles = new GameObject[columns, rows];
        floorLayerTiles = new GameObject[columns, rows];
        wallLayerTiles = new GameObject[columns, rows];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                floorLayerPrefabTiles[x, y] = RandomObject(floorTiles);
                wallLayerPrefabTiles[x, y] = null;
                floorLayerTiles[x, y] = null;
                wallLayerTiles[x, y] = null;
            }
        }
        int crossx, crossy, noWallIndex;
        crossx = Random.Range(minRoomSize - 1, columns - minRoomSize);
        crossy = Random.Range(minRoomSize - 1, rows - minRoomSize);
        noWallIndex = Random.Range(0, 4);
        int horizontalWallMin = 1;
        int horizontalWallMax = columns - 2;
        int verticalWallMin = 1;
        int verticalWallMax = rows - 2;
        switch (noWallIndex)
        {
            case 0:
                verticalWallMin = crossy;
                break;
            case 1:
                horizontalWallMax = crossx;
                break;
            case 2:
                verticalWallMax = crossy;
                break;
            case 3:
                horizontalWallMin = crossx;
                break;
        }
        for (int x = 0; x < columns; x++)
        {
            wallLayerPrefabTiles[x, 0] = RandomObject(wallTiles);
            wallLayerPrefabTiles[x, rows - 1] = RandomObject(wallTiles);
            if (x >= horizontalWallMin && x <= horizontalWallMax)
                wallLayerPrefabTiles[x, crossy] = RandomObject(wallTiles);
        }
        for (int y = 0; y < rows; y++)
        {
            wallLayerPrefabTiles[0, y] = RandomObject(wallTiles);
            wallLayerPrefabTiles[columns - 1, y] = RandomObject(wallTiles);
            if (y >= verticalWallMin && y <= verticalWallMax)
                wallLayerPrefabTiles[crossx, y] = RandomObject(wallTiles);
        }
        int doorx = crossx;
        while (doorx == crossx) doorx = Random.Range(horizontalWallMin, horizontalWallMax + 1);
        int doory = crossy;
        while (doory == crossy) doory = Random.Range(verticalWallMin, verticalWallMax + 1);
        wallLayerPrefabTiles[doorx, crossy] = RandomObject(horizontalDoorTiles);
        wallLayerPrefabTiles[crossx, doory] = RandomObject(verticalDoorTiles);
        //Instantiate Board and set boardHolder to its transform.
        boardHolder = new GameObject("Board").transform;

        //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
        for (int x = 0; x < columns; x++)
        {

            for (int y = 0; y < rows; y++)
            {
                GameObject floorToInstantiate = floorLayerPrefabTiles[x, y];
                GameObject floorInstance = Instantiate(floorToInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                floorInstance.transform.SetParent(boardHolder);
                floorLayerTiles[x, y] = floorInstance;


                GameObject wallToInstantiate = wallLayerPrefabTiles[x, y];
                if (wallToInstantiate != null)
                {
                    GameObject wallInstance = Instantiate(wallToInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    wallInstance.transform.SetParent(boardHolder);
                    wallLayerTiles[x, y] = wallInstance;
                }
            }
        }
    }

    public bool canMove(int x, int y)
    {
        if (x < 0 || y < 0 || x >= columns || y >= rows) return false;
        if (wallLayerTiles[x, y] == null)
            return true;
        BoxCollider2D collider = wallLayerTiles[x, y].GetComponent<BoxCollider2D>();
        if (collider == null)
            return true;
        return !collider.isActiveAndEnabled;
    }

    public void SetupScene()
    {
        BoardSetup();
    }


}
