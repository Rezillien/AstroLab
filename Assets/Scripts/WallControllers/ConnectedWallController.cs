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
        GameObject north = map.GetWallTile(x, y + 1);
        GameObject east = map.GetWallTile(x + 1, y);
        GameObject south = map.GetWallTile(x, y - 1);
        GameObject west = map.GetWallTile(x - 1, y);
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
