using UnityEngine;
using System;


using System.Collections.Generic;       //Allows us to use Lists. 

public class GameManager : MonoBehaviour
{
    public enum Turn
    {
        Player,
        Enemies
    };

    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    private Map map;                       //Store a reference to our BoardManager which will set up the level.                                 //Current level number, expressed in game as "Day 1".
    private MapGenerator mapGenerator;

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

        //Get a component reference to the attached BoardManager script
        map = GetComponent<Map>();
        mapGenerator = GetComponent<TmpMapGenerator>();

        //Call the InitGame function to initialize the first level 
        InitGame();
    }

    //Initializes the game for each level.
    void InitGame()
    {
        //Call the SetupScene function of the BoardManager script, pass it current level number.
        map.Setup(mapGenerator);
        turn = Turn.Player; //player starts
    }

    public Map GetMap()
    {
        return map;
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
    }
}
