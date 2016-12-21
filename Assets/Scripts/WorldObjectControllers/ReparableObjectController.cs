using System;
using UnityEngine;
using System.Collections;

public class ReparableObjectController : WorldObjectController
{
    public enum Status
    {
        CompletelyRepaird,
        Broken,
        PartiallyRepaired,
    };

    public bool isGameOn { get; set; }
    public Status status { get; set; }

    // Use this for initialization
    void Start()
    {
        status = Status.Broken;
        isGameOn = false;
    }


    // Update is called once per frame
    void Update()
    {

    }

    public virtual void PlayMiniGame()
    {
        //isGameOn = true;
        //Minigame returns status of object 
        //this.status=
    }

    public override bool Interact(Coords2 coords, GameObject player)
    {
        PlayMiniGame();
        //If we finish playing mini game we chage status to false 
        isGameOn = false;
        return true;
    }
    public override Coords2[] GetDummiesToCreate()
    {
        return new Coords2[0];
    }
}