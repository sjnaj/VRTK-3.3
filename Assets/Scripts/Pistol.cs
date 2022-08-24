using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pistol : Weapon
{
    [SerializeField]
    private Projectile bulletPrefab;

    // protected override void StartShooting()
    // {
    //     base.StartShooting();
    // }
    protected override void Shoot()
    {
        Projectile projectile =
            Instantiate(bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);
        projectile.Init(this);
        projectile.Launch();
        base.Shoot();
    }

    protected override void StopShooting()
    {
        base.StopShooting();
    }
}
