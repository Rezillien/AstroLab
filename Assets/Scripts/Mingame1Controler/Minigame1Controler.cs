using UnityEngine;
using System.Collections;

public class Minigame1Controler : MonoBehaviour {

    //private TemperatureControler temperatureControler;
    private ScrewControler screwControler;
    public GameObject screwPrefab;
    // Use this for initialization
    void Start () {
        screwControler = Instantiate(screwPrefab).GetComponent<ScrewControler>();
        //temperatureControler = Instantiate(Prefabs).GetComponent<TemperatureControl>();
        //temperatureControler = new TemperatureControler();
	}
	
	// Update is called once per frame
	void Update () {
       // temperatureControler.setTemperature(screwControler.getPosition);
	}

}
