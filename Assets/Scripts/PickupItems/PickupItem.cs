using UnityEngine;
using System.Collections;

public class PickupItem : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //returns true if interaction succeded
    public virtual bool Interact(PlayerMovement player)
    {
        return false;
    }
    public virtual bool isDead()
    {
        return true;
    }
    public virtual Rect Collider()
    {
        return new Rect();
    }
}
