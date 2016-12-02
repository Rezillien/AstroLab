using UnityEngine;
using System.Collections;

public class WorldObjectController : MonoBehaviour
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
}
