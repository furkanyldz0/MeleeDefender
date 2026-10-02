using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : MonoBehaviour, IDamagable, IHasHealthBar
{
    public static event Action<Enemy> OnAnyEnemyDied;
    public event EventHandler<IHasHealthBar.OnHealthChangedEventArgs> OnHealthChanged;

    [SerializeField] private Weapon weapon;

    public float Health { get; set; } = 100f;
    public bool IsSpawning { get; set; } = false;
    public float FirstAttackTime { get; set; } = 1f;

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
        if (IsSpawning)
            return;

        if(attackTimeDelta > 0) {
            attackTimeDelta -= Time.deltaTime;
        }
        else if(attackTimeDelta <= 0) {
            weapon.Shoot();
            attackTimeDelta = attackTime;
        }
    }

    public void Setup(bool isSpawning, float firstAttackTime) {
        IsSpawning = isSpawning;
        FirstAttackTime = firstAttackTime;
        attackTimeDelta = FirstAttackTime;
    }

    public void Damage(float DamageAmount) {
        Health -= DamageAmount;
        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = Health / maxHealth
        });

        if (Health <= 10) {
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
        OnAnyEnemyDied?.Invoke(this);
    }

}
