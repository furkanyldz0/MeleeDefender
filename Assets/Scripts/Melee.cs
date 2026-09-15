using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Melee : MonoBehaviour {

    [Header("Hitbox Ayarlarý")]
    [SerializeField] private Transform hitboxCenter;
    [SerializeField] private Vector3 hitboxSize = new Vector3(1f, 2f, 1f);
    [SerializeField] private LayerMask bulletLayer;
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private Transform trailRendererParent;
        
    private int comboStep = 1;

    private bool isHitboxActive = false;
    private List<Collider> alreadyHitBullets = new List<Collider>();
    private float attackDuration = 0.1f;

    private List<TrailRenderer> trails = new List<TrailRenderer>();    

    private void Start() {
        Player.Instance.OnAttack += Instance_OnAttack;

        trails = GetComponentsInChildren<TrailRenderer>().ToList();

        DisableTrails();
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
                    if (!bullet.IsParried) {
                        ParryBullet(bullet);
                        PlayImpactEffect(bullet.transform.position);
                        HitStop.Instance.StopTime(0.02f);
                    }
                }
                
            }
        }
    }

    private void ParryBullet(Bullet bullet) { //düþman parrylerse deðiþiriz, þuan sadece player
        float defaultDamageMultiplier = 1.5f;
        float speedDamageMultiplier = bullet.ProjectileSpeed * 0.1f;
        float finalDamageMultiplier = defaultDamageMultiplier + speedDamageMultiplier;

        float parriedBulletSpeed = bullet.ProjectileSpeed * 5f;

        //float turnSpeedMultiplier = 0.03f;
        //float extraSpeedFromTurn = Player.Instance.CurrentTurnSpeed * turnSpeedMultiplier;
        //float finalBulletSpeed = (bullet.ProjectileSpeed * 5f) + extraSpeedFromTurn;
        //float speedDamageMultiplier =  (finalBulletSpeed / parriedBulletSpeed) - 1;
        //speedDamageMultiplier = Mathf.Clamp(speedDamageMultiplier, 0f, 4f);

        Debug.Log(this + " hasar çarpaný: " + finalDamageMultiplier);

        bullet.BeParried(Player.Instance.transform.forward, parriedBulletSpeed, finalDamageMultiplier);
    }

    private void Instance_OnAttack(object sender, System.EventArgs e) {
        transform.DOKill();

        if (comboStep == 1) {
            // Attack 1: OnStart ve OnComplete ile Hitbox'ý senkronize et
            DOVirtual.Float(0f, -180f, attackDuration, y => {
                transform.localEulerAngles = new Vector3(0f, y, -90f);
            })
            .SetEase(Ease.OutQuad)
            .SetTarget(transform)
            .OnStart(() => {
                EnableHitbox();
                EnableTrails();
            })
            .OnComplete(() => {
                DisableHitbox();
                DisableTrails();
            });

            comboStep = 2;
        }
        else {
            // Attack 2: OnStart ve OnComplete ile Hitbox'ý senkronize et
            DOVirtual.Float(-180f, 0f, attackDuration, y => {
                transform.localEulerAngles = new Vector3(0f, y, -90f);
            })
            .SetEase(Ease.OutQuad)
            .SetTarget(transform)
            .OnStart(() => {
                EnableHitbox();
                EnableTrails();

            })
            .OnComplete(() => {
                DisableHitbox();
                DisableTrails();
            });

            comboStep = 1;
        }
    }

    private void PlayImpactEffect(Vector3 position) {
        var effect = Instantiate(impactEffect, position, Quaternion.identity);

        Destroy(effect.gameObject, 1f);
    }
    
    private void EnableHitbox() {
        alreadyHitBullets.Clear();
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