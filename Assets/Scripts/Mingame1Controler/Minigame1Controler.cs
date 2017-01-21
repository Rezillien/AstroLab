using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Minigame1Controler : MonoBehaviour {

    private ScrewControler screwControler;
    public GameObject screwPrefab;
   
    void Start () {
        screwControler = Instantiate(screwPrefab).GetComponent<ScrewControler>();
       
        //temperatureControler = Instantiate(Prefabs).GetComponent<TemperatureControl>();
        //temperatureControler = new TemperatureControler();
	}
	
	// Update is called once per frame
	void Update () {
        // temperatureControler.setTemperature(screwControler.getPosition);
        if (Input.GetKeyDown("r"))
        {
            SceneManager.UnloadScene(SceneManager.GetSceneByName("Minigame1").buildIndex);
         
        }
    }

}
