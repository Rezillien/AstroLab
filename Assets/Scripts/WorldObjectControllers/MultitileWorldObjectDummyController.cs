using UnityEngine;
using System.Collections;

public class MultitileWorldObjectDummyController : WorldObjectController
{
    private GameObject owner;
    
    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }

    public override bool Interact(Coords2 coords, GameObject player)
    {
        return owner.GetComponent<WorldObjectController>().Interact(coords, player);
    }

}
