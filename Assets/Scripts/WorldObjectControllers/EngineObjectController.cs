using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;

public class EngineObjectController : ReparableObjectController
{
    
    public override void PlayMiniGame()
    {
        isGameOn = true;
        throw new NullReferenceException("Tutaj damy minigierke ;(");
        //Minigame returns status of object 
        //this.status=
    }
    
    public override bool Interact(Coords2 coords, Player player)
    {
        
        PlayMiniGame();
       
        return true;
    }
    
}
