using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Base : MonoBehaviour, IDamagable, IHasHealthBar {

    public event EventHandler<IHasHealthBar.OnHealthChangedEventArgs> OnHealthChanged;
    public event Action OnDied;

    [SerializeField] private AudioClip[] hitSoundClips;
    [SerializeField] private AudioClip destroySoundClip;

    public float Health { get; set; } = 400f; //400f
    private float currentHealth;
    private float regenerationRate = 2f;

    private DamageFlashAlt damageFlash;

    private void Start() {
        damageFlash = GetComponent<DamageFlashAlt>();

        currentHealth = Health;

        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = currentHealth / Health
        }); //baþta can barýný gizlesin
    }

    private void FixedUpdate() {
        if(currentHealth < Health) {
            currentHealth += regenerationRate * Time.fixedDeltaTime;

            OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
                currentHealthNormalized = currentHealth / Health
            });
        }
    }

    public void Damage(float damageAmount) {
        currentHealth -= damageAmount;

        OnHealthChanged.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs { 
            currentHealthNormalized = currentHealth / Health
        });
        //Debug.Log(currentHealth / Health);

        if(currentHealth <= 0f) {
            StartCoroutine(DelayDie(0.15f));
        }

        damageFlash.CallDamageFlash();
        SoundFXManager.Instance.PlayRandomSoundFXClip(hitSoundClips, transform, 0.8f, true);
    }

    private IEnumerator DelayDie(float duration) {
        yield return new WaitForSeconds(duration);
        Die();
    }

    private void Die() {
        //Destroy(gameObject);
        OnDied?.Invoke();
        gameObject.SetActive(false);

        SoundFXManager.Instance.PlaySoundFXClip(destroySoundClip, transform, 1f);
    }
}
