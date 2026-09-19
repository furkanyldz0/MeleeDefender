using System.Collections.Generic;
using UnityEngine;

public class ProjectileWave : MonoBehaviour {
    [Header("Çarpýþma Ayarlarý")]
    [SerializeField] private LayerMask enemyLayer;    // Hasar verilecek düþmanlar
    [SerializeField] private LayerMask bulletLayer; // Temizlenecek mermiler
    [SerializeField] private LayerMask wallLayer;   // Dalganýn çarpýp yok olacaðý duvarlar
    [SerializeField] private Vector3 hitboxSize = new Vector3(3f, 1f, 0.5f);

    [SerializeField] private ParticleSystem impactEffect;

    private float projectileSpeed;
    private float projectileDamage;
    private float lifeTime;
    private Vector3 direction;

    // Ayný düþmana dalga geçip gidene kadar tekrar tekrar hasar vermemek için hafýza listesi
    private HashSet<Collider> alreadyHitColliders = new HashSet<Collider>();

    public void Setup(Vector3 moveDirection, float speed, float damage, float timeToLive) {
        direction = moveDirection.normalized;
        projectileSpeed = speed;
        projectileDamage = damage;
        lifeTime = timeToLive;

        Destroy(gameObject, lifeTime);
    }

    private void Update() {
        float moveDistance = projectileSpeed * Time.deltaTime;
        LayerMask combinedLayerMask = enemyLayer | bulletLayer | wallLayer;

        Vector3 subBoxSize = new Vector3(hitboxSize.x / 3f, hitboxSize.y, hitboxSize.z);

        float curveDepth = -0.5f;

        Vector3[] localOffsets = new Vector3[] {
            new Vector3(-hitboxSize.x / 3f, 0, curveDepth), // Sol kutu biraz geride
            Vector3.zero,                                   // Orta kutu en önde
            new Vector3(hitboxSize.x / 3f, 0, curveDepth)   // Sað kutu biraz geride
        };

        for (int i = 0; i < 3; i++) {
            Vector3 segmentWorldCenter = transform.position + transform.TransformDirection(localOffsets[i]);
            RaycastHit[] hits = Physics.BoxCastAll(segmentWorldCenter, subBoxSize / 2f, direction, transform.rotation, moveDistance, combinedLayerMask);

            foreach (RaycastHit hit in hits) {
                int hitObjLayer = hit.collider.gameObject.layer;

                // 2. Duvar Temasý Kontrolü
                if ((wallLayer.value & (1 << hitObjLayer)) > 0) {

                    // SADECE ORTA KUTU (i == 1) duvara çarptýysa dalgayý yok et
                    if (i == 1) {
                        PlayImpactEffect(hit.point);
                        Destroy(gameObject);
                        return; // Dalga tamamen yok oldu
                    }

                    // Eðer 0 (Sol) veya 2 (Sað) kutu duvara çarptýysa hiçbir þey yapma!
                    // continue diyerek duvarý es geçmesini ve sýradaki temasa bakmasýný saðlýyoruz.
                    continue;
                }

                // 3. Mermi Kontrolü (Sol ve sað kutular mermileri hala temizleyebilir)
                if ((bulletLayer.value & (1 << hitObjLayer)) > 0) {
                    Destroy(hit.collider.gameObject);
                    continue;
                }

                // 4. Düþman Kontrolü (Sol ve sað kutular düþmanlarý hala kesebilir)
                if ((enemyLayer.value & (1 << hitObjLayer)) > 0) {
                    if (!alreadyHitColliders.Contains(hit.collider)) {
                        alreadyHitColliders.Add(hit.collider);

                        if (hit.collider.TryGetComponent<IDamagable>(out IDamagable damagable)) {
                            damagable.Damage(projectileDamage);
                            PlayImpactEffect(hit.point);
                        }
                    }
                }
            }
        }

        transform.position += direction * moveDistance;
    }

    private void PlayImpactEffect(Vector3 position) {
        Vector3 currentEulerAngles = transform.rotation.eulerAngles;

        float randomZOffset = Random.Range(-60f, 60f);
        currentEulerAngles.z = -90f + randomZOffset;

        var effect = Instantiate(impactEffect, position, Quaternion.Euler(currentEulerAngles));

        float randomScale = Random.Range(0.8f, 1.2f);
        effect.transform.localScale *= randomScale;

        Destroy(effect.gameObject, 1f);
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f);

        // 1. Kutunun boyutunu gizmo çizimi için de 3'e bölüyoruz
        Vector3 subBoxSize = new Vector3(hitboxSize.x / 3f, hitboxSize.y, hitboxSize.z);
        float curveDepth = -0.5f;

        // 2. Týpký Update'deki gibi kavisli lokasyonlarý belirliyoruz
        Vector3[] localOffsets = {
            new Vector3(-hitboxSize.x / 3f, 0, curveDepth), // Sol
            Vector3.zero,                                   // Orta
            new Vector3(hitboxSize.x / 3f, 0, curveDepth)   // Sað
        };

        // 3. Her bir parçayý ayrý ayrý çizdiriyoruz
        foreach (var offset in localOffsets) {
            // Parçanýn dünyadaki asýl yerini bul
            Vector3 segmentWorldCenter = transform.position + transform.TransformDirection(offset);

            // Gizmo'nun Matrix'ini o parçanýn yerine ve açýsýna taþý
            Gizmos.matrix = Matrix4x4.TRS(segmentWorldCenter, transform.rotation, Vector3.one);

            // Bölünmüþ ufak kutuyu çiz
            Gizmos.DrawCube(Vector3.zero, subBoxSize);
        }
    }
}