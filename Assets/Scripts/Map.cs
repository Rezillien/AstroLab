using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public int width = 18;
    public int height = 14;

    private GameObject[,] floorLayerTiles;
    private GameObject[,] wallLayerTiles;

    //generates map using given generator
    void GenerateMap(MapGenerator mapGenerator)
    {
        floorLayerTiles = new GameObject[width, height];
        wallLayerTiles = new GameObject[width, height];

        mapGenerator.Generate(this);
    }

    public bool HasCollider(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return false;

        GameObject tile = wallLayerTiles[x, y];
        if (tile == null)
            return false;

        WallTileController controller = tile.GetComponent<WallTileController>();
        if (controller == null)
        {
            return tile.GetComponent<BoxCollider2D>() != null;
        }

        return controller.HasCollider();
    }

    public bool Interact(int x, int y, GameObject player)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return false;

        GameObject tile = wallLayerTiles[x, y];
        if (tile == null) return false;

        WallTileController controller = tile.GetComponent<WallTileController>();
        if (controller == null) return false;

        return tile.GetComponent<WallTileController>().Interact(player);
    }

    //sets reference
    public void SetFloorTile(int x, int y, GameObject tile)
    {
        floorLayerTiles[x, y] = tile;
    }

    public void SetWallTile(int x, int y, GameObject tile)
    {
        wallLayerTiles[x, y] = tile;
    }

    // instantiates!!! and sets reference
    public void CreateFloorTile(int x, int y, GameObject tile)
    {
        floorLayerTiles[x, y] = Instantiate(tile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
    }

    public void CreateWallTile(int x, int y, GameObject tile)
    {
        wallLayerTiles[x, y] = Instantiate(tile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
    }

    public void Setup(MapGenerator mapGenerator)
    {
        GenerateMap(mapGenerator);
    }
}
