using UnityEngine;
using System;

using UnityEngine.SceneManagement;
using System.Collections.Generic;       //Allows us to use Lists. 

public class GameManager : MonoBehaviour
{
    public enum Turn
    {
        Player,
        Enemies
    };

    public SceneControler sceneController;

    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

    private PlayerMovement player = null; 
    private Turn turn;

    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        //Call the InitGame function to initialize the first level 
        InitGame();
    }

    //Initializes the game for each level.
    void InitGame()
    {
        //Call the SetupScene function of the BoardManager script, pass it current level number.
        GetMap().Setup(GetMapGenerator());
        //sceneController = new SceneControler();
        turn = Turn.Player; //player starts
    }

    public MapGenerator GetMapGenerator()
    {
        return GetComponent<MapGenerator>();
    }

    public Prefabs GetPrefabs()
    {
        return GetComponent<Prefabs>();
    }

    public Map GetMap()
    {
        return GetComponent<Map>();
    }

    public PlayerMovement GetPlayer()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        }

        return player;
    }

    public bool IsPlayerTurn()
    {
        return turn == Turn.Player;
    }

    public bool IsEnemyTurn()
    {
        return turn == Turn.Enemies;
    }

    public void EndEnemyTurn()
    {
        turn = Turn.Player;
    }

    public void EndPlayerTurn()
    {
        turn = Turn.Enemies;

        EndEnemyTurn(); //temporary
    }

    //Update is called every frame.
    void Update()
    {
        if (Input.GetKey("y"))
        {
            SceneManager.LoadScene("Minigame1", LoadSceneMode.Additive);
        }

        if (Input.GetKey("t"))
        {
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
    }
}
