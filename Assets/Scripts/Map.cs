using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public delegate void WallTileChangedEventHandler(Coords2 coords);
    public event WallTileChangedEventHandler OnWallTileChanged;

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

    public bool IsInsideBounds(Coords2 coords)
    {
        return coords.x >= 0 && coords.y >= 0 && coords.x < width && coords.y < height;
    }

    public bool HasCollider(Coords2 coords)
    {
        GameObject tile = GetWallTile(coords);
        if (tile == null)
            return false;

        WallTileController controller = tile.GetComponent<WallTileController>();
        if (controller == null)
        {
            return tile.GetComponent<BoxCollider2D>() != null;
        }

        return controller.HasCollider();
    }
    public float Opacity(Coords2 coords)
    {
        GameObject tile = GetWallTile(coords);
        if (tile == null)
            return 0.0f;

        WallTileController controller = tile.GetComponent<WallTileController>();
        if (controller == null)
        {
            return tile.GetComponent<BoxCollider2D>() != null ? 1.0f : 0.0f;
        }

        return controller.Opacity();
    }

    private void WallTileChanged(Coords2 coords)
    {
        if (OnWallTileChanged != null) OnWallTileChanged(coords);
    }

    public bool Interact(Coords2 coords, GameObject player)
    {
        GameObject tile = GetWallTile(coords);
        if (tile == null) return false;

        WallTileController controller = tile.GetComponent<WallTileController>();
        if (controller == null) return false;

        bool interacted = tile.GetComponent<WallTileController>().Interact(player);
        if (interacted) WallTileChanged(coords);
        return interacted;
    }

    //sets reference
    private void SetTile(Coords2 coords, GameObject tile, GameObject[,] layer)
    {
        if (!IsInsideBounds(coords)) return;

        RemoveTile(coords, layer);
        layer[coords.x, coords.y] = tile;
    }

    //sets reference
    public void SetFloorTile(Coords2 coords, GameObject tile)
    {
        SetTile(coords, tile, floorTileLayer);
    }

    public void SetWallTile(Coords2 coords, GameObject tile)
    {
        SetTile(coords, tile, wallTileLayer);
        WallTileChanged(coords);
    }

    // instantiates!!! and sets reference
    public void CreateFloorTile(Coords2 coords, GameObject tile)
    {
        SetFloorTile(coords, Instantiate(tile, new Vector3(coords.x, coords.y, 0f), Quaternion.identity) as GameObject);
    }

    public void CreateWallTile(Coords2 coords, GameObject tile)
    {
        SetWallTile(coords, Instantiate(tile, new Vector3(coords.x, coords.y, 0f), Quaternion.identity) as GameObject);
    }

    private void RemoveTile(Coords2 coords, GameObject[,] layer)
    {
        if (!IsInsideBounds(coords)) return;
        if (layer[coords.x, coords.y] == null) return;
        
        Destroy(layer[coords.x, coords.y]);
        layer[coords.x, coords.y] = null;
    }

    public void RemoveFloorTile(Coords2 coords)
    {
        RemoveTile(coords, floorTileLayer);
    }

    public void RemoveWallTile(Coords2 coords)
    {
        RemoveTile(coords, wallTileLayer);
    }

    private GameObject GetTile(Coords2 coords, GameObject[,] layer)
    {
        if (!IsInsideBounds(coords)) return null;

        return layer[coords.x, coords.y];
    }

    public GameObject GetFloorTile(Coords2 coords)
    {
        return GetTile(coords, floorTileLayer);
    }

    public GameObject GetWallTile(Coords2 coords)
    {
        return GetTile(coords, wallTileLayer);
    }

    public GameObject GetFloorTileNorthOf(Coords2 coords)
    {
        return GetTile(DirectionHelper.NorthOf(coords), floorTileLayer);
    }
    public GameObject GetWallTileNorthOf(Coords2 coords)
    {
        return GetTile(DirectionHelper.NorthOf(coords), wallTileLayer);
    }

    public GameObject GetFloorTileSouthOf(Coords2 coords)
    {
        return GetTile(DirectionHelper.SouthOf(coords), floorTileLayer);
    }
    public GameObject GetWallTileSouthOf(Coords2 coords)
    {
        return GetTile(DirectionHelper.SouthOf(coords), wallTileLayer);
    }

    public GameObject GetFloorTileWestOf(Coords2 coords)
    {
        return GetTile(DirectionHelper.WestOf(coords), floorTileLayer);
    }
    public GameObject GetWallTileWestOf(Coords2 coords)
    {
        return GetTile(DirectionHelper.WestOf(coords), wallTileLayer);
    }

    public GameObject GetFloorTileEastOf(Coords2 coords)
    {
        return GetTile(DirectionHelper.EastOf(coords), floorTileLayer);
    }
    public GameObject GetWallTileEastOf(Coords2 coords)
    {
        return GetTile(DirectionHelper.EastOf(coords), wallTileLayer);
    }

    public void Setup(MapGenerator mapGenerator)
    {
        GenerateMap(mapGenerator);
    }
}
