using UnityEngine;
using System.Collections;

public class ConnectedWallController : WallTileController
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override bool HasCollider()
    {
        return true;
    }

    public override void UpdateSprite(int x, int y)
    {
        SpriteSet sprites = gameObject.GetComponent<SpriteSet>();
        Map map = GameManager.instance.GetMap();

        int spriteId = 0;
        GameObject north = map.GetWallTileNorthOf(x, y);
        GameObject east = map.GetWallTileEastOf(x, y);
        GameObject south = map.GetWallTileSouthOf(x, y);
        GameObject west = map.GetWallTileWestOf(x, y);
        if (north != null)
        {
            spriteId += 1;
        }
        if (east != null)
        {
            spriteId += 2;
        }
        if (south != null)
        {
            spriteId += 4;
        }
        if (west != null)
        {
            spriteId += 8;
        }

        gameObject.GetComponent<SpriteRenderer>().sprite = sprites.Get(spriteId);
    }
}
