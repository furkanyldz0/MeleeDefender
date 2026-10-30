using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable, IHasHealthBar
{
    public static event EventHandler<OnAnyEnemyDiedEventArgs> OnAnyEnemyDied;
    public event EventHandler<IHasHealthBar.OnHealthChangedEventArgs> OnHealthChanged;
    public class OnAnyEnemyDiedEventArgs : EventArgs {
        public int scoreToKill;
    }

    [SerializeField] private Staff staff;
    [SerializeField] private float bulletSpeedMultiplier = 1f;
    [SerializeField] private int defaultAttackCount = 1;
    [SerializeField] private int scoreToKill = 1;
    [SerializeField] private Animator animator;

    [SerializeField] private AudioClip[] hitSoundClips;
    [SerializeField] private AudioClip[] deathSoundClips;

    public EnemyTypeSO TypeData { get; set; }
    public float Health { get; set; } = 100f;
    public bool IsSpawning { get; set; } = false;
    public float FirstAttackTime { get; set; } = 1f;

    private float maxHealth;
    private float attackTime = 2.5f;
    private float attackTimeDelta;
    private bool isAttacking = false;
    private bool isDamageable;

    private DamageFlash damageFlash;

    private void Start() {
        damageFlash = GetComponent<DamageFlash>();

        maxHealth = Health;
        attackTimeDelta = attackTime;

        staff.OnAttackFinished += Weapon_OnAttackFinished;

        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = Health / maxHealth
        });
    }

    private void Weapon_OnAttackFinished(object sender, EventArgs e) {
        isAttacking = false;
        attackTimeDelta = attackTime;
    }

    private void Update() {
        if (IsSpawning || isAttacking)
            return;

        attackTimeDelta -= Time.deltaTime;

        if (attackTimeDelta <= 0f) {
            isAttacking = true;
            staff.PrepareToShoot(bulletSpeedMultiplier, transform.forward, defaultAttackCount);
        }
    }

    public void Setup(bool isSpawning, bool isDamageable, float firstAttackTime) {
        IsSpawning = isSpawning;
        this.isDamageable = isDamageable;
        FirstAttackTime = firstAttackTime;
        attackTimeDelta = FirstAttackTime;

        animator.SetBool("IsSpawning", IsSpawning);
    }

    public void Damage(float DamageAmount) {
        if (!isDamageable) {
            return;
        }

        Health -= DamageAmount;
        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = Health / maxHealth
        });

        if (Health <= 10) {
            StartCoroutine(DelayDie(0.15f));
            SoundFXManager.Instance.PlayRandomSoundFXClip(deathSoundClips, transform, 0.6f, true);
        }
        else {
            SoundFXManager.Instance.PlayRandomSoundFXClip(hitSoundClips, transform, 0.6f, true);
        }

        damageFlash.CallDamageFlash();
    }

    private IEnumerator DelayDie(float duration) {
        yield return new WaitForSeconds(duration);
        Die();
    }

    private void Die() {
        Destroy(gameObject);
        OnAnyEnemyDied?.Invoke(this, new OnAnyEnemyDiedEventArgs {
            scoreToKill = scoreToKill
        });
    }

    private void OnDestroy() {
        if (staff != null) {
            staff.OnAttackFinished -= Weapon_OnAttackFinished;
        }
    }
}