using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable, IHasHealthBar
{
    public static event EventHandler<OnAnyEnemyDiedEventArgs> OnAnyEnemyDied;
    public event EventHandler<IHasHealthBar.OnHealthChangedEventArgs> OnHealthChanged;
    public class OnAnyEnemyDiedEventArgs : EventArgs {
        public int scoreToKill;
        public Vector3 position;
    }

    [SerializeField] private Weapon weapon;
    [SerializeField] private float bulletSpeedMultiplier = 1f;
    [SerializeField] private int defaultAttackCount = 1;
    [SerializeField] private float attackInterval = 0f;
    [SerializeField] private int scoreToKill = 1;

    public EnemyTypeSO TypeData { get; set; }
    public float Health { get; set; } = 100f;
    public bool IsSpawning { get; set; } = false;
    public float FirstAttackTime { get; set; } = 1f;
    public bool IsDead { get; private set; }
    public int ScoreToKill => scoreToKill;
    public int BonusScore { get; set; } // ör. kalkanlı düşmanlar ek puan verir
    public Weapon Weapon => weapon;

    // Saldırıdan hemen önce görselin "şarj" animasyonu oynatabilmesi için
    public event Action OnAboutToShoot;

    private const float TelegraphTime = 0.35f;

    private float maxHealth;
    private float attackTime = 2.5f;
    private float attackTimeDelta;
    private float attackIntervalDelta;
    private int attackCount;
    private bool hasTelegraphed;

    private DamageFlash damageFlash;

    private void Start()
    {
        damageFlash = GetComponent<DamageFlash>();

        maxHealth = Health;
        attackTimeDelta = attackTime;
        attackIntervalDelta = attackInterval;
        attackCount = defaultAttackCount;

        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = Health / maxHealth
        });
    }

    private void Update()
    {
        if (IsSpawning || IsDead)
            return;

        if(attackTimeDelta > 0) {
            attackTimeDelta -= Time.deltaTime;

            if (!hasTelegraphed && attackTimeDelta <= TelegraphTime) {
                hasTelegraphed = true;
                OnAboutToShoot?.Invoke();
            }
        }
        else if(attackTimeDelta <= 0) {

            if (attackCount > 0) {
                if (attackIntervalDelta > 0) {
                    attackIntervalDelta -= Time.deltaTime;
                }
                else if (attackIntervalDelta <= 0) {
                    weapon.Shoot(bulletSpeedMultiplier);
                    attackIntervalDelta = attackInterval;
                    attackCount--;
                }
            }
            else if (attackCount == 0) {
                attackIntervalDelta = attackInterval;
                attackTimeDelta = attackTime;
                attackCount = defaultAttackCount;
                hasTelegraphed = false;
            }

        }
    }

    public void Setup(bool isSpawning, float firstAttackTime) {
        IsSpawning = isSpawning;
        FirstAttackTime = firstAttackTime;
        attackTimeDelta = FirstAttackTime;
        hasTelegraphed = false;
    }

    public void Damage(float DamageAmount) {
        if (IsDead) return;

        Health -= DamageAmount;
        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = Mathf.Clamp01(Health / maxHealth)
        });

        if (Health <= 10) {
            IsDead = true;
            StartCoroutine(DelayDie(0.15f));
        }

        damageFlash.CallDamageFlash();
    }

    // Boss ölünce sahnedeki yardımcıları temizlemek için
    public void Kill() {
        if (IsDead) return;

        IsDead = true;
        Die();
    }

    private IEnumerator DelayDie(float duration) {
        yield return new WaitForSeconds(duration);
        Die();
    }

    private void Die() {
        Destroy(gameObject);
        OnAnyEnemyDied?.Invoke(this, new OnAnyEnemyDiedEventArgs {
            scoreToKill = scoreToKill + BonusScore,
            position = transform.position
        });
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        OnAnyEnemyDied = null;
    }

}
