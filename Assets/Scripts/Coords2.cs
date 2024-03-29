﻿using UnityEngine;
using System.Collections;
using System;

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

    public static Coords2 operator*(Coords2 c, int s)
    {
        return new Coords2(c.x * s, c.y * s);
    }

    public static Coords2 operator-(Coords2 c)
    {
        return new Coords2(-c.x, -c.y);
    }
    public static bool operator==(Coords2 c1, Coords2 c2)
    {
        return c1.x == c2.x && c1.y == c2.y;
    }
    public static bool operator!=(Coords2 c1, Coords2 c2)
    {
        return c1.x != c2.x || c1.y != c2.y;
    }

    public Coords2 Abs()
    {
        return new Coords2(Mathf.Abs(x), Mathf.Abs(y));
    }
}
