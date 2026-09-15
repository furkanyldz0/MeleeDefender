using UnityEngine;

public class Bullet : MonoBehaviour {

    [SerializeField] private LayerMask bounceLayer;
    [SerializeField] private LayerMask hitLayer; // Hasar alabilenlerin (Düþman/Oyuncu) katmaný
    [SerializeField] private float bulletThickness = 0.1f;

    [SerializeField] private GameObject defaultVisual;
    [SerializeField] private GameObject reflectedVisual;

    public float ProjectileSpeed { get; set; } = 3f;
    public float LifeTime { get; set; } = 20f;
    public float ProjectileDamage { get; set; } = 34f;
    public Vector3 Direction { get; set; }
    public bool IsParried { get; set; }

    private void Start()
    {
        Destroy(gameObject, LifeTime);
        Direction = transform.forward;

        ProjectileSpeed = LevelManager.Instance.CurrentDifficultyTier.bulletSpeed; //bunu weapon'dan vs. ayarlayabilirim ileride
    }

    private void Update() {
        // 1. Bu karede ne kadar ileri gideceðimizi hesapla (Akýcýlýk için Time.deltaTime)
        float moveDistance = ProjectileSpeed * Time.deltaTime;

        // 2. Merminin gideceði yöne doðru kalýn bir ýþýn (Küre - SphereCast) yolla.
        // bounceLayer (Duvarlar) ve hitLayer (Düþmanlar) maskelerini ayný anda kontrol ediyoruz.
        if (Physics.SphereCast(transform.position, bulletThickness, Direction, out RaycastHit hit, moveDistance, bounceLayer | hitLayer)) {
            // Çarptýðýmýz obje Duvar/Sekme katmanýnda mý?
            if ((bounceLayer.value & (1 << hit.collider.gameObject.layer)) > 0) {
                // Mermiyi sektir
                Direction = Vector3.Reflect(Direction, hit.normal).normalized;
                transform.position = hit.point; // Duvara hizala
            }
            // Çarptýðýmýz obje sekme katmaný deðilse ve hasar alabiliyorsa
            else if (hit.collider.TryGetComponent<IDamagable>(out IDamagable damagable)) {
                damagable.Damage(ProjectileDamage);
                Destroy(gameObject); // Hasar verdik, mermiyi sil
            }
        }
        else {
            // 3. Önümüzde hiçbir engel yoksa mermiyi manuel olarak ilerlet ve döndür
            transform.position += Direction * moveDistance;

            if (Direction != Vector3.zero) {
                transform.rotation = Quaternion.LookRotation(Direction);
            }
        }
    }

    public void BeParried(Vector3 newDirection, float newProjectileSpeed, float damageMultiplier) {
        Direction = newDirection;
        ProjectileSpeed = newProjectileSpeed;
        ProjectileDamage *= damageMultiplier;
        IsParried = true;

        defaultVisual.SetActive(false);
        reflectedVisual.SetActive(true);
    }

}
