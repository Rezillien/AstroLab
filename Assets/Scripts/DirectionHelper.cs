using UnityEngine;
using System.Collections;

static public class DirectionHelper {

    static public int NorthOf(int y)
    {
        return y + 1;
    }
    static public int SouthOf(int y)
    {
        return y - 1;
    }
    static public int WestOf(int x)
    {
        return x - 1;
    }
    static public int EastOf(int x)
    {
        return x + 1;
    }
}
