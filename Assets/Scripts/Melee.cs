using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Melee : MonoBehaviour {

    public event EventHandler OnParried;
    public event Action OnSwing;

    [Header("Savuşturma Ayarları (kılıcın bıçağına göre)")]
    [Tooltip("Bıçağın görsel boyuna eklenen tolerans")]
    [SerializeField] private float bladeReachMargin = 0.25f;
    [Tooltip("Bıçağın iki yanındaki savuşturma genişliği")]
    [SerializeField] private float bladeHalfWidth = 0.3f;
    [Tooltip("Savuruş bittikten sonra kılıcın hâlâ savuşturduğu kısa süre")]
    [SerializeField] private float parryGraceTime = 0.06f;
    [SerializeField] private LayerMask bulletLayer;
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private Transform trailRendererParent;

    [SerializeField] private ProjectileWaveSkill projectileWaveSkill;

    private const float BaseReflectSpeedMultiplier = 5f;
    private const float SplitAngle = 14f;        // ek mermilerin ana mermiye göre açısı
    private const float SplitDamageRatio = 0.6f; // ek mermiler daha az hasar verir
    private const float HandRadius = 0.35f;              // kabza/el bölgesine gelen mermi her zaman savuşturulur
    private const float MaxParryHeightDifference = 1.5f;

    private int comboStep = 1;

    private bool isHitboxActive = false;
    private List<Collider> alreadyHitBullets = new List<Collider>();
    private float attackDuration = 0.1f;

    private List<TrailRenderer> trails = new List<TrailRenderer>();
    private Vector3[] baseTrailPositions;

    // Silaha ve perklere göre hesaplanan bıçak ölçüleri
    private float bladeLength = WeaponVisual.BaseBladeLength;
    private float bladeWidth;

    // Savuruş takibi: bıçağın iki kare arasında taradığı yay
    private float swingDirection = -1f;
    private float previousBladeYaw;
    private bool hasPreviousBladeYaw;

    private WeaponVisual weaponVisual;

    private Coroutine skillAttackCouroutine;

    private void Start() {
        Player.Instance.OnAttack += Player_OnAttack;

        trails = GetComponentsInChildren<TrailRenderer>().ToList();
        baseTrailPositions = trails.Select(t => t.transform.localPosition).ToArray();

        DisableTrails();

        weaponVisual = gameObject.AddComponent<WeaponVisual>();
        PlayerStats.Instance.OnStatsChanged += ApplyWeaponStats;
        ApplyWeaponStats();
    }

    private void ApplyWeaponStats() {
        PlayerStats stats = PlayerStats.Instance;
        float lengthScale = stats.Weapon.hitboxScale.y * stats.ReachMultiplier;

        attackDuration = stats.SwingDuration;
        bladeLength = WeaponVisual.BaseBladeLength * lengthScale;
        bladeWidth = bladeHalfWidth * stats.Weapon.hitboxScale.z * stats.ReachMultiplier;

        // İz efektleri bıçak boyunca yerleşir
        for (int i = 0; i < trails.Count; i++) {
            Vector3 p = baseTrailPositions[i];
            trails[i].transform.localPosition = new Vector3(p.x, p.y * lengthScale, p.z);
        }

        // "Geniş Savuruş" perki kılıcı görsel olarak da uzatır; savuşturma alanı her zaman görünen bıçakla aynı
        weaponVisual.Build(stats.Weapon, trails, stats.ReachMultiplier);
    }

    private void Update() {
        if (isHitboxActive && !projectileWaveSkill.IsActive) {
            CheckHits();
        }
    }

    private void Player_OnAttack(object sender, System.EventArgs e) {
        if (projectileWaveSkill != null && projectileWaveSkill.IsActive) {
            if (skillAttackCouroutine == null) {
                skillAttackCouroutine = StartCoroutine(SkillAttackCoroutine(projectileWaveSkill.AttackTime));
            }
        }
        else {
            DefaultAttack();
        } 
    }

    private void DefaultAttack() {
        transform.DOKill();
        OnSwing?.Invoke();

        // Kombo: bir sağdan sola, bir soldan sağa
        bool isFirstStep = comboStep == 1;
        float fromAngle = isFirstStep ? 0f : -180f;
        float toAngle = isFirstStep ? -180f : 0f;
        comboStep = isFirstStep ? 2 : 1;

        DOVirtual.Float(fromAngle, toAngle, attackDuration, y => {
            transform.localEulerAngles = new Vector3(0f, y, -90f);
        })
        .SetEase(Ease.OutQuad)
        .SetTarget(transform)
        .OnStart(() => {
            EnableHitbox(Mathf.Sign(toAngle - fromAngle));
            EnableTrails();
        })
        .OnComplete(() => {
            // Son karedeki hareketi de say, ardından kılıç kısa bir süre daha savuştursun
            CheckHits();
            DisableTrails();
            DOVirtual.DelayedCall(parryGraceTime, DisableHitbox).SetTarget(transform);
        });
    }

    private IEnumerator SkillAttackCoroutine(float attackTime) {
        transform.DOKill();
        OnSwing?.Invoke();
        if (comboStep == 1) {
            DOVirtual.Float(0f, -180f, attackTime, y => {
                transform.localEulerAngles = new Vector3(0f, y, -90f);
            })
            .SetEase(Ease.OutQuad)
            .SetTarget(transform)
            .OnStart(() => {
                EnableTrails();

            })
            .OnComplete(() => {
                DisableTrails();
            });

            comboStep = 2;
        }
        else {
            DOVirtual.Float(-180f, 0f, attackTime, y => {
                transform.localEulerAngles = new Vector3(0f, y, -90f);
            })
            .SetEase(Ease.OutQuad)
            .SetTarget(transform).
            OnStart(() => {
                EnableTrails();

            })
            .OnComplete(() => {
                DisableTrails();
            });

            comboStep = 1;
        }

        yield return new WaitForSeconds(attackTime);

        skillAttackCouroutine = null;
    }

    private void ParryBullet(Bullet bullet) { //düşman parrylerse değişiriz, şuan sadece player
        PlayerStats stats = PlayerStats.Instance;

        bool isCrit = UnityEngine.Random.value < stats.CritChance;
        float speedDamageMultiplier = bullet.ProjectileSpeed * 0.35f;
        float finalDamageMultiplier = speedDamageMultiplier * stats.DamageMultiplier * (isCrit ? stats.CritDamageMultiplier : 1f);

        float parriedBulletSpeed = bullet.ProjectileSpeed * BaseReflectSpeedMultiplier * stats.ReflectSpeedMultiplier;
        Vector3 forward = Player.Instance.transform.forward;

        // Çatallanma: ek mermiler ana merminin sağına ve soluna sırayla açılır
        int extraProjectiles = stats.ExtraProjectiles;
        for (int i = 0; i < extraProjectiles; i++) {
            float side = (i % 2 == 0) ? 1f : -1f;
            float angle = SplitAngle * (i / 2 + 1) * side;
            Vector3 splitDirection = Quaternion.Euler(0f, angle, 0f) * forward;

            Bullet copy = bullet.CreateUnparriedCopy();
            copy.BeParried(splitDirection, parriedBulletSpeed, finalDamageMultiplier * SplitDamageRatio, isCrit, stats.Pierce);
        }

        bullet.BeParried(forward, parriedBulletSpeed, finalDamageMultiplier, isCrit, stats.Pierce);
        Player.Instance.AddSkillPoint(1);
        OnParried?.Invoke(this, EventArgs.Empty);
        GameEvents.Parry(bullet.transform.position);
    }

    private void CheckHits() {
        Vector3 pivot = transform.position;
        float bladeYaw = GetBladeYaw();
        float fromYaw = hasPreviousBladeYaw ? previousBladeYaw : bladeYaw;
        previousBladeYaw = bladeYaw;
        hasPreviousBladeYaw = true;

        // Bıçağın son kareden bu yana savuruş yönünde taradığı açı
        float sweep = Mathf.Repeat((bladeYaw - fromYaw) * swingDirection, 360f);
        if (sweep > 200f) sweep = 0f; // oyuncu ters yöne dönerken oluşan küçük geri kayma

        float reach = bladeLength + bladeReachMargin;
        Collider[] hits = Physics.OverlapSphere(pivot, reach + 0.5f, bulletLayer);

        foreach (Collider hit in hits) {
            // Aynı mermiyi tek savuruşta 2 kere algılamamak için kontrol et
            if (alreadyHitBullets.Contains(hit)) continue;
            if (!hit.TryGetComponent<Bullet>(out Bullet bullet) || bullet.IsParried) continue;
            if (!IsTouchedByBlade(pivot, hit.transform.position, fromYaw, sweep, reach)) continue;

            alreadyHitBullets.Add(hit);
            ParryBullet(bullet);
            PlayImpactEffect(bullet.transform.position);
            HitStop.Instance.StopTime(0.02f);
        }
    }

    // Mermi, bıçağın bu karede taradığı yay diliminin içinde mi? (kameranın gördüğü gibi üstten, XZ düzleminde)
    private bool IsTouchedByBlade(Vector3 pivot, Vector3 point, float fromYaw, float sweep, float reach) {
        Vector3 offset = point - pivot;
        if (Mathf.Abs(offset.y) > MaxParryHeightDifference) return false;

        offset.y = 0f;
        float distance = offset.magnitude;
        if (distance > reach) return false;
        if (distance < HandRadius) return true;

        float yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        float margin = Mathf.Atan2(bladeWidth, distance) * Mathf.Rad2Deg;

        // Savuruşun başladığı açıya göre, savuruş yönünde ölçülen açı
        float relative = Mathf.Repeat((yaw - fromYaw) * swingDirection + margin, 360f) - margin;
        return relative <= sweep + margin;
    }

    // Bıçak Melee'nin yerel +Y ekseni boyunca uzanır
    private float GetBladeYaw() {
        Vector3 blade = transform.up;
        return Mathf.Atan2(blade.x, blade.z) * Mathf.Rad2Deg;
    }

    private void PlayImpactEffect(Vector3 position) {
        var effect = Instantiate(impactEffect, position, Quaternion.identity);

        Destroy(effect.gameObject, 1f);
    }
    
    private void EnableHitbox(float direction) {
        alreadyHitBullets.Clear();
        swingDirection = direction;
        hasPreviousBladeYaw = false;
        isHitboxActive = true;
    }

    private void DisableHitbox() {
        isHitboxActive = false;
    }

    private void EnableTrails() {
        foreach(TrailRenderer t in trails) {
            t.emitting = true;
        }
    }
    private void DisableTrails() {
        foreach (TrailRenderer t in trails) {
            t.emitting = false;
        }
    }

    // Scene ekranında savuşturma alanını görmek için: bıçak çizgisi ve iki yanındaki tolerans
    private void OnDrawGizmos() {
        Vector3 blade = transform.up;
        blade.y = 0f;
        if (blade.sqrMagnitude < 0.0001f) return;
        blade.Normalize();

        float reach = bladeLength + bladeReachMargin;
        float width = Application.isPlaying ? bladeWidth : bladeHalfWidth;
        Vector3 side = Vector3.Cross(Vector3.up, blade) * width;
        Vector3 start = transform.position;
        Vector3 end = start + blade * reach;

        Gizmos.color = isHitboxActive ? Color.cyan : new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawLine(start + side, end + side);
        Gizmos.DrawLine(start - side, end - side);
        Gizmos.DrawWireSphere(start, HandRadius);
    }

    private void OnDestroy() {
        if (Player.Instance != null)
            Player.Instance.OnAttack -= Player_OnAttack;

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged -= ApplyWeaponStats;

        transform.DOKill();
    }
}