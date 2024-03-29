﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class PickupSystem {

    private List<PickupItem> pickups;
	// Use this for initialization
	public PickupSystem() {
        pickups = new List<PickupItem>();
	}
	
	// Update is called once per frame
	public void Update () {
	    
	}

    public void FixedUpdate()
    {
        DeleteDeadPickups();
    }

    public void AddPickup(PickupItem newPickup)
    {
        pickups.Add(newPickup);
    }

    public bool OnInteractionAttempt(Vector2 position, Player player)
    {
        int itemIndex = QueryPickupItem(position);
        if (itemIndex < 0) return false;

        return pickups[itemIndex].Interact(player);
    }

    private int QueryPickupItem(Vector2 position)
    {
        for(int i = 0; i < pickups.Count; ++i)
        {
            PickupItem item = pickups[i];

            if (item.Collider().Contains(position))
            {
                return i;
            }
        }

        return -1;
    }

    private void DeleteDeadPickups()
    {
        foreach (PickupItem pickup in pickups)
        {
            if (pickup.IsDead())
            {
                UnityEngine.Object.Destroy(pickup.gameObject);
            }
        }
        pickups.RemoveAll(item => item.IsDead());
    }
}
