using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour {

    [Header("Hitbox Ayarlarý")]
    [SerializeField] private Transform hitboxCenter;
    [SerializeField] private Vector3 hitboxSize = new Vector3(1f, 2f, 1f);
    [SerializeField] private LayerMask bulletLayer;

    private Vector3 defaultLocalRotation;
    private int comboStep = 1;

    private bool isHitboxActive = false;
    private List<Collider> alreadyHitBullets = new List<Collider>();
    private float attackDuration = 0.1f;

    private void Start() {
        defaultLocalRotation = transform.localEulerAngles;

        if (Player.Instance != null)
            Player.Instance.OnAttack += Instance_OnAttack;
    }

    private void Update() {
        if (isHitboxActive) {
            CheckHits();
        }
    }

    private void CheckHits() {
        // Kutu içindeki mermileri bul
        Collider[] hits = Physics.OverlapBox(hitboxCenter.position, hitboxSize / 2, hitboxCenter.rotation, bulletLayer);

        foreach (Collider hit in hits) {
            // Ayný mermiyi tek savuruþta 2 kere algýlamamak için kontrol et
            if (!alreadyHitBullets.Contains(hit)) {
                alreadyHitBullets.Add(hit);

                if(hit.TryGetComponent<Bullet>(out Bullet bullet)) {
                    //Debug.Log("Mermiye vuruldu: " + bullet.name);
                    ParryBullet(bullet);
                }
                
            }
        }
    }

    private static void ParryBullet(Bullet bullet) {
        // Mermiyi yok et (veya yönünü geri çevirme kodunu buraya yaz)
        //Destroy(hit.gameObject);
        bullet.Direction = Player.Instance.transform.forward;
        bullet.ProjectileSpeed *= 5;
    }

    private void Instance_OnAttack(object sender, System.EventArgs e) {
        transform.DOKill();

        if (transform.localEulerAngles == defaultLocalRotation) {
            transform.localEulerAngles = new Vector3(0f, 0f, -90f);
        }

        if (comboStep == 1) {
            // Attack 1: OnStart ve OnComplete ile Hitbox'ý senkronize et
            DOVirtual.Float(0f, -180f, attackDuration, y => {
                transform.localEulerAngles = new Vector3(0f, y, -90f);
            })
            .SetEase(Ease.OutQuad)
            .SetTarget(transform)
            .OnStart(() => EnableHitbox())
            .OnComplete(() => DisableHitbox());

            comboStep = 2;
        }
        else {
            // Attack 2: OnStart ve OnComplete ile Hitbox'ý senkronize et
            DOVirtual.Float(-180f, 0f, attackDuration, y => {
                transform.localEulerAngles = new Vector3(0f, y, -90f);
            })
            .SetEase(Ease.OutQuad)
            .SetTarget(transform)
            .OnStart(() => EnableHitbox())
            .OnComplete(() => DisableHitbox());

            comboStep = 1;
        }

        transform.DOLocalRotate(defaultLocalRotation, 0.3f)
            .SetDelay(2.0f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                comboStep = 1;
            });
    }

    private void EnableHitbox() {
        alreadyHitBullets.Clear();
        isHitboxActive = true;
    }

    private void DisableHitbox() {
        isHitboxActive = false;
    }

    // Scene ekranýnda Hitbox'ý mavi yarý saydam bir kutu olarak görmek için
    private void OnDrawGizmos() {
        if (hitboxCenter == null) return;

        Gizmos.color = new Color(0f, 1f, 1f, 0.4f); // Cam göbeði/Mavi (Mermi temasýna uygun)
        Gizmos.matrix = Matrix4x4.TRS(hitboxCenter.position, hitboxCenter.rotation, hitboxCenter.lossyScale);
        Gizmos.DrawCube(Vector3.zero, hitboxSize);
    }

    private void OnDestroy() {
        if (Player.Instance != null)
            Player.Instance.OnAttack -= Instance_OnAttack;

        transform.DOKill();
    }
}