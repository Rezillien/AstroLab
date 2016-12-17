﻿using UnityEngine;
using System;
using UnityEngine.SceneManagement;  ////////////// change

public class PlayerMovement : MonoBehaviour
{
    // position
    public int x;
    public int y;

    SmoothTransition moveAnimation;

    void Awake()
    {
        x = 1;
        y = 1;
        transform.position = new Vector3(x, y, 0);

        //no animation to start with
        moveAnimation = null;
    }

    public void SetPosition(int _x, int _y)
    {
        x = _x;
        y = _y;
        transform.position = new Vector3(x, y, 0);
    }

    //inform GameManager that player ended turn
    private void EndTurn()
    {
        GameManager.instance.EndPlayerTurn();
    }

    //returns true on success, starts animation
    private bool TryMove()
    {
        Map map = GameManager.instance.GetMap();

        int dx = 0;
        int dy = 0;

        if (Input.GetKey("up")) dy = 1;
        if (Input.GetKey("left")) dx = -1;
        if (Input.GetKey("down")) dy = -1;
        if (Input.GetKey("right")) dx = 1;


        //////changes
       // if(Input.GetKey("y")) SceneManager.LoadScene("Minigame1", LoadSceneMode.Single);
       // if(Input.GetKey("t")) SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        ///////////


        if ((dx != 0 || dy != 0) && !map.HasCollider(new Coords2(x + dx, y + dy)))
        {
            moveAnimation = new SmoothTransition(new Vector3(x, y, 0.0f), new Vector3(x + dx, y + dy, 0.0f), 0.25f);
            x += dx;
            y += dy;
            
      
            return true;
        }

        return false;
    }

    //returns true on success
    private bool TryInteract()
    {
        Map map = GameManager.instance.GetMap();

        int dx = 0;
        int dy = 0;

        if (Input.GetKeyDown("w")) dy = 1;
        if (Input.GetKeyDown("a")) dx = -1;
        if (Input.GetKeyDown("s")) dy = -1;
        if (Input.GetKeyDown("d")) dx = 1;

        if (dx != 0 || dy != 0)
        {
            if (map.Interact(new Coords2(x + dx, y + dy), gameObject))
            {
                return true;
            }
        }

        return false;
    }

    // Update is called once per frame
    // player logic
    void Update()
    {
        GameManager gm = GameManager.instance;
        if (!gm.IsPlayerTurn()) return;

        if (moveAnimation == null)
        {
            if (!TryMove())
            {
                if (TryInteract())
                {
                    //if interaction performed some action, end turn
                    EndTurn();
                }
            }
        }
        else
        {
            moveAnimation.Update();

            transform.position = moveAnimation.GetInterpolatedPosition();

            if (moveAnimation.IsFinished())
            {
                moveAnimation = null;
                //end turn only after animation has finished
                EndTurn();
            }
        }
    }
}
