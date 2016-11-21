using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public int width;
    public int height;

    private GameObject[,] floorLayerTiles;
    private GameObject[,] wallLayerTiles;

    //generates map using given generator
    void GenerateMap(MapGenerator mapGenerator)
    {
        floorLayerTiles = new GameObject[width, height];
        wallLayerTiles = new GameObject[width, height];

        mapGenerator.Generate(this);
    }

    public bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public bool HasCollider(int x, int y)
    {
        if (!IsInsideBounds(x, y)) return false;

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
        if (!IsInsideBounds(x, y)) return false;

        GameObject tile = wallLayerTiles[x, y];
        if (tile == null) return false;

        WallTileController controller = tile.GetComponent<WallTileController>();
        if (controller == null) return false;

        return tile.GetComponent<WallTileController>().Interact(player);
    }

    //sets reference
    public void SetFloorTile(int x, int y, GameObject tile)
    {
        if (!IsInsideBounds(x, y)) return;

        RemoveFloorTile(x, y);
        floorLayerTiles[x, y] = tile;
    }

    public void SetWallTile(int x, int y, GameObject tile)
    {
        if (!IsInsideBounds(x, y)) return;

        RemoveWallTile(x, y);
        wallLayerTiles[x, y] = tile;
    }

    // instantiates!!! and sets reference
    public void CreateFloorTile(int x, int y, GameObject tile)
    {
        SetFloorTile(x, y, Instantiate(tile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject);
    }

    public void CreateWallTile(int x, int y, GameObject tile)
    {
        SetWallTile(x, y, Instantiate(tile, new Vector3(x, y, 0f), Quaternion.identity) as GameObject);
    }

    public void RemoveFloorTile(int x, int y)
    {
        if (!IsInsideBounds(x, y)) return;
        if (floorLayerTiles[x, y] == null) return;

        Destroy(floorLayerTiles[x, y]);
        floorLayerTiles[x, y] = null;
    }

    public void RemoveWallTile(int x, int y)
    {
        if (!IsInsideBounds(x, y)) return;
        if (wallLayerTiles[x, y] == null) return;

        Destroy(wallLayerTiles[x, y]);
        wallLayerTiles[x, y] = null;
    }

    public GameObject GetFloorTile(int x, int y)
    {
        if (!IsInsideBounds(x, y)) return null;
        return floorLayerTiles[x, y];
    }

    public GameObject GetWallTile(int x, int y)
    {
        if (!IsInsideBounds(x, y)) return null;
        return wallLayerTiles[x, y];
    }

    public void Setup(MapGenerator mapGenerator)
    {
        GenerateMap(mapGenerator);
    }
}
