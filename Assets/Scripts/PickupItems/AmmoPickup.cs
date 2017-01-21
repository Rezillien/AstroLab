using UnityEngine;
using System.Collections;
using System;

public class AmmoPickup : PickupItem {

    public static int NumberOfAmmoTypes = 10;

    public Vector2 colliderSize;

    private Vector2 position;
    private SpriteRenderer sprite;
    private int ammoLeft;
    private int ammoType;


    public static PickupItem CreateFromPrefab(GameObject prefab, Vector2 position, int count, int type)
    {
        GameObject instance = Instantiate(prefab);

        AmmoPickup item = instance.GetComponent<AmmoPickup>();
        item.SetPosition(position);
        item.SetAmmoCount(count);
        item.SetAmmoType(type);

        return item;
    }

    // Use this for initialization
    void Start() {
        sprite = gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update() {
	    if(isDead())
        {
            sprite.enabled = false;
        }
        else
        {
            sprite.transform.position = new Vector3(position.x, position.y);
        }
	}

    public void SetPosition(Vector2 newPosition)
    {
        position = newPosition;
    }
    public void SetColliderSize(Vector2 newColliderSize)
    {
        colliderSize = newColliderSize;
    }
    public void SetAmmoType(int newAmmoType)
    {
        ammoType = newAmmoType;
    }
    public void AddAmmo(int ammoCount)
    {
        ammoLeft += ammoCount;
    }
    public void SetAmmoCount(int newAmmoCount)
    {
        ammoLeft = newAmmoCount;
    }
    public int GetAmmoType()
    {
        return ammoType;
    }
    public int GetAmmoCount()
    {
        return ammoLeft;
    }

    public override bool Interact(PlayerMovement player)
    {
        if (isDead()) return false;

        return player.UseAmmoPickup(this);
    }

    public override bool isDead()
    {
        return ammoLeft == 0;
    }

    public override Rect Collider()
    {
        return Rect.MinMaxRect(
            position.x - colliderSize.x / 2.0f, position.y - colliderSize.y / 2.0f,
            position.x + colliderSize.x / 2.0f, position.y + colliderSize.y / 2.0f
            );
    }
}
