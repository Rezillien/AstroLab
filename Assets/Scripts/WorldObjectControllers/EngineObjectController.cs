using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

public class EngineObjectController : ReparableObjectController
{
    
    void  PlayMiniGame()
    {
        isGameOn = true;
        throw new NullReferenceException("Tutaj damy minigierke ;(");
        //Minigame returns status of object 
        //this.status=
    }
    
    public override bool Interact(Coords2 coords, GameObject player)
    {
        
        PlayMiniGame();
       
        return true;
    }
    
}
