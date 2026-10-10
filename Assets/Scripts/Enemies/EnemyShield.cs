using DG.Tweening;
using System;
using UnityEngine;

// Düşmanın önünde duran enerji kalkanı. Önden gelen yansıyan mermileri bloklar;
// birkaç vuruşta kırılır ve bir süre sonra yenilenir.
// Duvardan sekip yandan gelen veya delici (pierce) mermiler kalkanı atlatabilir.
public class EnemyShield : MonoBehaviour {

    public static event Action<EnemyShield> OnAnyShieldSpawned;

    private static readonly Color PanelColor = new Color(0.12f, 0.27f, 0.55f);
    private static readonly Color FrameColor = new Color(0.35f, 0.85f, 1f);

    private const float ShieldZ = -0.72f;

    public bool IsUp { get; private set; }
    public int HitsRemaining { get; private set; }

    private Transform shieldRoot;
    private BoxCollider shieldCollider;
    private int maxHits;
    private float regenTime;

    private void Awake() {
        BalanceSettings balance = GameDatabase.Instance.balance;
        maxHits = Mathf.Max(1, balance.shieldHits);
        regenTime = balance.shieldRegenTime;

        // Kalkanlı düşman biraz daha fazla puan/altın verir
        if (TryGetComponent(out Enemy enemy)) {
            enemy.BonusScore += 1;
        }

        Build();
        Restore(animate: false);
    }

    private void Start() {
        OnAnyShieldSpawned?.Invoke(this);
    }

    private void Build() {
        // Ayrı bir obje: mermiler kalkana çarpınca düşmanın kendisine değil buraya hasar verir
        GameObject shieldObject = new GameObject("Shield");
        shieldObject.layer = gameObject.layer;
        shieldRoot = shieldObject.transform;
        shieldRoot.SetParent(transform, false);
        shieldRoot.localPosition = new Vector3(0f, 0.5f, ShieldZ);

        shieldCollider = shieldObject.AddComponent<BoxCollider>();
        shieldCollider.size = new Vector3(1.2f, 0.95f, 0.25f);

        ShieldHitbox hitbox = shieldObject.AddComponent<ShieldHitbox>();
        hitbox.Owner = this;

        Transform visual = VisualFactory.CreateRoot(shieldRoot, "ShieldVisual");
        MarkNoFlash(VisualFactory.Part(PrimitiveType.Cube, visual, Vector3.zero, new Vector3(1.1f, 0.8f, 0.06f), VisualFactory.Solid(PanelColor)));
        VisualFactory.GlowPart(PrimitiveType.Cube, visual, new Vector3(0f, 0.41f, 0f), new Vector3(1.16f, 0.06f, 0.09f), FrameColor, 3f);
        VisualFactory.GlowPart(PrimitiveType.Cube, visual, new Vector3(0f, -0.41f, 0f), new Vector3(1.16f, 0.06f, 0.09f), FrameColor, 3f);
        VisualFactory.GlowPart(PrimitiveType.Cube, visual, new Vector3(-0.56f, 0f, 0f), new Vector3(0.06f, 0.86f, 0.09f), FrameColor, 3f);
        VisualFactory.GlowPart(PrimitiveType.Cube, visual, new Vector3(0.56f, 0f, 0f), new Vector3(0.06f, 0.86f, 0.09f), FrameColor, 3f);
        VisualFactory.GlowPart(PrimitiveType.Cube, visual, new Vector3(0f, 0f, -0.035f), new Vector3(0.18f, 0.18f, 0.04f), FrameColor, 4f, new Vector3(0f, 0f, 45f));

        Material metal = VisualFactory.Solid(new Color(0.22f, 0.24f, 0.3f));
        MarkNoFlash(VisualFactory.Part(PrimitiveType.Cube, visual, new Vector3(-0.32f, 0f, 0.18f), new Vector3(0.08f, 0.08f, 0.3f), metal));
        MarkNoFlash(VisualFactory.Part(PrimitiveType.Cube, visual, new Vector3(0.32f, 0f, 0.18f), new Vector3(0.08f, 0.08f, 0.3f), metal));

        // Düşmanın hasar yanıp sönmesi kalkanı etkilemesin
        if (TryGetComponent(out DamageFlash damageFlash)) {
            damageFlash.RefreshRenderers();
        }
    }

    private static void MarkNoFlash(Transform part) {
        part.gameObject.AddComponent<NoDamageFlash>();
    }

    // Kalkana çarpan her yansıyan mermi bir vuruş sayılır (hasar miktarı önemli değil)
    public void Block() {
        if (!IsUp) return;

        HitsRemaining--;
        GameEvents.ShieldBlocked(shieldRoot.position);

        shieldRoot.DOKill(true);
        if (HitsRemaining <= 0) {
            Break();
        }
        else {
            shieldRoot.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.2f, 8).SetLink(gameObject);
        }
    }

    private void Break() {
        IsUp = false;
        shieldCollider.enabled = false;

        if (FeedbackFX.Instance != null) {
            FeedbackFX.Instance.Debris(shieldRoot.position, FrameColor, 10, 1.4f, 0.12f);
        }

        shieldRoot.DOScale(0f, 0.15f).SetEase(Ease.InBack).SetLink(gameObject);
        DOVirtual.DelayedCall(regenTime, () => Restore(animate: true)).SetLink(gameObject);
    }

    private void Restore(bool animate) {
        HitsRemaining = maxHits;
        IsUp = true;
        shieldCollider.enabled = true;

        shieldRoot.DOKill();
        if (animate) {
            shieldRoot.localScale = Vector3.zero;
            shieldRoot.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetLink(gameObject);
        }
        else {
            shieldRoot.localScale = Vector3.one;
        }
    }

    private void OnDestroy() {
        if (shieldRoot != null) shieldRoot.DOKill();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        OnAnyShieldSpawned = null;
    }
}
