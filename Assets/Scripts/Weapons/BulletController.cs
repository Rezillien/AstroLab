using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public virtual void SetInitialSpeed(float newInitialSpeed)
    {

    }
    public virtual void SetDirection(Vector2 newDirection)
    {

    }
    public virtual bool IsDead()
    {
        return true;
    }
    public virtual float GetDamage()
    {
        return 0.0f;
    }
    public virtual void SetDamage(float newDamage)
    {

    }
    public virtual void SetOwner(GameObject newOwner)
    {

    }
    public virtual void OnHitWall(WallTileController wall)
    {

    }
    public virtual void SetPosition(Vector2 newPosition)
    {

    }
    public virtual Vector2 GetPosition()
    {
        return Vector2.zero;
    }
    public virtual Rect GetCollider()
    {
        return new Rect();
    }
    public virtual Vector2 GetVelocity()
    {
        return Vector2.zero;
    }
}
