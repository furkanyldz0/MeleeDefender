using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

// Boss: canı yarıya inince öfkelenir ve saldırı desenleri sertleşir.
// Mermiler normal düşman mermisiyle aynı prefabı kullanır; yani hepsi savuşturulabilir.
public class Boss : MonoBehaviour, IDamagable {

    public static event Action<Boss> OnAnyBossDefeated;

    public event Action<float> OnHealthChanged; // normalize can
    public event Action OnEnraged;
    public event Action OnTelegraph;
    public event Action OnDeathStarted;

    private const float BulletHeight = 0.55f;
    private const float EnrageThreshold = 0.5f;
    private const float EntranceDuration = 2.4f;

    public BossDefinition Definition { get; private set; }
    public float Health { get; set; }
    public float MaxHealth { get; private set; }
    public float RewardMultiplier { get; private set; } = 1f;
    public bool IsDead { get; private set; }
    public bool IsEnraged { get; private set; }
    public bool IsActive { get; private set; }

    private BossVisual visual;
    private DamageFlash damageFlash;
    private BoxCollider boxCollider;
    private EnemySpawner spawner;
    private Bullet bulletPrefab;

    private Vector3 homePosition;
    private float swayAmount;
    private float swaySpeed;
    private float swayTime;
    private int cannonIndex;
    private Coroutine attackRoutine;

    private float BulletDamage => GameDatabase.Instance.balance.bossBulletDamage;

    private float BulletSpeed {
        get {
            float tierSpeed = LevelManager.Instance.CurrentDifficultyTier.bulletSpeed;
            return Mathf.Clamp(tierSpeed, 6f, 10f);
        }
    }

    public static Boss Create(BossDefinition definition, float healthMultiplier, float rewardMultiplier, EnemySpawner spawner, Vector3 spawnPosition, Vector3 homePosition) {
        GameObject go = new GameObject("Boss_" + definition.id);
        go.layer = LayerMask.NameToLayer("Damageable");
        go.transform.position = spawnPosition;

        Boss boss = go.AddComponent<Boss>();
        boss.Init(definition, healthMultiplier, rewardMultiplier, spawner, homePosition);
        return boss;
    }

    private void Init(BossDefinition definition, float healthMultiplier, float rewardMultiplier, EnemySpawner enemySpawner, Vector3 home) {
        Definition = definition;
        MaxHealth = definition.baseHealth * healthMultiplier;
        Health = MaxHealth;
        RewardMultiplier = rewardMultiplier;
        spawner = enemySpawner;
        homePosition = home;
        bulletPrefab = spawner.NormalEnemyType.prefab.Weapon.BulletPrefab;

        visual = gameObject.AddComponent<BossVisual>();
        visual.Build(definition);

        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = visual.ColliderCenter;
        boxCollider.size = visual.ColliderSize;

        // Görseller kurulduktan sonra eklenir ki tüm parçaları bulsun
        damageFlash = gameObject.AddComponent<DamageFlash>();

        switch (definition.kind) {
            case BossKind.VortexSpire: swayAmount = 2.4f; swaySpeed = 0.8f; break;
            case BossKind.EmberLord: swayAmount = 1.2f; swaySpeed = 0.55f; break;
            default: swayAmount = 1.6f; swaySpeed = 0.5f; break;
        }

        transform.DOMove(homePosition, EntranceDuration)
            .SetEase(Ease.OutCubic)
            .SetLink(gameObject)
            .OnComplete(() => {
                IsActive = true;
                attackRoutine = StartCoroutine(AttackLoop());
            });
    }

    private void Update() {
        if (!IsActive || IsDead) return;

        // Yavaşça sağa sola salınır
        swayTime += Time.deltaTime * swaySpeed * (IsEnraged ? 1.5f : 1f);
        transform.position = homePosition + Vector3.right * Mathf.Sin(swayTime) * swayAmount;
    }

    #region Hasar

    public void Damage(float damageAmount) {
        if (IsDead || !IsActive) return;

        Health -= damageAmount;
        damageFlash.CallDamageFlash();
        OnHealthChanged?.Invoke(Mathf.Clamp01(Health / MaxHealth));

        if (!IsEnraged && Health <= MaxHealth * EnrageThreshold) {
            IsEnraged = true;
            visual.SetEnraged();
            OnEnraged?.Invoke();
        }

        if (Health <= 0f) {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence() {
        IsDead = true;
        IsActive = false;
        boxCollider.enabled = false;
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        OnDeathStarted?.Invoke();

        if (HitStop.Instance != null) {
            HitStop.Instance.SlowMotion(0.3f, 1.2f);
        }

        Vector3 center = transform.position + visual.ColliderCenter;
        for (int i = 0; i < 6; i++) {
            Vector3 point = center + Vector3.Scale(UnityEngine.Random.insideUnitSphere, visual.ColliderSize * 0.5f);
            FeedbackFX.Instance.Debris(point, i % 2 == 0 ? Definition.primaryColor : Definition.accentColor, 8, 2f, 0.22f);
            FeedbackFX.Instance.Shake(0.3f, 0.2f);
            yield return new WaitForSecondsRealtime(0.16f);
        }

        FeedbackFX.Instance.Debris(center, Definition.primaryColor, 30, 3.5f, 0.32f);
        FeedbackFX.Instance.Debris(center, Definition.accentColor, 20, 3f, 0.24f);

        OnAnyBossDefeated?.Invoke(this);
        Destroy(gameObject);
    }

    #endregion

    #region Saldırı desenleri

    private IEnumerator AttackLoop() {
        yield return new WaitForSeconds(0.6f);

        switch (Definition.kind) {
            case BossKind.VortexSpire: yield return VortexSpireRoutine(); break;
            case BossKind.EmberLord: yield return EmberLordRoutine(); break;
            default: yield return IronWardenRoutine(); break;
        }
    }

    // Demir Muhafız: yelpaze atışları + oyuncuya nişanlı top atışları
    private IEnumerator IronWardenRoutine() {
        while (!IsDead) {
            yield return Telegraph(0.45f);
            if (IsEnraged) {
                FireFan(4, 36f, BulletSpeed, BulletDamage);
            }
            else {
                FireFan(3, 26f, BulletSpeed, BulletDamage);
            }
            yield return new WaitForSeconds(IsEnraged ? 1.1f : 1.3f);

            yield return Telegraph(0.3f);
            int aimedShots = IsEnraged ? 3 : 2;
            for (int i = 0; i < aimedShots; i++) {
                FireAimed(NextCannon(), BulletSpeed * 1.15f, BulletDamage);
                yield return new WaitForSeconds(0.22f);
            }
            yield return new WaitForSeconds(IsEnraged ? 0.9f : 1f);
        }
    }

    // Girdap Kulesi: sağdan sola süpüren mermi dizileri, öfkeliyken hızlanır
    private IEnumerator VortexSpireRoutine() {
        float sweepDirection = 1f;
        int sweepCount = 0;

        while (!IsDead) {
            yield return Telegraph(0.45f);

            int shots = IsEnraged ? 8 : 6;
            float maxAngle = IsEnraged ? 34f : 28f;
            float interval = IsEnraged ? 0.1f : 0.14f;
            // Öfkeliyken her üç taramadan biri iki yönlü olur
            bool mirrored = IsEnraged && sweepCount % 3 == 2;

            for (int i = 0; i < shots; i++) {
                float angle = Mathf.Lerp(-maxAngle, maxAngle, i / (float)(shots - 1)) * sweepDirection;
                FireDirection(CannonPosition(0), AngleToDirection(angle), BulletSpeed * 0.9f, BulletDamage * 0.8f);
                if (mirrored) {
                    FireDirection(CannonPosition(0), AngleToDirection(-angle), BulletSpeed * 0.9f, BulletDamage * 0.8f);
                }
                yield return new WaitForSeconds(interval);
            }

            sweepDirection *= -1f;
            sweepCount++;
            yield return new WaitForSeconds(IsEnraged ? 1.2f : 1.7f);
        }
    }

    // Kor Lordu: yavaş ve iri "ağır küreler" (savuşturulursa çok sert vurur), yardımcı çağırma, öfkeliyken mermi yağmuru
    private IEnumerator EmberLordRoutine() {
        float nextSummonTime = Time.time + 4f;

        while (!IsDead) {
            yield return Telegraph(0.6f);
            int orbCount = IsEnraged ? 2 : 1;
            for (int i = 0; i < orbCount; i++) {
                FireHeavyOrb();
                yield return new WaitForSeconds(0.4f);
            }
            yield return new WaitForSeconds(IsEnraged ? 0.8f : 1.2f);

            if (IsEnraged) {
                yield return Telegraph(0.35f);
                for (int i = 0; i < 6; i++) {
                    float x = UnityEngine.Random.Range(-3.8f, 3.8f);
                    Vector3 origin = new Vector3(x, BulletHeight, transform.position.z + visual.FrontZ);
                    FireDirection(origin, Vector3.back, BulletSpeed * 1.2f, BulletDamage * 0.9f);
                    yield return new WaitForSeconds(0.15f);
                }
            }
            else {
                yield return Telegraph(0.35f);
                FireFan(3, 24f, BulletSpeed, BulletDamage);
            }
            yield return new WaitForSeconds(IsEnraged ? 1.3f : 1.6f);

            if (Time.time >= nextSummonTime) {
                spawner.SpawnExtra(2, 3);
                nextSummonTime = Time.time + 14f;
            }
        }
    }

    private IEnumerator Telegraph(float duration) {
        visual.Telegraph(duration);
        OnTelegraph?.Invoke();
        yield return new WaitForSeconds(duration);
    }

    private void FireFan(int count, float spread, float speed, float damage) {
        Vector3 origin = CannonPosition(0);
        for (int i = 0; i < count; i++) {
            float angle = count == 1 ? 0f : Mathf.Lerp(-spread / 2f, spread / 2f, i / (float)(count - 1));
            FireDirection(origin, AngleToDirection(angle), speed, damage);
        }
    }

    private void FireAimed(Vector3 origin, float speed, float damage) {
        Vector3 target = Player.Instance != null ? Player.Instance.transform.position : Vector3.zero;
        Vector3 direction = target - origin;
        direction.y = 0f;

        // Her zaman kaleye doğru (aşağı) gitsin
        if (direction.z > -1f) direction.z = -1f;

        FireDirection(origin, direction, speed, damage);
    }

    private void FireHeavyOrb() {
        Vector3 origin = CannonPosition(0);
        Vector3 target = Player.Instance != null ? Player.Instance.transform.position : Vector3.zero;
        Vector3 direction = target - origin;
        direction.y = 0f;
        if (direction.z > -1f) direction.z = -1f;

        Bullet orb = SpawnBullet(origin, direction);
        orb.SetupCustom(direction, BulletSpeed * 0.55f, GameDatabase.Instance.balance.bossHeavyOrbDamage, 2.4f, 3f);
    }

    private void FireDirection(Vector3 origin, Vector3 direction, float speed, float damage) {
        Bullet bullet = SpawnBullet(origin, direction);
        bullet.SetupCustom(direction, speed, damage);
    }

    private Bullet SpawnBullet(Vector3 origin, Vector3 direction) {
        return Instantiate(bulletPrefab, origin, Quaternion.LookRotation(direction.normalized));
    }

    private Vector3 CannonPosition(int index) {
        Vector3[] offsets = visual.CannonOffsets;
        Vector3 offset = offsets.Length > 0 ? offsets[index % offsets.Length] : new Vector3(0f, 0f, visual.FrontZ);
        Vector3 position = transform.position + offset;
        position.y = BulletHeight;
        return position;
    }

    private Vector3 NextCannon() {
        cannonIndex++;
        return CannonPosition(cannonIndex);
    }

    private static Vector3 AngleToDirection(float angle) {
        return Quaternion.Euler(0f, angle, 0f) * Vector3.back;
    }

    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        OnAnyBossDefeated = null;
    }
}
