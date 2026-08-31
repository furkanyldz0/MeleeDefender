using UnityEngine;

public interface IDamagable
{
    public float Health { get; set; }

    public void Damage(float damageAmount);
}
