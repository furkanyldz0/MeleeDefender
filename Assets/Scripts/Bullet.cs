using UnityEngine;

public class Bullet : MonoBehaviour {

    [SerializeField] private LayerMask bounceLayer;
    [SerializeField] private LayerMask hitLayer; // Hasar alabilenlerin (Düşman/Oyuncu) katmanı
    [SerializeField] private float bulletThickness = 0.1f;

    [SerializeField] private GameObject defaultVisual;
    [SerializeField] private GameObject reflectedVisual;

    public float ProjectileSpeed { get; set; }
    public float LifeTime { get; set; } = 20f;
    public float ProjectileDamage { get; set; } = 34f;
    public Vector3 Direction { get; set; }
    public bool IsParried { get; set; }
    public bool IsCrit { get; private set; }
    public int PierceRemaining { get; set; }
    // Bossun ağır mermileri savuşturulunca ekstra hasar verir
    public float ParryDamageBonus { get; set; } = 1f;

    private void Start()
    {
        Destroy(gameObject, LifeTime);

        // Kopyalanan/özel kurulan mermilerin yönünü ezme
        if (Direction == Vector3.zero) {
            Direction = transform.forward;
        }
    }

    private void Update() {
        // 1. Bu karede ne kadar ileri gideceğimizi hesapla (Akıcılık için Time.deltaTime)
        float moveDistance = ProjectileSpeed * Time.deltaTime;

        // 2. Merminin gideceği yöne doğru kalın bir ışın (Küre - SphereCast) yolla.
        // bounceLayer (Duvarlar) ve hitLayer (Düşmanlar) maskelerini aynı anda kontrol ediyoruz.
        if (Physics.SphereCast(transform.position, bulletThickness, Direction, out RaycastHit hit, moveDistance, bounceLayer | hitLayer)) {
            // Çarptığımız obje Duvar/Sekme katmanında mı?
            if ((bounceLayer.value & (1 << hit.collider.gameObject.layer)) > 0) {
                // Mermiyi sektir
                Direction = Vector3.Reflect(Direction, hit.normal).normalized;
                transform.position = hit.point; // Duvara hizala
                GameEvents.BulletBounced(hit.point);
            }
            // Çarptığımız obje sekme katmanı değilse ve hasar alabiliyorsa
            else if (hit.collider.TryGetComponent<IDamagable>(out IDamagable damagable)) {
                // Dost ateşi yok: düşman mermisi düşmanlara, yansıyan mermi kaleye zarar vermez
                bool isBase = damagable is Base;
                if (isBase == IsParried) {
                    Move(moveDistance);
                    return;
                }

                damagable.Damage(ProjectileDamage);

                // Kalkana çarpan mermi hasar sayısı göstermez (kalkan kendi "BLOK" geri bildirimini verir)
                if (IsParried && !(damagable is ShieldHitbox)) {
                    GameEvents.DamageDealt(hit.point, ProjectileDamage, IsCrit);
                }

                if (IsParried && PierceRemaining > 0) {
                    // Delici mermi hedefin içinden geçip yoluna devam eder
                    PierceRemaining--;
                    Move(moveDistance);
                }
                else {
                    Destroy(gameObject); // Hasar verdik, mermiyi sil
                }
            }
        }
        else {
            // 3. Önümüzde hiçbir engel yoksa mermiyi manuel olarak ilerlet ve döndür
            Move(moveDistance);
        }
    }

    private void Move(float distance) {
        transform.position += Direction * distance;

        if (Direction != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(Direction);
        }
    }

    public void BeParried(Vector3 newDirection, float newProjectileSpeed, float damageMultiplier, bool isCrit = false, int pierce = 0) {
        Direction = newDirection.normalized;
        ProjectileSpeed = newProjectileSpeed;
        ProjectileDamage *= damageMultiplier * ParryDamageBonus;
        IsParried = true;
        IsCrit = isCrit;
        PierceRemaining = pierce;

        transform.rotation = Quaternion.LookRotation(Direction);

        defaultVisual.SetActive(false);
        reflectedVisual.SetActive(true);

        if (isCrit) {
            reflectedVisual.transform.localScale *= 1.4f;
        }
    }

    // Çatallanma için: henüz savuşturulmamış hâlinin bir kopyasını oluşturur
    public Bullet CreateUnparriedCopy() {
        Bullet copy = Instantiate(this, transform.position, transform.rotation);
        copy.ProjectileSpeed = ProjectileSpeed;
        copy.ProjectileDamage = ProjectileDamage;
        copy.ParryDamageBonus = ParryDamageBonus;
        copy.LifeTime = LifeTime;
        copy.Direction = Direction;
        return copy;
    }

    public void Setup(float speedMultiplier) {
        ProjectileSpeed = speedMultiplier * LevelManager.Instance.CurrentDifficultyTier.bulletSpeed;
    }

    // Bossların özel mermileri için
    public void SetupCustom(Vector3 direction, float speed, float damage, float scale = 1f, float parryDamageBonus = 1f) {
        Direction = direction.normalized;
        ProjectileSpeed = speed;
        ProjectileDamage = damage;
        ParryDamageBonus = parryDamageBonus;

        if (!Mathf.Approximately(scale, 1f)) {
            transform.localScale *= scale;
            bulletThickness *= scale;
        }
    }

}
