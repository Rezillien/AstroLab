using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneControler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {


        if (Input.GetKey("y"))
        {
            SceneManager.LoadScene("Minigame1", LoadSceneMode.Single);
        }

        if (Input.GetKey("t"))
        {
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
    }
}
