using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour {

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public virtual bool TryShoot(Player player, Vector2 direction)
    {
        return false;
    }
}
