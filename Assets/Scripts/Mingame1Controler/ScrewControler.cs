using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScrewControler : MonoBehaviour {

    private int initKeyPosition = 3;
    private bool broken = false;

    private TemperatureControl temperatureControl;


    // Use this for initialization
    void Start () {
      
	}
	
	// Update is called once per frame
	void Update () {

        System.Random randomGenerator = new System.Random();
        int move=0;
        if ((Input.GetKeyDown("1") || Input.GetKeyDown("2")) && !broken)
        {
            int rnd = randomGenerator.Next(1, 7);
            if (initKeyPosition + rnd > 30)
                broken = true;
            if (Input.GetKeyDown("1"))
            {
                move = rnd;
            }
            if (Input.GetKeyDown("2"))
            {
                move = -1 * rnd;
            }
            initKeyPosition += move;
            if (initKeyPosition < 0)
            {
                move -= initKeyPosition;
                initKeyPosition = 0;
            }
            rotate(move);
           

        }
        

    }

    private void rotate(int rotation)
    {
        transform.Rotate(Vector3.forward * 10 * rotation);
    }

    public int getPosition()
    {
        return initKeyPosition;
    }

    public void getTemperatureControl(TemperatureControl temperatureControl)
    {
        this.temperatureControl = temperatureControl;
    }
}
