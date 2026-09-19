using System;
using System.Collections;
using UnityEngine;

public class ProjectileWaveSkill : MonoBehaviour {

    public event Action<float> OnSkillProgressChanged;

    [SerializeField] private ProjectileWave projectileWavePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    private float waveSpeed = 30f;
    private float waveDamage = 100f;
    private float waveLifeTime = 5;

    private float duration = 15f;

    public bool IsActive { get; private set; }
    public float AttackTime { get; private set; } = 0.2f;

    private Coroutine windowCoroutine;
    private Coroutine attackCoroutine;

    private void Start() {
        Player.Instance.OnAttack += Player_OnAttack;
        Player.Instance.OnSkillUsed += Player_OnSkillUsed;
    }

    private void Player_OnSkillUsed(object sender, EventArgs e) {
        if (windowCoroutine != null) StopCoroutine(windowCoroutine);
        windowCoroutine = StartCoroutine(ActivateSkill(duration));
    }

    private IEnumerator ActivateSkill(float duration) {
        IsActive = true;

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float remainingRatio = 1f - (elapsed / duration);
            OnSkillProgressChanged?.Invoke(remainingRatio);
            yield return null;
        }

        IsActive = false;
        OnSkillProgressChanged?.Invoke(0f);
        windowCoroutine = null;
    }

    private void Player_OnAttack(object sender, EventArgs e) {
        if (!IsActive) return;

        if(attackCoroutine == null) {
            attackCoroutine = StartCoroutine(AttackCoroutine(AttackTime));
        }
        //SendProjectileWave();
    }

    private IEnumerator AttackCoroutine(float waitDuration) {
        SendProjectileWave();

        yield return new WaitForSeconds(waitDuration);

        attackCoroutine = null;
    }

    private void SendProjectileWave() {
        var wave = Instantiate(projectileWavePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(projectileSpawnPoint.forward));
        wave.Setup(projectileSpawnPoint.forward, waveSpeed, waveDamage, waveLifeTime);
    }

    private void OnDestroy() {
        if (Player.Instance != null) {
            Player.Instance.OnAttack -= Player_OnAttack;
            Player.Instance.OnSkillUsed -= Player_OnSkillUsed;
        }

        if (windowCoroutine != null) StopCoroutine(windowCoroutine);
    }
}