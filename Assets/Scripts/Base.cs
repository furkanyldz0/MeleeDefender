using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Base : MonoBehaviour, IDamagable {
    public float Health { get; set; } = 300f;

    private DamageFlash damageFlash;
    private void Start() {
        damageFlash = GetComponent<DamageFlash>();
    }

    public void Damage(float damageAmount) {
        Health -= damageAmount;
        if(Health <= 0f) {
            StartCoroutine(DelayDie(0.15f));
        }

        damageFlash.CallDamageFlash();
    }

    private IEnumerator DelayDie(float duration) {
        yield return new WaitForSeconds(duration);
        Die();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //ileride levelmanager ile hallederiz þimdilik kalsýn
    }

    private void Die() {
        Destroy(gameObject);
    }
}
