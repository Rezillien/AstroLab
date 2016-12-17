using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    //events
    public delegate void WallTileChangedEventHandler(Coords2 coords);
    public event WallTileChangedEventHandler OnWallTileChanged;

    public int width;
    public int height;

    public int startX = 1;
    public int startY = 1;

    private GameObject[,] floorTileLayer;
    private GameObject[,] wallTileLayer;
    private GameObject[,] worldObjectLayer;

    //generates map using given generator
    void GenerateMap(MapGenerator mapGenerator)
    {
        floorTileLayer = new GameObject[width, height];
        wallTileLayer = new GameObject[width, height];
        worldObjectLayer = new GameObject[width, height];

        mapGenerator.Generate(this);

        PlayerMovement player = GameManager.instance.GetPlayer();
        player.SetPosition(startX, startY);
    }

    public bool IsInsideBounds(Coords2 coords)
    {
        return coords.x >= 0 && coords.y >= 0 && coords.x < width && coords.y < height;
    }

    //return true if tile has boxcollider if wall controller is not present or value specified by wallcontroller
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

    //return 1.0f if tile has boxcollider if wall controller is not present or value specified by wallcontroller
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

    //emit event
    private void WallTileChanged(Coords2 coords)
    {
        if (OnWallTileChanged != null) OnWallTileChanged(coords);
    }

    //requires wall to have wallcontroler
    private bool InteractWall(Coords2 coords, GameObject player)
    {
        GameObject tile = GetWallTile(coords);
        if (tile == null) return false;

        WallTileController controller = tile.GetComponent<WallTileController>();
        if (controller == null) return false;

        bool interacted = controller.Interact(coords, player);
        if (interacted) WallTileChanged(coords);
        return interacted;
    }

    //requires object to have worldobjectcontroller
    //should be called when wallobjectcontroller.interact returns false or is not called
    private bool InteractObject(Coords2 coords, GameObject player)
    {
        GameObject tile = GetWorldObject(coords);
        if (tile == null) return false;

        WorldObjectController controller = tile.GetComponent<WorldObjectController>();
        if (controller == null) return false;

        return controller.Interact(coords, player);
    }
    
    public bool Interact(Coords2 coords, GameObject player)
    {
        return InteractWall(coords, player) || InteractObject(coords, player);
    }

    //only sets reference to passed tile
    private GameObject SetTile(Coords2 coords, GameObject tile, GameObject[,] layer)
    {
        if (!IsInsideBounds(coords)) return null;

        RemoveTile(coords, layer);
        layer[coords.x, coords.y] = tile;
        return tile;
    }

    public GameObject SetFloorTile(Coords2 coords, GameObject tile)
    {
        return SetTile(coords, tile, floorTileLayer);
    }

    public GameObject SetWallTile(Coords2 coords, GameObject tile)
    {
        GameObject placedTile = SetTile(coords, tile, wallTileLayer);
        WallTileChanged(coords);
        return placedTile;
    }
    public GameObject SetWorldObject(Coords2 coords, GameObject tile)
    {
        return SetTile(coords, tile, worldObjectLayer);
    }

    // instantiates!!! and sets reference
    public GameObject CreateFloorTile(Coords2 coords, GameObject tile)
    {
        return SetFloorTile(coords, Instantiate(tile, new Vector3(coords.x, coords.y, 0f), Quaternion.identity) as GameObject);
    }

    public GameObject CreateWallTile(Coords2 coords, GameObject tile)
    {
        return SetWallTile(coords, Instantiate(tile, new Vector3(coords.x, coords.y, 0f), Quaternion.identity) as GameObject);
    }
    public GameObject CreateWorldObject(Coords2 coords, GameObject tile)
    {
        return SetWorldObject(coords, Instantiate(tile, new Vector3(coords.x, coords.y, 0f), Quaternion.identity) as GameObject);
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
    public void RemoveWorldObject(Coords2 coords)
    {
        RemoveTile(coords, worldObjectLayer);
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
    public GameObject GetWorldObject(Coords2 coords)
    {
        return GetTile(coords, worldObjectLayer);
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
