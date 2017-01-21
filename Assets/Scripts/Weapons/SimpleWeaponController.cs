using UnityEngine;
using System.Collections;
using System;

public class SimpleWeaponController : WeaponController {

    public GameObject bulletPrefab;
    public float initialBulletSpeed;
    public float baseWeaponDamage;
    public float baseCooldown; //in seconds
    public int ammoType;

    private float cooldownLeft;

    // Use this for initialization
    void Start () {
        cooldownLeft = baseCooldown;
	}
	
	// Update is called once per frame
	void Update () {

    }

    void FixedUpdate()
    {
        cooldownLeft -= Time.fixedDeltaTime;
    }

    public override bool TryShoot(Player player, Vector2 direction)
    {
        if (cooldownLeft > 0.0f) return false;
        if (player.GetAmmoCount(ammoType) == 0) return false;

        Map map = GameManager.instance.GetMap();
        GameObject bullet = Instantiate(bulletPrefab);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.SetPosition(new Vector2(player.x, player.y));
        bulletController.SetInitialSpeed(initialBulletSpeed);
        bulletController.SetDirection(direction);
        bulletController.SetDamage(baseWeaponDamage);
        bulletController.SetOwner(player.gameObject);

        map.AddBullet(bulletController);
        cooldownLeft = baseCooldown;
        player.RemoveAmmo(ammoType, 1);
        return true;
    }
}
