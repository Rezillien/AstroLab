using UnityEngine;
using System.Collections;

public class MultitileWorldObjectDummyController : WorldObjectController
{
    private WorldObjectController ownerController;
    
    public void SetOwner(GameObject newOwner)
    {
        ownerController = newOwner.GetComponent<WorldObjectController>();
    }

    public override bool Interact(Coords2 coords, Player player)
    {
        return ownerController.Interact(coords, player);
    }

}
