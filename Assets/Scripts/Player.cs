using UnityEngine;
using System;
using UnityEngine.SceneManagement;  ////////////// change

public class Player : MonoBehaviour
{
    private static int NumberOfWeapons = 10;

    // position
    public int x;
    public int y;

    private int[] ammo;
    private WeaponController[] weapons;
    private int currentWeapon;

    SmoothTransition moveAnimation;

    void Awake()
    {
        transform.position = new Vector3(x, y, 0);

        //no animation to start with
        moveAnimation = null;

        ammo = new int[AmmoPickup.NumberOfAmmoTypes];
        Util.Fill(ammo, 0);

        weapons = new WeaponController[NumberOfWeapons];
        currentWeapon = 1;
    }

    private void Start()
    {
        //temporarily create only one needed for testing
        weapons[1] = Instantiate(GameManager.instance.GetPrefabs().weaponPrefabs[0]).GetComponent<WeaponController>();
        weapons[2] = Instantiate(GameManager.instance.GetPrefabs().weaponPrefabs[1]).GetComponent<WeaponController>();
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
        if (Input.GetMouseButtonDown(1)) //right click
        {
            Map map = GameManager.instance.GetMap();

            Vector3 worldPos3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 worldPos = new Vector2(worldPos3.x, worldPos3.y);
            return map.Interact(worldPos, this);
        }

        return false;
    }
    private bool TryShoot()
    {
        if (weapons[currentWeapon] == null) return false;

        if (Input.GetMouseButtonDown(0)) //left click
        {
            Vector3 worldPos3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 worldPos = new Vector2(worldPos3.x, worldPos3.y);
            Vector2 direction = (worldPos - new Vector2(x, y)).normalized;
            return weapons[currentWeapon].TryShoot(this, direction);
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
            if (TryMove())
            {
                EndTurn();
            }
            else if(TryInteract())
            {
                EndTurn();
            }
            else if(TryShoot())
            {
                EndTurn();
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

        if (Input.GetKeyDown(KeyCode.Alpha0)) currentWeapon = 0;
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentWeapon = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentWeapon = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentWeapon = 3;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentWeapon = 4;
        if (Input.GetKeyDown(KeyCode.Alpha5)) currentWeapon = 5;
        if (Input.GetKeyDown(KeyCode.Alpha6)) currentWeapon = 6;
        if (Input.GetKeyDown(KeyCode.Alpha7)) currentWeapon = 7;
        if (Input.GetKeyDown(KeyCode.Alpha8)) currentWeapon = 8;
        if (Input.GetKeyDown(KeyCode.Alpha9)) currentWeapon = 9;
    }

    public bool UseAmmoPickup(AmmoPickup ammoPickup)
    {
        int type = ammoPickup.GetAmmoType();
        ammo[type] += ammoPickup.GetAmmoCount(); //TODO: ammo count limitation
        ammoPickup.SetAmmoCount(0);

        return true;
    }

    public int GetAmmoCount(int ammoType)
    {
        return ammo[ammoType];
    }
    public void RemoveAmmo(int ammoType, int count)
    {
        ammo[ammoType] -= count;
    }
}
