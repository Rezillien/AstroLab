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

        GameObject[] wallTilePrefabs = prefabs.wallTilePrefabs;
        GameObject[] floorTilePrefabs = prefabs.floorTilePrefabs;
        GameObject[] verticalDoorTilePrefabs = prefabs.verticalDoorTilePrefabs;
        GameObject[] horizontalDoorTilePrefabs = prefabs.horizontalDoorTilePrefabs;

        GameObject[,] floorLayerTiles = new GameObject[width, height];
        GameObject[,] wallLayerTiles = new GameObject[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                floorLayerTiles[x, y] = RandomizeTile(floorTilePrefabs);
                wallLayerTiles[x, y] = RandomizeTile(wallTilePrefabs);
            }
        }

        BacktrackCarve(map, wallLayerTiles, wallTilePrefabs);

        if(doorChance > 0.0f)
        {
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    if (wallLayerTiles[x, y] != null) continue;

                    float r = Random.Range(0.0f, 1.0f);
                    if (r < doorChance)
                    {
                        TryPlaceDoor(map, wallLayerTiles, horizontalDoorTilePrefabs, verticalDoorTilePrefabs, x, y);
                    }
                }
            }
        }
        
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

    private void TryPlaceDoor(Map map, GameObject[,] wallLayerTiles, GameObject[] horizontalDoorPrefabs, GameObject[] verticalDoorPrefabs, int x, int y)
    {
        if (x <= 1 || y <= 1 || x >= map.width || y >= map.height) return;

        GameObject north = wallLayerTiles[x, y + 1];
        GameObject south = wallLayerTiles[x, y - 1];
        GameObject west = wallLayerTiles[x - 1, y];
        GameObject east = wallLayerTiles[x + 1, y];
        if(north != null && south != null)
        {
            wallLayerTiles[x, y] = RandomizeTile(horizontalDoorPrefabs);
        }
        else if(west != null && east != null)
        {
            wallLayerTiles[x, y] = RandomizeTile(verticalDoorPrefabs);
        }
    }

    private void BacktrackCarve(Map map, GameObject[,] wallLayerTiles, GameObject[] wallPrefabs)
    {
        BacktrackCarve(map, wallLayerTiles, wallPrefabs, 1, 1);
    }

    private void BacktrackCarve(Map map, GameObject[,] wallLayerTiles, GameObject[] wallPrefabs, int x, int y)
    {
        int[] order = { 0, 1, 2, 3 };
        Shuffle(order);

        foreach(int i in order)
        {
            int nx = x + moves[i, 0];
            int ny = y + moves[i, 1];

            if(map.IsInsideBounds(nx, ny))
            {
                if(wallLayerTiles[nx, ny] != null)
                {
                    wallLayerTiles[nx, ny] = null;
                    wallLayerTiles[x + moves[i, 0] / 2, y + moves[i, 1] / 2] = null;

                    BacktrackCarve(map, wallLayerTiles, wallPrefabs, nx, ny);
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
