using UnityEngine;
using System.Collections;

static public class DirectionHelper {

    static public Coords2 NorthOf(Coords2 coords)
    {
        return coords + new Coords2(0, 1);
    }
    static public Coords2 SouthOf(Coords2 coords)
    {
        return coords + new Coords2(0, -1);
    }
    static public Coords2 WestOf(Coords2 coords)
    {
        return coords + new Coords2(-1, 0);
    }
    static public Coords2 EastOf(Coords2 coords)
    {
        return coords + new Coords2(1, 0);
    }
}
