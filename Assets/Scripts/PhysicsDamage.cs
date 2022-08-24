using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
public class PhysicsDamage : MonoBehaviour, ITakeDamage
{
    // Start is called before the first frame update
    private new Rigidbody rigidbody;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void TakeDamage(
        Weapon weapon,
        Projectile projectile,
        Vector3 contactPoint
    )
    {
        rigidbody.AddForce(projectile.transform.forward, ForceMode.Impulse);
    }

   

    
}
