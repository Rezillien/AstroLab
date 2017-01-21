using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.SceneManagement;

public class EngineObjectController : ReparableObjectController
{   

    public override void PlayMiniGame()
    {
        isGameOn = true;

        SceneManager.LoadScene("Minigame1", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Minigame1"));
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("Minigame1"));
        //Minigame returns status of object 
    
    public override bool Interact(Coords2 coords, Player player)
    {
        PlayMiniGame();

        return true;
    }

}
