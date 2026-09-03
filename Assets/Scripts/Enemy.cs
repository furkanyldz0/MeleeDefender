using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    public event Action<Enemy> OnDied;

    [SerializeField] private Weapon weapon;

    public float Health { get; set; } = 100f;
    public bool IsSpawning { get; set; } = false;

    private float attackTime = 2.5f;
    private float attackTimeDelta;

    private DamageFlash damageFlash;

    private void Start()
    {
        damageFlash = GetComponent<DamageFlash>();

        attackTimeDelta = attackTime;
    }

    
    private void Update()
    {
        if(attackTimeDelta > 0 && !IsSpawning) {
            attackTimeDelta -= Time.deltaTime;
        }
        else if(attackTimeDelta <= 0) {
            weapon.Shoot();
            attackTimeDelta = attackTime;
        }
    }

    public void Damage(float DamageAmount) {
        Health -= DamageAmount;

        if (Health <= 0) {
            StartCoroutine(DelayDie(0.15f));
        }

        damageFlash.CallDamageFlash();
    }

    private IEnumerator DelayDie(float duration) {
        yield return new WaitForSeconds(duration);
        Die();
    }

    private void Die() {
        Destroy(gameObject);
        OnDied?.Invoke(this);
    }
}
