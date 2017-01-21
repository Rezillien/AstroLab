using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class BulletSystem {

    Map map;
    private List<BulletController> bullets;

    // Use this for initialization
    public BulletSystem() {
        bullets = new List<BulletController>();
        map = GameManager.instance.GetMap();
	}

    public void FixedUpdate()
    {
        UpdateBullets();
        DeleteDeadBullets();
    }

    public void AddBullet(BulletController newBullet)
    {
        bullets.Add(newBullet);
    }

    private void DeleteDeadBullets()
    {
        foreach(BulletController bullet in bullets)
        {
            if (bullet.IsDead())
            {
                UnityEngine.Object.Destroy(bullet.gameObject);
            }
        }
        bullets.RemoveAll(bullet => bullet.IsDead());
    }

    private void UpdateBullets()
    {
        //TODO: better checking for collisions. possibly using rectangle sweep
        
        foreach(BulletController bullet in bullets)
        {
            UpdateBulletsPhysics(bullet);
        }
    }

    private void UpdateBulletsPhysics(BulletController bullet)
    {
        Vector2 position = bullet.GetPosition();
        Vector2 velocity = bullet.GetVelocity();
        Vector2 newPosition = position + velocity * Time.fixedDeltaTime;
        bullet.SetPosition(newPosition);
        Rect collider = bullet.GetCollider();

        int minX = Mathf.FloorToInt(collider.xMin + 0.5f);
        int minY = Mathf.FloorToInt(collider.yMin + 0.5f);
        int maxX = Mathf.FloorToInt(collider.xMax + 0.5f);
        int maxY = Mathf.FloorToInt(collider.yMax + 0.5f);

        for (int x = minX; x <= maxX; ++x)
        {
            for(int y = minY; y <= maxY; ++y)
            {
                Coords2 coords = new Coords2(x, y);
                GameObject wall = map.GetWallTile(coords);
                if (wall == null) continue;
                WallTileController controller = wall.GetComponent<WallTileController>();
                if(controller.HasCollider())
                {
                    bullet.OnHitWall(controller);
                    return;
                }
            }
        }
    }
}
