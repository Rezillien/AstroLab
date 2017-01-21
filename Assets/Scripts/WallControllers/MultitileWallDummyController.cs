using UnityEngine;
using System.Collections;

public class MultitileWallDummyController : WallTileController {

    private WallTileController ownerController;

    public void SetOwner(GameObject newOwner)
    {
        ownerController = newOwner.GetComponent<WallTileController>();
    }
    
    public override bool Interact(Coords2 coords, PlayerMovement player)
    {
        return ownerController.Interact(coords, player);
    }

    public override bool HasCollider()
    {
        return ownerController.HasCollider();
    }
    public override float Opacity()
    {
        return ownerController.Opacity();
    }
}
