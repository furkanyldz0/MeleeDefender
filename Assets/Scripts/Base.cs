using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Base : MonoBehaviour, IDamagable, IHasHealthBar {

    public event EventHandler<IHasHealthBar.OnHealthChangedEventArgs> OnHealthChanged;
    public event Action OnDied; 

    public float Health { get; set; } = 400f; //400f
    private float currentHealth;

    private DamageFlash damageFlash;

    private void Start() {
        damageFlash = GetComponent<DamageFlash>();

        currentHealth = Health;

        OnHealthChanged?.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs {
            currentHealthNormalized = currentHealth / Health
        }); //baþta can barýný gizlesin
    }

    public void Damage(float damageAmount) {
        currentHealth -= damageAmount;

        OnHealthChanged.Invoke(this, new IHasHealthBar.OnHealthChangedEventArgs { 
            currentHealthNormalized = currentHealth / Health
        });
        Debug.Log(currentHealth / Health);

        if(currentHealth <= 0f) {
            StartCoroutine(DelayDie(0.15f));
        }

        damageFlash.CallDamageFlash();
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
