using UnityEngine;
using System.Collections;

public class DoorController : WallTileController
{

    public Sprite closedSprite;
    public Sprite openedSprite;
    public bool isOpen;

    // Use this for initialization
    void Start()
    {
        isOpen = false;
        GetComponent<SpriteRenderer>().sprite = closedSprite;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Toggles between open/closed, returns true indicating that some action was performed
    public override bool Interact(GameObject player)
    {
        isOpen = !isOpen;

        Sprite spriteToUse;
        if (isOpen) spriteToUse = openedSprite;
        else spriteToUse = closedSprite;

        GetComponent<SpriteRenderer>().sprite = spriteToUse;

        return true;
    }

    //returns if the player is unable to stand on it
    public override bool HasCollider()
    {
        return !isOpen;
    }
}
