using System;
using UnityEngine;
using System.Collections;

public class ServerObjectController : ReparableObjectController {

    void PlayMiniGame()
    {
        isGameOn = true;
        //Just for purposes if it's working 
        throw new NullReferenceException("ale bieda ;(");
        //Tutaj bd wywolywac gierke
    }
    public override bool Interact(Coords2 coords, GameObject player)
    {

        PlayMiniGame();
        //If we finish playing mini game we chage status to false 
        isGameOn = false;
        return true;
    }
}
