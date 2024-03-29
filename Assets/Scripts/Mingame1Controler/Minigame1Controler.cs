﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Minigame1Controler : MonoBehaviour {

    private TemperatureControl temperatureControl;
    private ScrewControler screwControler;
    public GameObject screwPrefab;
    public GameObject temperaturePrefab;
    // Use this for initialization
    void Start () {
        screwControler = (Instantiate(screwPrefab,new Vector3(-40,-40,0), Quaternion.identity) as GameObject).GetComponent<ScrewControler>();

        temperatureControl = (Instantiate(temperaturePrefab, new Vector3(-35, -40, 0), Quaternion.identity) as GameObject).GetComponent<TemperatureControl>();
        

	}

    // Update is called once per frame
    void Update() {
        //temperatureControl.setTemperature(screwControler.getPosition());
        temperatureControl.Resize(screwControler.getPosition());
        if (Input.GetKeyDown("r"))
        {
            SceneManager.UnloadScene(SceneManager.GetSceneByName("Minigame1").buildIndex);
        }
    }
        // temperatureControler.setTemperature(screwControler.getPosition);
    

}
