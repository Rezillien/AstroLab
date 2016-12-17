using UnityEngine;
using System.Collections;

public class Shooter : MonoBehaviour {

    public Transform cursorPosition;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        RotateVeapon();
    }

    private void RotateVeapon()
    {
        float z = AngleBetweenVector2(this.transform.position, cursorPosition.position);
        if (z >= -45 && z < 45) { z = 0; }
        else if (z >= 45 && z < 135) { z = 90; }
        else if (z >= 135 && z < 180 || z < -135 && z > -180) { z = 180; }
        else if (z >= -135 && z < -45) { z = 270; }
        this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, z);
    }

    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }
}
