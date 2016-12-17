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

    public SpriteRenderer sprite { get; set; }
    public bool isGameOn { get; set; }
    public Status status { get; set; }

    // Use this for initialization
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        status = Status.Broken;
        isGameOn = false;
    }


    // Update is called once per frame
    void Update()
    {

    }

    void PlayMiniGame()
    {
        //isGameOn = true;
        //Minigame returns status of object 
        //this.status=
    }

    public override bool Interact(Coords2 coords, GameObject player)
    {

        PlayMiniGame();
        //throw new NullReferenceException("xD");
        return true;
    }
}