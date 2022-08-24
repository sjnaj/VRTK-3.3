using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    protected Weapon weapon;

    public virtual void Init(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public virtual void Launch()
    {
    }
}
