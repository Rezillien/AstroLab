﻿using UnityEngine;
using System;
using UnityEngine.SceneManagement;  ////////////// change

public class Player : MonoBehaviour
{
    // position
    public int x;
    public int y;

    private int[] ammo;

    SmoothTransition moveAnimation;

    void Awake()
    {
        x = 1;
        y = 1;
        transform.position = new Vector3(x, y, 0);

        //no animation to start with
        moveAnimation = null;

        ammo = new int[AmmoPickup.NumberOfAmmoTypes];
        Util.Fill(ammo, 0);
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

        if (Input.GetKey("w")) dy = 1;
        if (Input.GetKey("a")) dx = -1;
        if (Input.GetKey("s")) dy = -1;
        if (Input.GetKey("d")) dx = 1;


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
        //TODO: interaction using mouse, this may be temporary
        Map map = GameManager.instance.GetMap();

        Vector3 worldPos3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPos = new Vector2(worldPos3.x, worldPos3.y);
        
        if (Input.GetMouseButtonDown(1)) //right click
        {
            return map.Interact(worldPos, this);
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

    public bool UseAmmoPickup(AmmoPickup ammoPickup)
    {
        int type = ammoPickup.GetAmmoType();
        ammo[type] += ammoPickup.GetAmmoCount(); //TODO: ammo count limitation
        ammoPickup.SetAmmoCount(0);

        return true;
    }
}
