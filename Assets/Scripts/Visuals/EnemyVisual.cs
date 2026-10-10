using DG.Tweening;
using UnityEngine;

// Düşmanın küp gövdesini küçük bir nöbetçi robota çevirir.
// Renk, prefabdaki orijinal materyalden okunur; böylece yeni düşman tipleri otomatik farklı görünür.
public class EnemyVisual : MonoBehaviour {

    private static readonly Color DefaultEye = new Color(1f, 0.82f, 0.3f);
    private static readonly Color EliteEye = new Color(0.36f, 0.95f, 1f);

    private Enemy enemy;
    private Transform head;
    private Transform eye;
    private Transform antennaTip;
    private float phase;

    private void Awake() {
        enemy = GetComponent<Enemy>();
        phase = Random.Range(0f, 10f);

        Transform originalBody = transform.Find("Mesh");
        Color mainColor = VisualFactory.GetBaseColor(originalBody != null ? originalBody.GetComponent<Renderer>() : null, new Color(0.85f, 0.1f, 0.1f));
        VisualFactory.HideRenderer(originalBody);

        Build(mainColor, enemy != null && enemy.ScoreToKill >= 2);

        // Yeni parçaların da hasar alınca parlaması için
        if (TryGetComponent(out DamageFlash damageFlash)) {
            damageFlash.RefreshRenderers();
        }
    }

    private void Start() {
        if (enemy != null) {
            enemy.OnAboutToShoot += Enemy_OnAboutToShoot;
        }
    }

    private void Build(Color mainColor, bool isElite) {
        Transform root = VisualFactory.CreateRoot(transform, "SentryVisual");

        Material main = VisualFactory.Solid(mainColor);
        Material dark = VisualFactory.Solid(VisualFactory.Shade(mainColor, 0.45f));
        Material light = VisualFactory.Solid(Color.Lerp(mainColor, Color.white, 0.25f));
        Material metal = VisualFactory.Solid(new Color(0.22f, 0.24f, 0.3f));
        Color eyeColor = isElite ? EliteEye : DefaultEye;

        // Alt gövde
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 0.06f, 0f), new Vector3(0.92f, 0.12f, 0.82f), metal);
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 0.38f, 0f), new Vector3(0.8f, 0.5f, 0.72f), main);
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 0.26f, -0.37f), new Vector3(0.84f, 0.16f, 0.06f), dark);
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(-0.43f, 0.4f, 0f), new Vector3(0.06f, 0.28f, 0.44f), dark);
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0.43f, 0.4f, 0f), new Vector3(0.06f, 0.28f, 0.44f), dark);

        // Silah namlusunu gövdeye bağlayan yuva
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(-0.3f, 0.53f, -0.42f), new Vector3(0.24f, 0.24f, 0.2f), metal);

        // Kafa (hafifçe süzülür ve döner)
        head = VisualFactory.CreateRoot(root, "Head");
        head.localPosition = new Vector3(0f, 0.68f, 0f);
        VisualFactory.Part(PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.66f, 0.44f, 0.62f), light);
        VisualFactory.Part(PrimitiveType.Cube, head, new Vector3(0f, 0.03f, -0.26f), new Vector3(0.5f, 0.11f, 0.08f), metal);
        eye = VisualFactory.GlowPart(PrimitiveType.Cube, head, new Vector3(0f, 0.03f, -0.3f), new Vector3(0.34f, 0.05f, 0.03f), eyeColor, 4f);

        VisualFactory.Part(PrimitiveType.Cube, head, new Vector3(0.2f, 0.3f, 0.05f), new Vector3(0.03f, 0.22f, 0.03f), metal);
        antennaTip = VisualFactory.GlowPart(PrimitiveType.Sphere, head, new Vector3(0.2f, 0.42f, 0.05f), Vector3.one * 0.07f, eyeColor, 3f);

        if (isElite) {
            // Elit düşman: omuz dikenleri ve ikinci anten
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(-0.36f, 0.7f, 0f), new Vector3(0.08f, 0.3f, 0.08f), dark, new Vector3(0f, 0f, 25f));
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0.36f, 0.7f, 0f), new Vector3(0.08f, 0.3f, 0.08f), dark, new Vector3(0f, 0f, -25f));
            VisualFactory.Part(PrimitiveType.Cube, head, new Vector3(-0.2f, 0.3f, 0.05f), new Vector3(0.03f, 0.22f, 0.03f), metal);
            VisualFactory.GlowPart(PrimitiveType.Sphere, head, new Vector3(-0.2f, 0.42f, 0.05f), Vector3.one * 0.07f, eyeColor, 3f);
        }
    }

    private void Update() {
        if (head == null) return;

        float t = Time.time + phase;
        head.localPosition = new Vector3(0f, 0.68f + Mathf.Sin(t * 2.2f) * 0.025f, 0f);
        head.localRotation = Quaternion.Euler(0f, Mathf.Sin(t * 0.9f) * 12f, 0f);
        antennaTip.localScale = Vector3.one * (0.07f + Mathf.Max(0f, Mathf.Sin(t * 5f)) * 0.025f);
    }

    // Ateş etmeden hemen önce göz büyür: oyuncuya tepki süresi kazandırır
    private void Enemy_OnAboutToShoot() {
        if (eye == null) return;

        eye.DOKill(true);
        eye.DOScale(new Vector3(0.42f, 0.11f, 0.03f), 0.12f).SetLoops(2, LoopType.Yoyo).SetLink(gameObject);
    }

    private void OnDestroy() {
        if (enemy != null) {
            enemy.OnAboutToShoot -= Enemy_OnAboutToShoot;
        }
        if (eye != null) eye.DOKill();
    }
}
