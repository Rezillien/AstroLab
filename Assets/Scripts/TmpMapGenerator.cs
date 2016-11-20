using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class TmpMapGenerator : MapGenerator
{
    public int minRoomSize = 5;
    //prefabs set in the inspector
    public GameObject[] wallTilePrefabs;
    public GameObject[] floorTilePrefabs;
    public GameObject[] verticalDoorTilePrefabs;
    public GameObject[] horizontalDoorTilePrefabs;

    private GameObject RandomizeTile(GameObject[] tiles)
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    public override void Generate(Map map)
    {
        int width = map.width;
        int height = map.height;

        GameObject[,] floorLayerTiles = new GameObject[width, height];
        GameObject[,] wallLayerTiles = new GameObject[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                floorLayerTiles[x, y] = RandomizeTile(floorTilePrefabs);
                wallLayerTiles[x, y] = null;

                map.SetFloorTile(x, y, null);
                map.SetWallTile(x, y, null);
            }
        }

        //random point where walls cross
        int crossX, crossY, missingWallOrientation;
        crossX = Random.Range(minRoomSize - 1, width - minRoomSize);
        crossY = Random.Range(minRoomSize - 1, height - minRoomSize);

        //selection of missing wall
        missingWallOrientation = Random.Range(0, 4);
        int horizontalWallMin = 1;
        int horizontalWallMax = width - 2;
        int verticalWallMin = 1;
        int verticalWallMax = height - 2;
        switch (missingWallOrientation)
        {
            case 0:
                verticalWallMin = crossY;
                break;
            case 1:
                horizontalWallMax = crossX;
                break;
            case 2:
                verticalWallMax = crossY;
                break;
            case 3:
                horizontalWallMin = crossX;
                break;
        }

        //placing walls
        for (int x = 0; x < width; ++x)
        {
            wallLayerTiles[x, 0] = RandomizeTile(wallTilePrefabs);
            wallLayerTiles[x, height - 1] = RandomizeTile(wallTilePrefabs);
            if (x >= horizontalWallMin && x <= horizontalWallMax)
                wallLayerTiles[x, crossY] = RandomizeTile(wallTilePrefabs);
        }
        for (int y = 0; y < height; ++y)
        {
            wallLayerTiles[0, y] = RandomizeTile(wallTilePrefabs);
            wallLayerTiles[width - 1, y] = RandomizeTile(wallTilePrefabs);
            if (y >= verticalWallMin && y <= verticalWallMax)
                wallLayerTiles[crossX, y] = RandomizeTile(wallTilePrefabs);
        }

        //selecting positions for doors
        int doorX = crossX;
        while (doorX == crossX) doorX = Random.Range(horizontalWallMin, horizontalWallMax + 1);
        int doorY = crossY;
        while (doorY == crossY) doorY = Random.Range(verticalWallMin, verticalWallMax + 1);

        //placing doors
        wallLayerTiles[doorX, crossY] = RandomizeTile(horizontalDoorTilePrefabs);
        wallLayerTiles[crossX, doorY] = RandomizeTile(verticalDoorTilePrefabs);

        //instantiating tiles
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                GameObject floorToInstantiate = floorLayerTiles[x, y];
                map.CreateFloorTile(x, y, floorToInstantiate);

                GameObject wallToInstantiate = wallLayerTiles[x, y];
                if (wallToInstantiate != null)
                {
                    map.CreateWallTile(x, y, wallToInstantiate);
                }
            }
        }
    }
}