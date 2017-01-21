using UnityEngine;
using System.Collections;

public class Rocket : BulletController
{

    public float colliderWidth;
    public float colliderHeight;
    public GameObject explosionAnimation;

    private Vector2 position;
    private Vector2 velocity;
    private float damage;
    private float initialSpeed;
    private GameObject owner;
    private bool isDead;

    // Use this for initialization
    void Start()
    {
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(position.x, position.y);
    }

    public override void SetInitialSpeed(float newInitialSpeed)
    {
        initialSpeed = newInitialSpeed;
    }
    public override void SetDirection(Vector2 newDirection)
    {
        velocity = newDirection * initialSpeed;
    }
    public override bool IsDead()
    {
        return isDead;
    }
    public override float GetDamage()
    {
        return damage;
    }
    public override void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    public override void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }
    public override void OnHitWall(WallTileController wall)
    {
        Map map = GameManager.instance.GetMap();
        Coords2 centerOfExplosion = new Coords2((int)(transform.position.x + 0.5f), (int)(transform.position.y + 0.5f));
        for(int i=-1; i<=1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                map.RemoveWallTile(new Coords2(i, j)+centerOfExplosion);
            }
        }
        GameObject explosion = Instantiate(explosionAnimation, new Vector3(centerOfExplosion.x, centerOfExplosion.y), Quaternion.identity) as GameObject;
        explosion.transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
        Destroy(explosion, 2.0f);
        isDead = true;
    }
    public override Vector2 GetPosition()
    {
        return position;
    }
    public override void SetPosition(Vector2 newPosition)
    {
        position = newPosition;
    }
    public override Rect GetCollider()
    {
        return Rect.MinMaxRect(
            position.x - colliderWidth / 2.0f,
            position.y - colliderHeight / 2.0f,
            position.x + colliderWidth / 2.0f,
            position.y + colliderHeight / 2.0f
            );
    }
    public override Vector2 GetVelocity()
    {
        return velocity;
    }
}
