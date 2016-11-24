using UnityEngine;
using System.Collections;

//base class for all wall controllers
public class WallTileController : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //should return true if any action is performed
    public virtual bool Interact(GameObject player)
    {
        return false;
    }

    public virtual bool HasCollider()
    {
        return false;
    }

    public virtual void UpdateSprite(Coords2 coords)
    {

    }
}
