using UnityEngine;
using System.Collections;

public static class Util
{
    public static void Fill<T>(T[,] array, T value)
    {
        for (int x = 0; x < array.GetLength(0); ++x)
        {
            for (int y = 0; y < array.GetLength(1); ++y)
            {
                array[x, y] = value;
            }
        }
    }
}
