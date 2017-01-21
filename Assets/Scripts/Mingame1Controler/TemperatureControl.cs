using UnityEngine;
using System.Collections;

public  class TemperatureControl : MonoBehaviour
{

    private int tempValue;
    private int tempGrowth;
    private int temp;
    private GameObject temperatureGameObject;
   
	// Use this for initialization
	void Start ()
	{
	    tempGrowth = 0;
	    tempValue = 0;
        
    }
	
	// Update is called once per frame
	void Update () {

	 //Resize(temp);
	 tempValue = tempGrowth + tempValue;
	}

    public void Resize(int temp)
    {
       
        Vector3 scale = new Vector3(1, temp*0.3f,1);
        transform.localScale = scale;
    }

    public void setTemperature(int temp)
    {
        this.tempValue = temp;
        if (temp > 20)
            changeColor(0, 0, 0);
        
            
    }

    private void changeColor(int red, int green, int blue)
    { 
        GameObject gameObject = this.gameObject;
        gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        
    }

    public void SetTempGrowth(int tempGrowth)
    {
        this.tempGrowth = tempGrowth;
    }

}
