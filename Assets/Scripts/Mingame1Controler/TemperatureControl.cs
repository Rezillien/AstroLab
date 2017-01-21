using UnityEngine;
using System.Collections;

public  class TemperatureControl : MonoBehaviour
{

    private int tempValue;
    private int tempGrowth;
   
	// Use this for initialization
	void Start ()
	{
	    tempGrowth = 0;
	    tempValue = 0;
        
    }
	
	// Update is called once per frame
	void Update () {

	 Resize(tempGrowth);
	 tempValue = tempGrowth + tempValue;
	}

    void Resize(int value)
    {
       
        Vector3 scale = new Vector3(1, tempValue+tempGrowth,1);
        transform.localScale = scale;
    }

    public void SetTempGrowth(int tempGrowth)
    {
        this.tempGrowth = tempGrowth;
    }
}
