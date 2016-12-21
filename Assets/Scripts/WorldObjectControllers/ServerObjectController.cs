using System;
using UnityEngine;
using System.Collections;

public class ServerObjectController : ReparableObjectController {

    private Map map;
    private Prefabs prefabs;
    private SpriteRenderer sprite;

    void Start()
    {
        map = GameManager.instance.GetMap();
        prefabs = GameManager.instance.GetPrefabs();
        sprite = GetComponent<SpriteRenderer>();

        //cheaty way of getting position
        Coords2 coords = new Coords2(Mathf.RoundToInt(sprite.transform.position.x), Mathf.RoundToInt(sprite.transform.position.y));

        //create dummys on map.
        //TODO: way to inform map about size of the object
        CreateDummy(new Coords2(coords.x, coords.y + 1));
        CreateDummy(new Coords2(coords.x + 1, coords.y));
        CreateDummy(new Coords2(coords.x + 1, coords.y + 1));
    }

    public override void PlayMiniGame()
    {
        isGameOn = true;
        //Just for purposes if it's working 
        throw new NullReferenceException("ale bieda ;(");
        //Tutaj bd wywolywac gierke
    }

    private void CreateDummy(Coords2 coords)
    {
        GameObject dummy = map.CreateWorldObject(coords, prefabs.multitileWorldObjectDummy);
        dummy.GetComponent<MultitileWorldObjectDummyController>().SetOwner(gameObject);
    }
}
