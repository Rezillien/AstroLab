using UnityEngine;
using System.Collections.Generic;

public class ShipMapGenerator : MapGenerator
{
    //holds coordinates of tiles strictly inside the room (bounded by walls)
    private class Room
    {
        public List<Coords2> tiles;

        public Room(List<Coords2> tiles)
        {
            this.tiles = tiles;
        }
    }

    private class RoomConnection
    {
        public Coords2 initialPosition;
        public Coords2 finalPosition;
        public Coords2 direction;
        public int initialRegionId;
        public int finalRegionId;
        public int length;

        public RoomConnection(Coords2 initialPosition, Coords2 finalPosition, Coords2 direction, int initialRegionId, int finalRegionId, int length)
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
    public float minRadius = 10.0f;
    public float noiseSampleRange = 10000.0f;
    public bool isSymetrical = true;
    public int minRoomSize = 6;
    public int maxRoomSize = 60;
    public int numberOfAdditionalWorkingConnections = 10;

    private int width; //these are used throughout the process of map generation to minimize number of parameters being passed
    private int height;
    private bool[,] isWall;
    private int[,] regionIds;
    private Room[] roomRegions;
    private float[] corridorFactors;
    private int nextRegionId;

    GameObject[] floorTilePrefabs;
    GameObject[] verticalDoorTilePrefabs;
    GameObject[] horizontalDoorTilePrefabs;

    GameObject wallTilePrefab;
    GameObject[,] floorLayerTiles;
    GameObject[,] wallLayerTiles;
    GameObject[,] worldObjectLayer;

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
        Util.Fill(floorLayerTiles, null);
        Util.Fill(wallLayerTiles, null);
        Util.Fill(worldObjectLayer, null);

        //generates boundaries of the ship contained inside map
        Vector2[] boundingPolygon = GenerateRandomPolygon();
        isWall = new bool[width, height];
        Util.Fill(isWall, false);

        //fill walls of the ship based on the generated polygon
        MarkBorder(boundingPolygon);

        // .X  -->  XX
        // X.       X.
        CloseDiagonalWalls(); //has to be done here (and later) to maintain ship shape

        //Fill the whole spaceship
        FillFloorTilesInsideRoom(new Coords2(width / 2, height / 2));

        //subdivide the spaceship to form rooms of desired size
        SubdivideRecursively();
        FillSmallRooms();
        //again, because subdivision may have created diagonal walls
        CloseDiagonalWalls();

        FillRegionIdsAndRoomRegions();

        //more processing should be placed here
        CalculateCorridorFactorsForRooms();

        CreateRoomConnections();

        FillWallTiles();

        InstantiateTiles();

        List<Room> allRooms = GetAllRooms(); //temporary
        Room randomRoom = allRooms[Random.Range(0, allRooms.Count)];
        Coords2 randomPos = randomRoom.tiles[Random.Range(0, randomRoom.tiles.Count)];

        map.startX = randomPos.x;
        map.startY = randomPos.y;

        map.AddPickupItem(
            AmmoPickup.CreateFromPrefab(
                prefabs.ammoPickupPrefab,
                new Vector2(randomPos.x, randomPos.y),
                24,
                0
            )
            );
    }

    private void CreateRoomConnections()
    {
        List<RoomConnection> connections = new List<RoomConnection>();

        Coords2[] directions = { new Coords2(0, 1), new Coords2(1, 0) }; //only 2 directions to omit repetitions

        //tries to 'carve' to the other room, saves the connection info if successful
        for (int x = 0; x < width - 1; ++x)
        {
            for (int y = 0; y < height - 1; ++y)
            {
                int currentRegionId = regionIds[x, y];
                if (currentRegionId < 1) continue;

                for (int d = 0; d < directions.Length; ++d)
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
                            connections.Add(new RoomConnection(start, destination, direction, currentRegionId, destinationRegionId, connectionLength));
                        }
                    }
                }
            }
        }

        List<RoomConnection> chosenConnections = ChooseConnections(connections);
        foreach (RoomConnection con in chosenConnections)
        {
            PlaceDoorInsideConnection(con);
        }
    }

    //removes walls and places door on random location on path
    private void PlaceDoorInsideConnection(RoomConnection con)
    {
        int length = con.length;
        Coords2 erasingPos = con.initialPosition;
        Coords2 direction = con.direction;

        int depth = Random.Range(1, length);
        for (int i = 1; i < length; ++i)
        {
            erasingPos += direction;
            isWall[erasingPos.x, erasingPos.y] = false;

        }
        Coords2 doorPos = con.initialPosition + direction * depth;

        GameObject[] prefabs = direction.x == 0 ? verticalDoorTilePrefabs : horizontalDoorTilePrefabs;
        wallLayerTiles[doorPos.x, doorPos.y] = RandomizeTile(prefabs);
    }

    //creates minimal spanning tree between rooms using kurskal's algorithm
    //adds specified number of additional connections to form cycles
    private List<RoomConnection> ChooseConnections(List<RoomConnection> allConnections)
    {

        List<RoomConnection> chosenConnections = new List<RoomConnection>();

        allConnections.Sort(
            delegate (RoomConnection c1, RoomConnection c2)
            {
                Coords2 v1 = c1.finalPosition - c1.initialPosition;
                Coords2 v2 = c2.finalPosition - c2.initialPosition;
                int len1 = v1.x + v1.y;
                int len2 = v2.x + v2.y;
                return len1.CompareTo(len2);
            }
            );

        HashSet<int>[] vertexSets = new HashSet<int>[nextRegionId];
        for (int i = 1; i < nextRegionId; ++i)
        {
            vertexSets[i] = new HashSet<int>();
            vertexSets[i].Add(i);
        }

        int currentConnectionIndex = 0;
        while (chosenConnections.Count < nextRegionId - 1 && currentConnectionIndex < allConnections.Count)
        {
            RoomConnection currentConnection = allConnections[currentConnectionIndex];

            int initialRegionId = currentConnection.initialRegionId;
            int finalRegionId = currentConnection.finalRegionId;

            if (vertexSets[initialRegionId] == vertexSets[finalRegionId])
            {
                ++currentConnectionIndex;
                continue; //compare references
            }

            chosenConnections.Add(currentConnection);
            foreach (int r in vertexSets[finalRegionId])
            {
                vertexSets[initialRegionId].Add(r);
                vertexSets[r] = vertexSets[initialRegionId]; //set all vertices in the set to have the same reference
            }

            ++currentConnectionIndex;
        }

        //adds some additional connections to form cycles
        currentConnectionIndex = 0;
        int additionalConnectionsPlaced = 0;
        while (additionalConnectionsPlaced < numberOfAdditionalWorkingConnections && currentConnectionIndex < allConnections.Count)
        {
            RoomConnection currentConnection = allConnections[currentConnectionIndex];

            int initialRegionId = currentConnection.initialRegionId;
            int finalRegionId = currentConnection.finalRegionId;

            bool sameFound = false;
            foreach (RoomConnection con in chosenConnections)
            {
                if ((con.initialRegionId == initialRegionId && con.finalRegionId == finalRegionId) ||
                   (con.finalRegionId == initialRegionId && con.initialRegionId == finalRegionId))
                {
                    sameFound = true;
                    break;
                }
            }

            if (!sameFound)
            {
                chosenConnections.Add(currentConnection);
                ++additionalConnectionsPlaced;
            }
            ++currentConnectionIndex;
        }

        return chosenConnections;
    }

    //tries to carve through walls
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

    private void CloseDiagonalWalls()
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
        bool[,] isWallLocal = isWall.Clone() as bool[,];

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

    //fills the regionIds array
    //0 - outside
    //-1 - wall
    //>0 - regions inside the ship
    private void FillRegionIdsAndRoomRegions()
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
        roomRegions = new Room[rooms.Count + 1]; //because acutal rooms are numbered starting with 1

        int currentId = 1;
        foreach (Room room in rooms)
        {
            foreach (Coords2 tile in room.tiles)
            {
                regionIds[tile.x, tile.y] = currentId;
            }
            roomRegions[currentId] = room;

            ++currentId;
        }

        nextRegionId = currentId;
    }

    private void CalculateCorridorFactorsForRooms()
    {
        corridorFactors = new float[roomRegions.Length];

        for (int i = 1; i < roomRegions.Length; ++i)
        {
            Room room = roomRegions[i];
            corridorFactors[i] = CalculateCorridorFactor(room);
        }
    }

    private float CalculateCorridorFactor(Room room)
    {
        int neighbourThreshold = 6;
        int corridorTileCount = 0;

        foreach (Coords2 tile in room.tiles)
        {
            if (WallNeighbours(tile) >= neighbourThreshold)
                corridorTileCount += 1;
        }

        return (float)corridorTileCount / room.tiles.Count;
    }

    private int WallNeighbours(Coords2 tile)
    {
        int wallNeighbourCount = 0;

        int minX = Mathf.Max(0, tile.x - 1);
        int minY = Mathf.Max(0, tile.y - 1);
        int maxX = Mathf.Max(width - 1, tile.x + 1);
        int maxY = Mathf.Max(height - 1, tile.y + 1);

        for (int x = minX; x <= maxX; ++x)
        {
            for (int y = minY; y <= maxY; ++y)
            {
                if (x == tile.x && y == tile.y) continue;

                if (isWall[x, y])
                    wallNeighbourCount += 1;
            }
        }

        return wallNeighbourCount;
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

        //directions which will span the new walls
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

        //span the new walls until another wall is hit
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

        bool[,] isWallLocal = isWall.Clone() as bool[,];

        //retrieve newly created rooms
        List<Room> createdRooms = new List<Room>();
        foreach (Coords2 tile in room.tiles)
        {
            List<Coords2> tilesInside = new List<Coords2>();
            GetTilesInsideRoomAndFill(tilesInside, tile, isWallLocal); //modifies isWallLocal
            if (tilesInside.Count > 0)
            {
                createdRooms.Add(new Room(tilesInside));
            }
        }
        return createdRooms;
    }

    private Vector2 AveragePoint(List<Coords2> points) //this is not equivalent to center of mass
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

    //samples n discrete points and returns the closest one to the reference point
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

    //instantiates tiles on the map
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

    private void GetTilesInsideRoomAndFill(List<Coords2> tiles, Coords2 startPos, bool[,] isWall)
    {
        Stack<Coords2> callStack = new Stack<Coords2>();
        callStack.Push(new Coords2(startPos.x, startPos.y));

        while (callStack.Count > 0)
        {
            Coords2 pos = callStack.Pop();
            int x = pos.x;
            int y = pos.y;

            isWall[x, y] = true;

            tiles.Add(new Coords2(x, y));

            if (x > 0 && !isWall[x - 1, y]) callStack.Push(new Coords2(x - 1, y));
            if (y > 0 && !isWall[x, y - 1]) callStack.Push(new Coords2(x, y - 1));
            if (x < width - 1 && !isWall[x + 1, y]) callStack.Push(new Coords2(x + 1, y));
            if (y < height - 1 && !isWall[x, y + 1]) callStack.Push(new Coords2(x, y + 1));
        }
    }

    private List<Coords2> GetTilesInsideRoom(Coords2 origin)
    {
        if (!isInsideBounds(origin.x, origin.y)) return new List<Coords2>();

        bool[,] isWallLocal = isWall.Clone() as bool[,];
        List<Coords2> tiles = new List<Coords2>();

        GetTilesInsideRoomAndFill(tiles, origin, isWallLocal);

        return tiles;
    }

    //fills tiles inside the room and ALL walls
    private void FillFloorTilesInsideRoom(Coords2 origin)
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

    private void FillWallTiles()
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

    //generates pseudo random polygon inside map's bounds using perlin noise
    private Vector2[] GenerateRandomPolygon()
    {
        Vector2[] polygon = new Vector2[polygonSize];

        float maxRadius = Mathf.Min(width / 2 - 1, height / 2 - 1);
        float sampleRadius = 1.0f;
        //in case when width != height to maintain map ratio
        float widthMul = width / (float)Mathf.Min(width, height);
        float heightMul = height / (float)Mathf.Min(width, height);

        //choose origin for samples on 'perlin noise value space'
        Vector2 sampleOrigin = new Vector2(
            Random.Range(-noiseSampleRange, noiseSampleRange),
            Random.Range(-noiseSampleRange, noiseSampleRange)
            );
        Vector2 boardCenter = new Vector2(width / 2.0f, height / 2.0f);

        for (int i = 0; i < polygonSize; ++i)
        {
            float angle = 2 * Mathf.PI * ((float)i / (float)polygonSize) - Mathf.PI * 0.5f;
            //direction is rescaled to match the map's ratio
            Vector2 direction = new Vector2(Mathf.Cos(angle) * widthMul, Mathf.Sin(angle) * heightMul);
            if (isSymetrical && i > polygonSize / 2)
            {
                angle = Mathf.PI - angle;
            }
            Vector2 sampleDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 samplePos = sampleDirection * sampleRadius + sampleOrigin;
            float n = Mathf.PerlinNoise(samplePos.x, samplePos.y);
            n = 1.0f - n * n; //change distribution

            float radius = minRadius + (maxRadius - minRadius) * n;

            polygon[i] = direction * radius + boardCenter;

        }

        return polygon;
    }

    // rasterizes polygon onto isWall array
    private void MarkBorder(Vector2[] polygon)
    {
        for (int i = 0; i < polygon.Length; ++i)
        {
            Vector2 begin = polygon[i];
            Vector2 end = polygon[(i + 1) % polygon.Length];
            MarkBorder(begin, end);
        }
    }

    // rasterizes line segment onto isWall array
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
