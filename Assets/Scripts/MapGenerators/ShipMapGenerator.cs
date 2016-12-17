using UnityEngine;
using System.Collections.Generic;

public class ShipMapGenerator : MapGenerator
{
    public int polygonSize = 64;
    //in tile sizes
    public float minDepth = 10.0f;
    public float noiseSampleRange = 10000.0f;
    public bool isSymetrical = true;
    public int minRoomSize = 6;
    public int maxRoomSize = 60;

    private class Room
    {
        public List<Coords2> tiles;

        public Room(List<Coords2> tiles)
        {
            this.tiles = tiles;
        }
    }

    private class AABB
    {
        public int minX;
        public int minY;
        public int maxX;
        public int maxY;

        public AABB(int minX, int minY, int maxX, int maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }

        public int width()
        {
            return maxX - minX;
        }

        public int height()
        {
            return maxY - minY;
        }
    }

    private GameObject RandomizeTile(GameObject[] tiles)
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    public override void Generate(Map map)
    {
        Prefabs prefabs = GameManager.instance.GetPrefabs();

        int width = map.width;
        int height = map.height;

        //TEMP only use first wall because others don't have connected textures
        GameObject wallTilePrefab = prefabs.wallTilePrefabs[0];
        GameObject[] floorTilePrefabs = prefabs.floorTilePrefabs;
        GameObject[] verticalDoorTilePrefabs = prefabs.verticalDoorTilePrefabs;
        GameObject[] horizontalDoorTilePrefabs = prefabs.horizontalDoorTilePrefabs;

        GameObject[,] floorLayerTiles = new GameObject[width, height];
        GameObject[,] wallLayerTiles = new GameObject[width, height];
        GameObject[,] worldObjectLayer = new GameObject[width, height];
        FillLayer(floorLayerTiles, null, width, height);
        FillLayer(wallLayerTiles, null, width, height);
        FillLayer(worldObjectLayer, null, width, height);

        Vector2[] boundingPolygon = GenerateRandomPolygon(width, height);
        bool[,] isBoundary = new bool[width, height];
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                isBoundary[i, j] = false;
            }
        }
        MarkBorder(boundingPolygon, isBoundary);
        CloseDiagonals(isBoundary, width, height); //has to be done here (and later) bacause not closing diagonals may cause going outside of the main room

        FillFloorTilesInside(floorLayerTiles, floorTilePrefabs, isBoundary, new Coords2(width / 2, height / 2), width, height);

        SubdivideRecursively(floorLayerTiles, isBoundary, width, height);
        FillSmallRooms(floorLayerTiles, isBoundary, width, height);
        CloseDiagonals(isBoundary, width, height);

        FillWallTilesOnBoundaries(wallLayerTiles, wallTilePrefab, isBoundary, width, height);

        InstantiateTiles(map, floorLayerTiles, wallLayerTiles, worldObjectLayer);

        map.startX = width / 2;
        map.startY = height / 2;
    }

    private void CloseDiagonals(bool[,] isBoundary, int width, int height)
    {
        for (int x = 0; x < width - 1; ++x)
        {
            for (int y = 0; y < height - 1; ++y)
            {
                bool b00 = isBoundary[x, y];
                bool b01 = isBoundary[x, y + 1];
                bool b10 = isBoundary[x + 1, y];
                bool b11 = isBoundary[x + 1, y + 1];
                if ((b00 && b11 && !b01 && !b10) || (!b00 && !b11 && b01 && b10))
                {
                    if (!b00) isBoundary[x, y] = true;
                    else if (!b01) isBoundary[x, y + 1] = true;
                    else if (!b10) isBoundary[x + 1, y] = true;
                    else if (!b11) isBoundary[x + 1, y + 1] = true;

                }
            }
        }
    }

    private void FillSmallRooms(GameObject[,] floorLayer, bool[,] isBoundary, int width, int height)
    {
        List<Room> rooms = GetAllRooms(floorLayer, isBoundary, width, height);
        foreach (Room room in rooms)
        {
            if (room.tiles.Count < minRoomSize)
            {
                foreach (Coords2 pos in room.tiles)
                {
                    isBoundary[pos.x, pos.y] = true;
                }
            }
        }
    }

    private List<Room> GetAllRooms(GameObject[,] floorLayer, bool[,] isBoundary, int width, int height)
    {
        bool[,] isBoundaryCopy = new bool[width, height];
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                isBoundaryCopy[x, y] = isBoundary[x, y];
            }
        }

        List<Room> rooms = new List<Room>();
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (floorLayer[x, y] != null && !isBoundary[x, y])
                {
                    List<Coords2> tilesInsideRoom = new List<Coords2>();
                    GetTilesInsideRoom(tilesInsideRoom, new Coords2(x, y), isBoundaryCopy, width, height);
                    rooms.Add(new Room(tilesInsideRoom));

                }
            }
        }

        return rooms;
    }

    private void SubdivideRecursively(GameObject[,] floorLayer, bool[,] isBoundary, int width, int height)
    {
        List<Room> rooms = GetAllRooms(floorLayer, isBoundary, width, height);
        foreach (Room room in rooms)
        {
            SubdivideRecursively(room, isBoundary, width, height);
        }
    }

    private void SubdivideRecursively(Room room, bool[,] isBoundary, int width, int height)
    {
        if (room.tiles.Count < minRoomSize) return;
        if (room.tiles.Count < maxRoomSize) //randomly end subdivision
        {
            AABB boundingBox = BoundingBox(room);
            int roomWidth = boundingBox.width();
            int roomHeight = boundingBox.height();
            float ratio = (float)Mathf.Max(roomWidth, roomHeight) / (float)Mathf.Min(roomWidth, roomHeight);
            if (ratio < 1.1) return; //prevents from subdividing corridors

            int r = Random.Range(0, maxRoomSize);
            if (r > room.tiles.Count) return;
        }
        List<Room> createdRooms = TrySubdivide(room, isBoundary, width, height);
        foreach (Room createdRoom in createdRooms)
        {
            SubdivideRecursively(createdRoom, isBoundary, width, height);
        }
    }

    private bool isInsideBounds(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    private AABB BoundingBox(Room room)
    {
        int minX = room.tiles[0].x;
        int minY = room.tiles[0].y;
        int maxX = room.tiles[0].x;
        int maxY = room.tiles[0].y;

        foreach (Coords2 tile in room.tiles)
        {
            if (tile.x < minX) minX = tile.x;
            if (tile.y < minY) minY = tile.y;
            if (tile.x > maxX) maxX = tile.x;
            if (tile.y > maxY) maxY = tile.y;
        }

        return new AABB(minX, minY, maxX, maxY);
    }

    private List<Room> TrySubdivide(Room room, bool[,] isBoundary, int width, int height)
    {
        Coords2 divisionPoint = SampleClosePoint(room.tiles, AveragePoint(room.tiles), Mathf.Max(1, Mathf.RoundToInt(Mathf.Pow(room.tiles.Count, 0.3f))));

        Coords2[,] directionCombinations =
        {
            //first direction, second direction, point in one of the regions (-this is in second region)
            { new Coords2(0, 1), new Coords2(1, 0), new Coords2(1, 1)},
            { new Coords2(0, 1), new Coords2(0, -1), new Coords2(1, 0) },
            { new Coords2(0, 1), new Coords2(-1, 0), new Coords2(-1, 1) },
            { new Coords2(1, 0), new Coords2(0, -1), new Coords2(1, -1) },
            { new Coords2(1, 0), new Coords2(-1, 0), new Coords2(0, 1) },
            { new Coords2(0, -1), new Coords2(-1, 0), new Coords2(-1, -1) }
        };

        int r = Random.Range(0, 6); //6 combinations
        Coords2 d1 = directionCombinations[r, 0];
        Coords2 d2 = directionCombinations[r, 1];
        Coords2 rp = directionCombinations[r, 2];

        int dx = 0;
        int dy = 0;
        while (true)
        {
            int xx = divisionPoint.x + dx;
            int yy = divisionPoint.y + dy;
            if (isBoundary[xx, yy]) break;

            isBoundary[xx, yy] = true;
            dx += d1.x;
            dy += d1.y;
        }

        dx = d2.x; //to not check the same tile twice
        dy = d2.y;
        while (true)
        {
            int xx = divisionPoint.x + dx;
            int yy = divisionPoint.y + dy;
            if (isBoundary[xx, yy]) break;

            isBoundary[xx, yy] = true;
            dx += d2.x;
            dy += d2.y;
        }

        bool[,] isBoundaryLocal = new bool[width, height];
        for(int x = 0; x < width; ++x)
        {
            for(int y = 0; y < height; ++y)
            {
                isBoundaryLocal[x, y] = isBoundary[x, y];
            }
        }

        List<Room> createdRooms = new List<Room>();
        foreach (Coords2 tile in room.tiles)
        {
            List<Coords2> tilesInside = new List<Coords2>();
            GetTilesInsideRoom(tilesInside, tile, isBoundaryLocal, width, height); //modifies isBoundaryLocal
            if (tilesInside.Count > 0)
            {
                createdRooms.Add(new Room(tilesInside));
            }
        }
        return createdRooms;
    }

    private Vector2 AveragePoint(List<Coords2> points)
    {
        float sumX = 0.0f;
        float sumY = 0.0f;
        foreach (Coords2 pos in points)
        {
            sumX += pos.x;
            sumY += pos.y;
        }
        float averageX = sumX / points.Count;
        float averageY = sumY / points.Count;

        return new Vector2(averageX, averageY);
    }

    private Coords2 SampleClosePoint(List<Coords2> points, Vector2 reference, int numberOfSamples)
    {
        float minDistSquared = float.MaxValue;
        Coords2 min = new Coords2(0, 0);
        for (int i = 0; i < numberOfSamples; ++i)
        {
            Coords2 sample = points[Random.Range(0, points.Count)];
            float dx = reference.x - sample.x;
            float dy = reference.y - sample.y;
            float distSquared = dx * dx + dy * dy;

            if (distSquared < minDistSquared)
            {
                minDistSquared = distSquared;
                min = sample;
            }
        }

        return new Coords2(min.x, min.y);
    }

    private void InstantiateTiles(Map map, GameObject[,] floorLayerTiles, GameObject[,] wallLayerTiles, GameObject[,] worldObjectLayer)
    {
        int width = map.width;
        int height = map.height;
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                Coords2 coords = new Coords2(x, y);

                GameObject floorToInstantiate = floorLayerTiles[x, y];
                if (floorToInstantiate != null)
                {
                    map.CreateFloorTile(coords, floorToInstantiate);
                }

                GameObject wallToInstantiate = wallLayerTiles[x, y];
                if (wallToInstantiate != null)
                {
                    map.CreateWallTile(coords, wallToInstantiate);
                }
            }
        }

        //update wall sprites to connect after all walls are placed
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                GameObject wallTile = map.GetWallTile(new Coords2(x, y));
                if (wallTile != null)
                {
                    wallTile.GetComponent<WallTileController>().UpdateSprite(new Coords2(x, y));
                }
            }
        }
    }

    private void GetTilesInsideRoom(List<Coords2> tiles, Coords2 pos, bool[,] isBoundary, int width, int height)
    {
        int x = pos.x;
        int y = pos.y;
        if (isBoundary[x, y])
        {
            return;
        }
        else
        {
            isBoundary[x, y] = true;

            tiles.Add(new Coords2(x, y));
            if (x > 0) GetTilesInsideRoom(tiles, new Coords2(x - 1, y), isBoundary, width, height);
            if (y > 0) GetTilesInsideRoom(tiles, new Coords2(x, y - 1), isBoundary, width, height);
            if (x < width - 1) GetTilesInsideRoom(tiles, new Coords2(x + 1, y), isBoundary, width, height);
            if (y < height - 1) GetTilesInsideRoom(tiles, new Coords2(x, y + 1), isBoundary, width, height);
        }
    }

    private List<Coords2> GetTilesInsideRoom(Coords2 origin, bool[,] isBoundary, int width, int height)
    {
        if (!isInsideBounds(origin.x, origin.y, width, height)) return new List<Coords2>();

        bool[,] isBoundaryCopy = new bool[width, height];
        List<Coords2> tiles = new List<Coords2>();
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                isBoundaryCopy[x, y] = isBoundary[x, y];
            }
        }

        GetTilesInsideRoom(tiles, origin, isBoundaryCopy, width, height);

        return tiles;
    }

    private void FillFloorTilesInside(GameObject[,] layer, GameObject[] floorTilePrefabs, bool[,] isBoundary, Coords2 origin, int width, int height)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (isBoundary[x, y])
                {
                    layer[x, y] = RandomizeTile(floorTilePrefabs);
                }
            }
        }

        foreach (Coords2 pos in GetTilesInsideRoom(new Coords2(origin.x, origin.y), isBoundary, width, height))
        {
            layer[pos.x, pos.y] = RandomizeTile(floorTilePrefabs);
        }
    }

    private void FillWallTilesOnBoundaries(GameObject[,] layer, GameObject wallTilePrefab, bool[,] isBoundary, int width, int height)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (isBoundary[x, y])
                {
                    layer[x, y] = wallTilePrefab;
                }
            }
        }
    }

    private void FillLayer(GameObject[,] layer, GameObject prefab, int width, int height)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                layer[x, y] = prefab;
            }
        }
    }

    private Vector2[] GenerateRandomPolygon(int width, int height)
    {
        Vector2[] polygon = new Vector2[polygonSize];

        float maxDepth = Mathf.Min(width / 2 - 1, height / 2 - 1);
        float sampleRadius = 1.0f;
        Vector2 sampleOrigin = new Vector2(
            Random.Range(-noiseSampleRange, noiseSampleRange),
            Random.Range(-noiseSampleRange, noiseSampleRange)
            );
        Vector2 boardCenter = new Vector2(width / 2.0f, height / 2.0f);

        for (int i = 0; i < polygonSize; ++i)
        {
            float angle = 2 * Mathf.PI * ((float)i / (float)polygonSize) - Mathf.PI * 0.5f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (isSymetrical && i > polygonSize / 2)
            {
                angle = Mathf.PI - angle;
            }
            Vector2 sampleDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 samplePos = sampleDirection * sampleRadius + sampleOrigin;
            float n = Mathf.PerlinNoise(samplePos.x, samplePos.y);
            n = 1.0f - n * n; //change distribution

            float depth = minDepth + (maxDepth - minDepth) * n;

            polygon[i] = direction * depth + boardCenter;

        }

        return polygon;
    }

    private void MarkBorder(Vector2[] polygon, bool[,] isBorder)
    {
        for (int i = 0; i < polygon.Length; ++i)
        {
            Vector2 begin = polygon[i];
            Vector2 end = polygon[(i + 1) % polygon.Length];
            MarkBorder(begin, end, isBorder);
        }
    }

    private void MarkBorder(Vector2 begin, Vector2 end, bool[,] isBorder)
    {
        Vector2 v = end - begin;
        int minX = Mathf.RoundToInt(Mathf.Min(begin.x, end.x));
        int maxX = Mathf.RoundToInt(Mathf.Max(begin.x, end.x));
        int minY = Mathf.RoundToInt(Mathf.Min(begin.y, end.y));
        int maxY = Mathf.RoundToInt(Mathf.Max(begin.y, end.y));
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
        {
            float a = v.y / v.x;
            if (float.IsNaN(a)) return;
            float b = begin.y - a * begin.x;
            //y = ax + b
            for (int x = minX; x <= maxX; ++x)
            {
                float y = a * x + b;
                int yi = Mathf.RoundToInt(y);
                isBorder[x, yi] = true;
            }
        }
        else
        {
            float a = v.x / v.y;
            if (float.IsNaN(a)) return;
            float b = begin.x - a * begin.y;
            //x = ay + b
            for (int y = minY; y <= maxY; ++y)
            {
                float x = a * y + b;
                int xi = Mathf.RoundToInt(x);
                isBorder[xi, y] = true;
            }
        }
    }
}
