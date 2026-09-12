using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable, IHasHealthBar
{
    public event Action<Enemy> OnDied;
    public event EventHandler<IHasHealthBar.OnHealthChangedEventArgs> OnHealthChanged;

    [SerializeField] private Weapon weapon;

    public float Health { get; set; } = 100f;
    public bool IsSpawning { get; set; } = false;

    private float maxHealth;
    private float attackTime = 2.5f;
    private float attackTimeDelta;

    private DamageFlash damageFlash;

    private void Start()
    {
        damageFlash = GetComponent<DamageFlash>();

        maxHealth = Health;
        attackTimeDelta = attackTime;

        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = Health / maxHealth
        });
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
        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = Health / maxHealth
        });

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
