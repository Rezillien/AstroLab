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
    public virtual bool Interact(Coords2 coords, GameObject player)
    {
        return false;
    }

    public virtual bool HasCollider()
    {
        return false;
    }
    public virtual float Opacity()
    {
        return 0.0f;
    }

    public virtual void UpdateSprite(Coords2 coords)
    {

    }

    public virtual Coords2[] GetDummiesToCreate()
    {
        return new Coords2[0];
    }
}
