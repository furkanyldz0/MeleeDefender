using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

// Yeşil kutuyu mazgallı bir kale duvarına çevirir.
// Kamera kaleyi arkadan ve yukarıdan gördüğü için detaylar duvarın üst kısmında yoğunlaşır.
public class BaseVisual : MonoBehaviour {

    private static readonly Color Stone = new Color(0.48f, 0.52f, 0.6f);
    private static readonly Color DarkStone = new Color(0.3f, 0.33f, 0.4f);
    private static readonly Color Banner = new Color(0.18f, 0.36f, 0.92f);
    private static readonly Color Gold = new Color(0.95f, 0.76f, 0.31f);
    private static readonly Color CrystalColor = new Color(0.36f, 0.85f, 1f);

    private const float WallLength = 9f;
    private const float TowerX = 4.15f;

    private Base baseObject;
    private Transform crystal;
    private readonly List<Transform> flags = new List<Transform>();

    private void Awake() {
        baseObject = GetComponent<Base>();

        VisualFactory.HideRenderer(transform.Find("Mesh"));
        Build();

        if (TryGetComponent(out DamageFlash damageFlash)) {
            damageFlash.RefreshRenderers();
        }
    }

    private void Start() {
        baseObject.OnHealthValuesChanged += Base_OnHealthValuesChanged;

        // Kalenin canı artık HUD'da gösteriliyor; duvarın üstündeki eski dünya-uzayı çubuğu modeli kapatıyordu
        foreach (HealthBarUI healthBar in GetComponentsInChildren<HealthBarUI>(true)) {
            Destroy(healthBar.gameObject);
        }
    }

    private void Build() {
        Transform root = VisualFactory.CreateRoot(transform, "CastleVisual");

        Material stone = VisualFactory.Solid(Stone);
        Material darkStone = VisualFactory.Solid(DarkStone);
        Material banner = VisualFactory.Solid(Banner);
        Material gold = VisualFactory.Solid(Gold);
        Material wood = VisualFactory.Solid(new Color(0.36f, 0.23f, 0.14f));

        // Duvar gövdesi
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 0.09f, 0f), new Vector3(WallLength + 0.1f, 0.18f, 1.02f), darkStone);
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 0.5f, 0f), new Vector3(WallLength, 0.9f, 0.9f), stone);
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 0.98f, 0f), new Vector3(WallLength + 0.1f, 0.1f, 1f), darkStone);

        // Mazgallar (düşmana bakan ön kenarda)
        for (float x = -3.6f; x <= 3.61f; x += 0.8f) {
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(x, 1.19f, 0.34f), new Vector3(0.42f, 0.32f, 0.28f), stone);
        }
        // Arka korkuluk
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 1.08f, -0.42f), new Vector3(WallLength - 1.4f, 0.12f, 0.1f), darkStone);

        // Duvardaki sancaklar (kameraya bakan iç yüzde)
        for (int i = -1; i <= 1; i += 2) {
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(i * 2.2f, 0.62f, -0.46f), new Vector3(0.55f, 0.6f, 0.02f), banner);
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(i * 2.2f, 0.66f, -0.475f), new Vector3(0.16f, 0.16f, 0.02f), gold, new Vector3(0f, 0f, 45f));
        }

        // Kuleler
        for (int i = -1; i <= 1; i += 2) {
            float x = i * TowerX;
            VisualFactory.Part(PrimitiveType.Cylinder, root, new Vector3(x, 0.85f, 0f), new Vector3(1f, 0.85f, 1f), stone);
            VisualFactory.Part(PrimitiveType.Cylinder, root, new Vector3(x, 1.72f, 0f), new Vector3(1.16f, 0.06f, 1.16f), darkStone);

            for (int k = 0; k < 6; k++) {
                float angle = k * 60f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.48f;
                VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(x, 1.88f, 0f) + offset, new Vector3(0.22f, 0.26f, 0.22f), stone, new Vector3(0f, -k * 60f, 0f));
            }

            // Bayrak direği ve dalgalanan bayrak
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(x, 2.25f, 0f), new Vector3(0.05f, 0.9f, 0.05f), wood);
            Transform flagPivot = VisualFactory.CreateRoot(root, "FlagPivot");
            flagPivot.localPosition = new Vector3(x, 2.55f, 0f);
            VisualFactory.Part(PrimitiveType.Cube, flagPivot, new Vector3(0f, 0f, -0.24f), new Vector3(0.02f, 0.3f, 0.46f), banner);
            VisualFactory.Part(PrimitiveType.Cube, flagPivot, new Vector3(0f, 0f, -0.25f), new Vector3(0.03f, 0.08f, 0.12f), gold);
            flags.Add(flagPivot);
        }

        // Kapının üstünde yüzen koruma kristali
        VisualFactory.Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.08f, 0f), new Vector3(0.6f, 0.06f, 0.6f), gold);
        crystal = VisualFactory.GlowPart(PrimitiveType.Cube, root, new Vector3(0f, 1.7f, 0f), Vector3.one * 0.34f, CrystalColor, 4f, new Vector3(45f, 0f, 45f));
    }

    private void Update() {
        float t = Time.time;

        if (crystal != null) {
            crystal.localPosition = new Vector3(0f, 1.7f + Mathf.Sin(t * 1.8f) * 0.08f, 0f);
            crystal.localRotation = Quaternion.Euler(45f, t * 50f, 45f);
        }

        for (int i = 0; i < flags.Count; i++) {
            flags[i].localRotation = Quaternion.Euler(0f, Mathf.Sin(t * 3f + i) * 18f, 0f);
        }
    }

    private void Base_OnHealthValuesChanged(float current, float max) {
        if (crystal == null) return;

        // Kale hasar aldıkça kristal küçülür
        float ratio = max > 0f ? current / max : 0f;
        crystal.DOKill();
        crystal.localScale = Vector3.one * Mathf.Lerp(0.18f, 0.34f, ratio);
        crystal.DOPunchScale(Vector3.one * 0.12f, 0.25f, 8).SetLink(gameObject);
    }

    private void OnDestroy() {
        if (baseObject != null) {
            baseObject.OnHealthValuesChanged -= Base_OnHealthValuesChanged;
        }
        if (crystal != null) crystal.DOKill();
    }
}
