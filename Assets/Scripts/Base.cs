using System;
using System.Collections;
using UnityEngine;

public class Base : MonoBehaviour, IDamagable, IHasHealthBar {

    public static Base Instance { get; private set; }

    public event EventHandler<IHasHealthBar.OnHealthChangedEventArgs> OnHealthChanged;
    public event Action OnDied;
    public event Action<float, float> OnHealthValuesChanged; // mevcut, azami

    private const float BaseMaxHealth = 400f;

    public float Health { get; set; } = BaseMaxHealth; // azami can
    public float CurrentHealth => currentHealth;
    public bool IsDead { get; private set; }

    private float currentHealth;

    private DamageFlash damageFlash;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla Base var!");
        }
        Instance = this;
    }

    private void Start() {
        damageFlash = GetComponent<DamageFlash>();

        // Kalıcı geliştirmeler azami canı artırabilir
        Health = BaseMaxHealth * PlayerStats.Instance.BaseHealthMultiplier;
        currentHealth = Health;

        NotifyHealthChanged(); //başta can barını gizlesin
    }

    public void Damage(float damageAmount) {
        if (IsDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - damageAmount);
        NotifyHealthChanged();
        GameEvents.BaseDamaged(transform.position, damageAmount);

        if(currentHealth <= 0f) {
            IsDead = true;
            StartCoroutine(DelayDie(0.15f));
        }

        damageFlash.CallDamageFlash();
    }

    public void HealPercent(float percent) {
        if (IsDead) return;

        currentHealth = Mathf.Min(Health, currentHealth + Health * percent);
        NotifyHealthChanged();
    }

    // "Sağlam Surlar" perki gibi azami canı değiştiren etkilerden sonra çağrılır
    public void RefreshMaxHealth(bool healAddedAmount) {
        float oldMax = Health;
        Health = BaseMaxHealth * PlayerStats.Instance.BaseHealthMultiplier;

        if (healAddedAmount && Health > oldMax) {
            currentHealth += Health - oldMax;
        }
        currentHealth = Mathf.Min(currentHealth, Health);

        NotifyHealthChanged();
    }

    private void NotifyHealthChanged() {
        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = currentHealth / Health
        });
        OnHealthValuesChanged?.Invoke(currentHealth, Health);
    }

    private IEnumerator DelayDie(float duration) {
        yield return new WaitForSeconds(duration);
        Die();
    }

    private void Die() {
        //Destroy(gameObject);
        OnDied?.Invoke();
        gameObject.SetActive(false);
    }
}
