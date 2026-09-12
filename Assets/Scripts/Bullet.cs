using UnityEngine;

public class Bullet : MonoBehaviour {

    [SerializeField] private LayerMask bounceLayer;
    [SerializeField] private GameObject defaultVisual;
    [SerializeField] private GameObject reflectedVisual;

    public float ProjectileSpeed { get; set; } = 3f;
    public float LifeTime { get; set; } = 20f;
    public float ProjectileDamage { get; set; } = 34f;
    public Vector3 Direction { get; set; }
    public bool IsParried { get; set; }

    private Rigidbody rb;

    private void Start()
    {
        Destroy(gameObject, LifeTime);

        Direction = transform.forward;

        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider col) {
        if(col.TryGetComponent<IDamagable>(out IDamagable damagable)) {
            damagable.Damage(ProjectileDamage);
            Destroy(gameObject);
        }
    }

    private void FixedUpdate() {
        float moveDistance = ProjectileSpeed * Time.fixedDeltaTime;

        if(Physics.Raycast(rb.position, Direction, out RaycastHit raycastHit, moveDistance, bounceLayer)) {
            Direction = Vector3.Reflect(Direction, raycastHit.normal).normalized;
            rb.position = raycastHit.point;
        }
        else {
            if (Direction != Vector3.zero) {
                Quaternion targetRotation = Quaternion.LookRotation(Direction);

                rb.MoveRotation(targetRotation);
            }

            rb.MovePosition(rb.position + Direction * moveDistance);
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
