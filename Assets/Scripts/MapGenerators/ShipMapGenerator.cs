using UnityEngine;
using System.Collections.Generic;

public class ShipMapGenerator : MapGenerator
{
    enum Door
    {
        None,
        Horizontal,
        Vertical
    }


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

    private class Connection
    {
        public Coords2 initialPosition;
        public Coords2 finalPosition;
        public Coords2 direction;
        public int initialRegionId;
        public int finalRegionId;
        public int length;

        public Connection(Coords2 initialPosition, Coords2 finalPosition, Coords2 direction, int initialRegionId, int finalRegionId, int length)
        {
            this.initialPosition = initialPosition;
            this.finalPosition = finalPosition;
            this.direction = direction;
            this.initialRegionId = initialRegionId;
            this.finalRegionId = finalRegionId;
            this.length = length;
        }
    }


    public int polygonSize = 64;
    //in tile sizes
    public float minDepth = 10.0f;
    public float noiseSampleRange = 10000.0f;
    public bool isSymetrical = true;
    public int minRoomSize = 6;
    public int maxRoomSize = 60;

    private int width;
    private int height;
    private bool[,] isWall;
    private int[,] regionIds;
    private int nextRegionId;

    GameObject[] floorTilePrefabs;
    GameObject[] verticalDoorTilePrefabs;
    GameObject[] horizontalDoorTilePrefabs;

    GameObject wallTilePrefab;
    GameObject[,] floorLayerTiles;
    GameObject[,] wallLayerTiles;
    GameObject[,] worldObjectLayer;

    Door[,] doors;

    Map map;

    private GameObject RandomizeTile(GameObject[] tiles)
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    public override void Generate(Map map)
    {
        Prefabs prefabs = GameManager.instance.GetPrefabs();

        this.map = map;

        width = map.width;
        height = map.height;

        //TEMP only use first wall because others don't have connected textures
        wallTilePrefab = prefabs.wallTilePrefabs[0];
        floorTilePrefabs = prefabs.floorTilePrefabs;
        verticalDoorTilePrefabs = prefabs.verticalDoorTilePrefabs;
        horizontalDoorTilePrefabs = prefabs.horizontalDoorTilePrefabs;

        floorLayerTiles = new GameObject[width, height];
        wallLayerTiles = new GameObject[width, height];
        worldObjectLayer = new GameObject[width, height];
        FillLayer(floorLayerTiles, null);
        FillLayer(wallLayerTiles, null);
        FillLayer(worldObjectLayer, null);

        Vector2[] boundingPolygon = GenerateRandomPolygon();
        isWall = new bool[width, height];
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                isWall[i, j] = false;
            }
        }


        MarkBorder(boundingPolygon);
        CloseDiagonals(); //has to be done here (and later) to maintain ship shape

        FillFloorTilesInside(new Coords2(width / 2, height / 2));

        SubdivideRecursively();
        FillSmallRooms();
        CloseDiagonals();

        CreatePossibleRoomConnections();

        FillWallTilesOnBoundaries();

        InstantiateTiles();


        map.startX = width / 2;
        map.startY = height / 2;
    }

    private void CreatePossibleRoomConnections()
    {
        FillRegionIds();

        List<Connection>[] connections = new List<Connection>[nextRegionId]; //0th is unused because it's outside the ship
        for (int i = 0; i < nextRegionId; ++i)
        {
            connections[i] = new List<Connection>();
        }

        Coords2[] directions = { new Coords2(0, 1), new Coords2(0, -1), new Coords2(1, 0), new Coords2(-1, 0) };

        for (int x = 1; x < width - 1; ++x)
        {
            for (int y = 1; y < height - 1; ++y)
            {
                int currentRegionId = regionIds[x, y];
                if (currentRegionId < 1) continue;

                for (int d = 0; d < 4; ++d)
                {
                    Coords2 direction = directions[d];
                    int xx = x + direction.x;
                    int yy = y + direction.y;
                    if (isWall[xx, yy])
                    {
                        Coords2 start = new Coords2(x, y);
                        int connectionLength = TryMakeConnection(start, direction, currentRegionId);
                        if (connectionLength != 0)
                        {
                            Coords2 destination = start + direction * connectionLength;
                            int destinationRegionId = regionIds[destination.x, destination.y];
                            connections[currentRegionId].Add(new Connection(start, destination, direction, currentRegionId, destinationRegionId, connectionLength));
                        }
                    }
                }
            }
        }

        List<Connection> chosenConnections = ChooseConnections(connections);
        foreach(Connection con in chosenConnections)
        {
            PlaceDoorInsideConnection(con);
        }
    }

    private void PlaceDoorInsideConnection(Connection con)
    {
        int depth = Random.Range(1, con.length);
        Coords2 pos = con.initialPosition + con.direction * depth;
        wallLayerTiles[pos.x, pos.y] = horizontalDoorTilePrefabs[0];
        isWall[pos.x, pos.y] = false;
    }

    private List<Connection> ChooseConnections(List<Connection>[] allConnections)
    {
        List<Connection> chosenConnections = new List<Connection>();
        
        foreach(List<Connection> conlist in allConnections)
        {
            if (conlist.Count == 0) continue;
            chosenConnections.Add(conlist[Random.Range(0, conlist.Count)]);
        }

        return chosenConnections;
    }

    private int TryMakeConnection(Coords2 pos, Coords2 direction, int currentRegionId)
    {
        Coords2 perpToDirection = new Coords2(direction.y, direction.x);

        int length = 1;
        int x = pos.x + direction.x;
        int y = pos.y + direction.y;
        while (isInsideBounds(x, y) && regionIds[x, y] == -1)
        {
            Coords2 perp1 = new Coords2(x, y) + perpToDirection;
            Coords2 perp2 = new Coords2(x, y) - perpToDirection;
            if (!isInsideBounds(perp1.x, perp1.y) || !isWall[perp1.x, perp1.y]) return 0; //connection must have walls on each side through whole length
            if (!isInsideBounds(perp2.x, perp2.y) || !isWall[perp2.x, perp2.y]) return 0; 

            ++length;
            x += direction.x;
            y += direction.y;
        }

        if (!isInsideBounds(x, y) || regionIds[x, y] == 0) return 0;

        return length;
    }

    private void CloseDiagonals()
    {
        for (int x = 0; x < width - 1; ++x)
        {
            for (int y = 0; y < height - 1; ++y)
            {
                bool b00 = isWall[x, y];
                bool b01 = isWall[x, y + 1];
                bool b10 = isWall[x + 1, y];
                bool b11 = isWall[x + 1, y + 1];
                if ((b00 && b11 && !b01 && !b10) || (!b00 && !b11 && b01 && b10))
                {
                    if (!b00) isWall[x, y] = true;
                    else if (!b01) isWall[x, y + 1] = true;
                    else if (!b10) isWall[x + 1, y] = true;
                    else if (!b11) isWall[x + 1, y + 1] = true;

                }
            }
        }
    }

    private void FillSmallRooms()
    {
        List<Room> rooms = GetAllRooms();
        foreach (Room room in rooms)
        {
            if (room.tiles.Count < minRoomSize)
            {
                foreach (Coords2 pos in room.tiles)
                {
                    isWall[pos.x, pos.y] = true;
                }
            }
        }
    }

    private List<Room> GetAllRooms()
    {
        bool[,] isWallLocal = new bool[width, height];
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                isWallLocal[x, y] = isWall[x, y];
            }
        }

        List<Room> rooms = new List<Room>();
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (floorLayerTiles[x, y] != null && !isWallLocal[x, y])
                {
                    List<Coords2> tilesInsideRoom = new List<Coords2>();
                    GetTilesInsideRoomAndFill(tilesInsideRoom, new Coords2(x, y), isWallLocal);
                    rooms.Add(new Room(tilesInsideRoom));

                }
            }
        }

        return rooms;
    }

    private void FillRegionIds()
    {
        regionIds = new int[width, height];
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                regionIds[x, y] = (isWall[x, y] ? -1 : 0); //0 is a region outside the ship, -1 is a wall

            }
        }
        List<Room> rooms = GetAllRooms();
        int currentId = 1;
        foreach (Room room in rooms)
        {
            foreach (Coords2 tile in room.tiles)
            {
                regionIds[tile.x, tile.y] = currentId;
            }

            ++currentId;
        }

        nextRegionId = currentId;
    }
    private void SubdivideRecursively()
    {
        List<Room> rooms = GetAllRooms();
        foreach (Room room in rooms)
        {
            SubdivideRecursively(room);
        }
    }

    private void SubdivideRecursively(Room room)
    {
        if (room.tiles.Count < minRoomSize) return;
        if (room.tiles.Count < maxRoomSize) //randomly end subdivision
        {
            AABB boundingBox = BoundingBox(room);
            int roomWidth = boundingBox.width();
            int roomHeight = boundingBox.height();
            float ratio = (float)Mathf.Max(roomWidth, roomHeight) / Mathf.Min(roomWidth, roomHeight);
            if (ratio < 1.6) return; //prevents from subdividing corridors

            int r = Random.Range(0, maxRoomSize);
            if (r > room.tiles.Count) return;
        }
        List<Room> createdRooms = TrySubdivide(room);
        foreach (Room createdRoom in createdRooms)
        {
            SubdivideRecursively(createdRoom);
        }
    }

    private bool isInsideBounds(int x, int y)
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

    private List<Room> TrySubdivide(Room room)
    {
        Coords2 divisionPoint = SampleClosePoint(room.tiles, AveragePoint(room.tiles), Mathf.Max(1, Mathf.RoundToInt(Mathf.Pow(room.tiles.Count, 0.3f))));

        Coords2[,] directionCombinations =
        {
            //first direction, second direction
            { new Coords2(0, 1), new Coords2(1, 0) },
            { new Coords2(0, 1), new Coords2(0, -1) },
            { new Coords2(0, 1), new Coords2(-1, 0) },
            { new Coords2(1, 0), new Coords2(0, -1) },
            { new Coords2(1, 0), new Coords2(-1, 0) },
            { new Coords2(0, -1), new Coords2(-1, 0) }
        };

        int r = Random.Range(0, 6); //6 combinations
        Coords2 d1 = directionCombinations[r, 0];
        Coords2 d2 = directionCombinations[r, 1];

        int dx = 0;
        int dy = 0;
        while (true)
        {
            int xx = divisionPoint.x + dx;
            int yy = divisionPoint.y + dy;
            if (isWall[xx, yy]) break;

            isWall[xx, yy] = true;
            dx += d1.x;
            dy += d1.y;
        }

        dx = d2.x; //to not check the same tile twice
        dy = d2.y;
        while (true)
        {
            int xx = divisionPoint.x + dx;
            int yy = divisionPoint.y + dy;
            if (isWall[xx, yy]) break;

            isWall[xx, yy] = true;
            dx += d2.x;
            dy += d2.y;
        }

        bool[,] isWallLocal = new bool[width, height];
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                isWallLocal[x, y] = isWall[x, y];
            }
        }

        List<Room> createdRooms = new List<Room>();
        foreach (Coords2 tile in room.tiles)
        {
            List<Coords2> tilesInside = new List<Coords2>();
            GetTilesInsideRoomAndFill(tilesInside, tile, isWallLocal); //modifies isBoundaryLocal
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

    private void InstantiateTiles()
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

    private void GetTilesInsideRoomAndFill(List<Coords2> tiles, Coords2 pos, bool[,] isWall)
    {
        int x = pos.x;
        int y = pos.y;
        if (isWall[x, y])
        {
            return;
        }
        else
        {
            isWall[x, y] = true;

            tiles.Add(new Coords2(x, y));
            if (x > 0) GetTilesInsideRoomAndFill(tiles, new Coords2(x - 1, y), isWall);
            if (y > 0) GetTilesInsideRoomAndFill(tiles, new Coords2(x, y - 1), isWall);
            if (x < width - 1) GetTilesInsideRoomAndFill(tiles, new Coords2(x + 1, y), isWall);
            if (y < height - 1) GetTilesInsideRoomAndFill(tiles, new Coords2(x, y + 1), isWall);
        }
    }

    private List<Coords2> GetTilesInsideRoom(Coords2 origin)
    {
        if (!isInsideBounds(origin.x, origin.y)) return new List<Coords2>();

        bool[,] isWallLocal = new bool[width, height];
        List<Coords2> tiles = new List<Coords2>();
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                isWallLocal[x, y] = isWall[x, y];
            }
        }

        GetTilesInsideRoomAndFill(tiles, origin, isWallLocal);

        return tiles;
    }

    private void FillFloorTilesInside(Coords2 origin)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (isWall[x, y])
                {
                    floorLayerTiles[x, y] = RandomizeTile(floorTilePrefabs);
                }
            }
        }

        foreach (Coords2 pos in GetTilesInsideRoom(new Coords2(origin.x, origin.y)))
        {
            floorLayerTiles[pos.x, pos.y] = RandomizeTile(floorTilePrefabs);
        }
    }

    private void FillWallTilesOnBoundaries()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (isWall[x, y])
                {
                    wallLayerTiles[x, y] = wallTilePrefab;
                }
            }
        }
    }

    private void FillLayer(GameObject[,] layer, GameObject prefab)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                layer[x, y] = prefab;
            }
        }
    }

    private Vector2[] GenerateRandomPolygon()
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

    private void MarkBorder(Vector2[] polygon)
    {
        for (int i = 0; i < polygon.Length; ++i)
        {
            Vector2 begin = polygon[i];
            Vector2 end = polygon[(i + 1) % polygon.Length];
            MarkBorder(begin, end);
        }
    }

    private void MarkBorder(Vector2 begin, Vector2 end)
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
                isWall[x, yi] = true;
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
                isWall[xi, y] = true;
            }
        }
    }
}
