using UnityEngine;
using System.Collections;

public class DirtController : WallTileController
{

    public Sprite[] dirtSprite;

    private SpriteRenderer spriteRenderer;
    private int dirtState;
    private bool isDestroyed;

	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        dirtState = dirtSprite.Length-1;
	}
	
	void Update () {
	
	}

    public override bool Interact(Coords2 coords, PlayerMovement player)
    {
        spriteRenderer.sprite = dirtSprite[dirtState];
        if(dirtState > 0) { dirtState--; }
        else { isDestroyed = true; }
        return true;
    }

    public override bool HasCollider()
    {
        return !isDestroyed;
    }

}
