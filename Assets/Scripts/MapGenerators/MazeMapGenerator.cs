using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class MazeMapGenerator : MapGenerator
{
    private static int[,] moves = { { -2, 0 }, { 2, 0 }, { 0, -2 }, { 0, 2 } };
    public float doorChance = 0.1f;

    private GameObject RandomizeTile(GameObject[] tiles)
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    public override void Generate(Map map)
    {
        Prefabs prefabs = GameManager.instance.GetPrefabs();

        int width = map.width;
        int height = map.height;

        GameObject wallTilePrefab = prefabs.wallTilePrefabs[0];
        GameObject[] floorTilePrefabs = prefabs.floorTilePrefabs;
        GameObject[] verticalDoorTilePrefabs = prefabs.verticalDoorTilePrefabs;
        GameObject[] horizontalDoorTilePrefabs = prefabs.horizontalDoorTilePrefabs;

        GameObject[,] floorLayerTiles = new GameObject[width, height];
        GameObject[,] wallLayerTiles = new GameObject[width, height];
        GameObject[,] worldObjectLayer = new GameObject[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                floorLayerTiles[x, y] = RandomizeTile(floorTilePrefabs);
                wallLayerTiles[x, y] = wallTilePrefab;
                worldObjectLayer[x, y] = null;
            }
        }

        BacktrackCarve(map, wallLayerTiles);

        if (doorChance > 0.0f)
        {
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    if (wallLayerTiles[x, y] != null) continue;

                    float r = Random.Range(0.0f, 1.0f);
                    if (r < doorChance)
                    {
                        TryPlaceDoor(map, wallLayerTiles, horizontalDoorTilePrefabs, verticalDoorTilePrefabs, new Coords2(x, y));
                    }
                }
            }
        }

        worldObjectLayer[0, 1] = prefabs.cameraPrefab;
        worldObjectLayer[3, 4] = prefabs.enginePrefab;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                Coords2 coords = new Coords2(x, y);

                GameObject floorToInstantiate = floorLayerTiles[x, y];
                map.CreateFloorTile(coords, floorToInstantiate);

                GameObject wallToInstantiate = wallLayerTiles[x, y];
                if (wallToInstantiate != null)
                {
                    map.CreateWallTile(coords, wallToInstantiate);
                }

                GameObject worldObjectToInstantiate = worldObjectLayer[x, y];
                if (worldObjectToInstantiate != null)
                {
                   
                    CameraObjectController cameraController = map.CreateWorldObject(new Coords2(x, y), worldObjectToInstantiate).GetComponent<CameraObjectController>();
                    if(cameraController != null)
                    {
                        cameraController.SetOrigin(coords, new Vector2(x + 1.2f, y + 0.5f));
                        cameraController.SetAnchor(new Vector2(x + 0.5f, y - 0.2f));
                        cameraController.RotateAroundOrigin(45.0f);
                        cameraController.SetEnabled(coords, false);
                    }
                }
            }
        }

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                GameObject wallTile = map.GetWallTile(new Coords2(x, y));
                if(wallTile != null)
                {
                    wallTile.GetComponent<WallTileController>().UpdateSprite(new Coords2(x, y));
                }
            }
        }
    }

    private void TryPlaceDoor(Map map, GameObject[,] wallLayerTiles, GameObject[] horizontalDoorPrefabs, GameObject[] verticalDoorPrefabs, Coords2 coords)
    {
        if (coords.x <= 1 || coords.y <= 1 || coords.x >= map.width || coords.y >= map.height) return;

        Coords2 northCoords = DirectionHelper.NorthOf(coords);
        Coords2 southCoords = DirectionHelper.SouthOf(coords);
        Coords2 westCoords = DirectionHelper.WestOf(coords);
        Coords2 eastCoords = DirectionHelper.EastOf(coords);

        GameObject north = wallLayerTiles[northCoords.x, northCoords.y];
        GameObject south = wallLayerTiles[southCoords.x, southCoords.y];
        GameObject west = wallLayerTiles[westCoords.x, westCoords.y];
        GameObject east = wallLayerTiles[eastCoords.x, eastCoords.y];

        if (north != null && south != null)
        {
            if (west != null || east != null) return;

            wallLayerTiles[coords.x, coords.y] = RandomizeTile(horizontalDoorPrefabs);
        }
        else if (west != null && east != null)
        {
            if (north != null || south != null) return;

            wallLayerTiles[coords.x, coords.y] = RandomizeTile(verticalDoorPrefabs);
        }
    }

    private void BacktrackCarve(Map map, GameObject[,] wallLayerTiles)
    {
        BacktrackCarve(map, wallLayerTiles, new Coords2(1, 1));
    }

    private void BacktrackCarve(Map map, GameObject[,] wallLayerTiles, Coords2 coords)
    {
        int[] order = { 0, 1, 2, 3 };
        Shuffle(order);

        foreach (int i in order)
        {
            int nx = coords.x + moves[i, 0];
            int ny = coords.y + moves[i, 1];

            if (map.IsInsideBounds(new Coords2(nx, ny)))
            {
                if (wallLayerTiles[nx, ny] != null)
                {
                    wallLayerTiles[nx, ny] = null;
                    wallLayerTiles[coords.x + moves[i, 0] / 2, coords.y + moves[i, 1] / 2] = null;

                    BacktrackCarve(map, wallLayerTiles, new Coords2(nx, ny));
                }
            }
        }
    }

    private static void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; ++i)
            Swap(array, i, Random.Range(i, array.Length));
    }

    private static void Swap(int[] array, int i, int j)
    {
        int temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }
}
