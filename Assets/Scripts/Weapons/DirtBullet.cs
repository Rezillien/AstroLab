using UnityEngine;
using System.Collections;

public class DirtBullet : BulletController {

    public float colliderWidth;
    public float colliderHeight;
    public GameObject dirtWallPrefab;

    private Vector2 position;
    private Vector2 velocity;
    private float damage;
    private float initialSpeed;
    private GameObject owner;
    private bool isDead;

    // Use this for initialization
    void Start () {
        isDead = false;
	}
	
	// Update is called once per frame
	void Update ()
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
        GameManager.instance.GetMap().CreateWallTile(new Coords2((int)(transform.position.x + 0.5f), (int)(transform.position.y + 0.5f)), dirtWallPrefab);
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
