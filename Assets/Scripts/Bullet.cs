using UnityEngine;

public class Bullet : MonoBehaviour {
    public float ProjectileSpeed { get; set; } = 3f;
    public float LifeTime { get; set; } = 30f;
    public float ProjectileDamage { get; set; } = 34f;
    public Vector3 Direction { get; set; }

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
        rb.MovePosition(rb.position + Direction * ProjectileSpeed * Time.fixedDeltaTime);
    }

}
