using UnityEngine;
using System.Collections;

public struct Coords2 {

    public int x;
    public int y;

    public Coords2(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public static Coords2 operator +(Coords2 c1, Coords2 c2)
    {
        return new Coords2(c1.x + c2.x, c1.y + c2.y);
    }

    public static Coords2 operator -(Coords2 c1, Coords2 c2)
    {
        return new Coords2(c1.x - c2.x, c1.y - c2.y);
    }

}
