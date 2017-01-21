using UnityEngine;
using System.Collections;

public class ScrewControler : MonoBehaviour {

    private int initKeyPosition = 3;
  
    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {

        System.Random randomGenerator = new System.Random();
        int move=0;
        if (Input.GetKeyDown("up") || Input.GetKeyDown("down"))
        {
            if (Input.GetKeyDown("up"))
            {
                move = randomGenerator.Next(1, 3);
            }
            if (Input.GetKeyDown("down"))
            {
                move = -1 * (randomGenerator.Next(1, 3));
            }
            initKeyPosition += move;
            rotate(move);
            
        }
        

    }

    private void rotate(int rotation)
    {
        transform.Rotate(Vector3.forward * 30 * rotation);
    }

    public int getPosition()
    {
        return initKeyPosition;
    }
}
