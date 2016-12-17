using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour {

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0f);
    }

    public float getX()
    {
        return this.transform.position.x;
    }

    public float getY()
    {
        return this.transform.position.y;
    }
}
