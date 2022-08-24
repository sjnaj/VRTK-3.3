using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
public class PhysicsProjectile : Projectile
{
    [SerializeField]
    private float lifeTime;

    private new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public override void Init(Weapon weapon)
    {
        base.Init(weapon);
        // Destroy (gameObject, lifeTime); //一定时间后子弹消失
    }

    public override void Launch()
    {
        base.Launch();
        rigidbody
            .AddRelativeForce(Vector3.forward * weapon.GetShootForce(),
            ForceMode.Impulse); //向前发射
                Debug.Log("Shooting");

    }

    private void OnTraggerEnter(Collider collider)
    {
        Destroy (gameObject);
        ITakeDamage[] damageTakers =
            collider.GetComponentsInChildren<ITakeDamage>();
        foreach (ITakeDamage damageTaker in damageTakers)
        {
            damageTaker.TakeDamage(weapon, this, transform.position);
        }
    }
}
