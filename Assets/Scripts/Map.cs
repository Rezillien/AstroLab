using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public int width;
    public int height;

    private GameObject[,] floorTileLayer;
    private GameObject[,] wallTileLayer;

    //generates map using given generator
    void GenerateMap(MapGenerator mapGenerator)
    {
        floorTileLayer = new GameObject[width, height];
        wallTileLayer = new GameObject[width, height];

        mapGenerator.Generate(this);
    }

    public bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public bool HasCollider(int x, int y)
    {
        GameObject tile = GetWallTile(x, y);
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
        GameObject tile = GetWallTile(x, y);
        if (tile == null) return false;

        WallTileController controller = tile.GetComponent<WallTileController>();
        if (controller == null) return false;

        return tile.GetComponent<WallTileController>().Interact(player);
    }

    //sets reference
    private void SetTile(int x, int y, GameObject tile, GameObject[,] layer)
    {
        if (!IsInsideBounds(x, y)) return;

        RemoveTile(x, y, layer);
        layer[x, y] = tile;
    }

    //sets reference
    public void SetFloorTile(int x, int y, GameObject tile)
    {
        SetTile(x, y, tile, floorTileLayer);
    }

    public void SetWallTile(int x, int y, GameObject tile)
    {
        SetTile(x, y, tile, wallTileLayer);
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

    private void RemoveTile(int x, int y, GameObject[,] layer)
    {
        if (!IsInsideBounds(x, y)) return;
        if (layer[x, y] == null) return;

        Destroy(layer[x, y]);
        layer[x, y] = null;
    }

    public void RemoveFloorTile(int x, int y)
    {
        RemoveTile(x, y, floorTileLayer);
    }

    public void RemoveWallTile(int x, int y)
    {
        RemoveTile(x, y, wallTileLayer);
    }

    private GameObject GetTile(int x, int y, GameObject[,] layer)
    {
        if (!IsInsideBounds(x, y)) return null;

        return layer[x, y];
    }

    public GameObject GetFloorTile(int x, int y)
    {
        return GetTile(x, y, floorTileLayer);
    }

    public GameObject GetWallTile(int x, int y)
    {
        return GetTile(x, y, wallTileLayer);
    }

    public GameObject GetFloorTileNorthOf(int x, int y)
    {
        return GetTile(x, DirectionHelper.NorthOf(y), floorTileLayer);
    }
    public GameObject GetWallTileNorthOf(int x, int y)
    {
        return GetTile(x, DirectionHelper.NorthOf(y), wallTileLayer);
    }

    public GameObject GetFloorTileSouthOf(int x, int y)
    {
        return GetTile(x, DirectionHelper.SouthOf(y), floorTileLayer);
    }
    public GameObject GetWallTileSouthOf(int x, int y)
    {
        return GetTile(x, DirectionHelper.SouthOf(y), wallTileLayer);
    }

    public GameObject GetFloorTileWestOf(int x, int y)
    {
        return GetTile(DirectionHelper.WestOf(x), y, floorTileLayer);
    }
    public GameObject GetWallTileWestOf(int x, int y)
    {
        return GetTile(DirectionHelper.WestOf(x), y, wallTileLayer);
    }

    public GameObject GetFloorTileEastOf(int x, int y)
    {
        return GetTile(DirectionHelper.EastOf(x), y, floorTileLayer);
    }
    public GameObject GetWallTileEastOf(int x, int y)
    {
        return GetTile(DirectionHelper.EastOf(x), y, wallTileLayer);
    }

    public void Setup(MapGenerator mapGenerator)
    {
        GenerateMap(mapGenerator);
    }
}
