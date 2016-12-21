using System;
using UnityEngine;
using System.Collections;

public class ServerObjectController : ReparableObjectController {


    void Start()
    {
    }

    public override void PlayMiniGame()
    {
        isGameOn = true;
        //Just for purposes if it's working 
        throw new NullReferenceException("ale bieda ;(");
        //Tutaj bd wywolywac gierke
    }

    public override Coords2[] GetDummiesToCreate()
    {
        return new Coords2[3] { new Coords2(0, 1), new Coords2(1, 0), new Coords2(1, 1) };
    }
}
