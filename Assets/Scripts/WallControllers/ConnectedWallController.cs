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
    public override float Opacity()
    {
        return 1.0f;
    }

    public override void UpdateSprite(Coords2 coords)
    {
        SpriteSet sprites = gameObject.GetComponent<SpriteSet>();
        Map map = GameManager.instance.GetMap();

        int spriteId = 0;
        GameObject north = map.GetWallTileNorthOf(coords);
        GameObject east = map.GetWallTileEastOf(coords);
        GameObject south = map.GetWallTileSouthOf(coords);
        GameObject west = map.GetWallTileWestOf(coords);
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
