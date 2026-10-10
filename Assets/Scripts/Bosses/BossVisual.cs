using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

// Her boss türü için basit şekillerden model kurar ve canlandırır.
// Not: Kamera açısı yüzünden bossun yüksekliği ~2.1 birimi geçmemeli, yoksa ekranın üstünden taşar.
public class BossVisual : MonoBehaviour {

    private struct Spinner {
        public Transform transform;
        public float speed;
    }

    public Vector3 ColliderCenter { get; private set; }
    public Vector3 ColliderSize { get; private set; }
    public float FrontZ { get; private set; }
    public Vector3[] CannonOffsets { get; private set; } = new Vector3[0];

    private readonly List<Spinner> spinners = new List<Spinner>();
    private Transform core;
    private Vector3 coreBaseScale;
    private Transform root;
    private float spinMultiplier = 1f;

    public void Build(BossDefinition definition) {
        root = VisualFactory.CreateRoot(transform, "BossVisual");

        switch (definition.kind) {
            case BossKind.VortexSpire: BuildVortexSpire(definition); break;
            case BossKind.EmberLord: BuildEmberLord(definition); break;
            default: BuildIronWarden(definition); break;
        }

        coreBaseScale = core.localScale;
    }

    private void BuildIronWarden(BossDefinition d) {
        Material main = VisualFactory.Solid(d.primaryColor);
        Material dark = VisualFactory.Solid(VisualFactory.Shade(d.primaryColor, 0.45f));
        Material mid = VisualFactory.Solid(VisualFactory.Shade(d.primaryColor, 0.75f));
        Material accent = VisualFactory.Solid(d.accentColor);

        for (int side = -1; side <= 1; side += 2) {
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(side * 1.15f, 0.25f, 0f), new Vector3(0.7f, 0.5f, 2.3f), dark);
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(side * 1.15f, 0.47f, 0f), new Vector3(0.72f, 0.07f, 2.32f), mid);

            // Omuz topları
            VisualFactory.Part(PrimitiveType.Cylinder, root, new Vector3(side * 1.2f, 1.05f, -0.5f), new Vector3(0.32f, 0.45f, 0.32f), dark, new Vector3(90f, 0f, 0f));
            VisualFactory.GlowPart(PrimitiveType.Sphere, root, new Vector3(side * 1.2f, 1.05f, -0.97f), Vector3.one * 0.2f, d.accentColor, 3f);
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(side * 0.8f, 1.35f, 0.35f), new Vector3(0.16f, 0.45f, 0.16f), dark, new Vector3(0f, 0f, -side * 20f));
        }

        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 0.85f, 0f), new Vector3(2f, 0.8f, 1.8f), main);
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 0.62f, -0.95f), new Vector3(2f, 0.4f, 0.25f), mid, new Vector3(-25f, 0f, 0f));
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 1.2f, 0f), new Vector3(2.04f, 0.1f, 1.84f), accent);
        VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(0f, 1.5f, 0.05f), new Vector3(1.1f, 0.5f, 0.9f), mid);
        core = VisualFactory.GlowPart(PrimitiveType.Cube, root, new Vector3(0f, 1.52f, -0.42f), new Vector3(0.85f, 0.1f, 0.06f), d.accentColor, 4f);

        Transform radar = VisualFactory.CreateRoot(root, "Radar");
        radar.localPosition = new Vector3(0f, 1.8f, 0.15f);
        VisualFactory.Part(PrimitiveType.Cylinder, radar, new Vector3(0f, -0.03f, 0f), new Vector3(0.06f, 0.05f, 0.06f), dark);
        VisualFactory.Part(PrimitiveType.Cube, radar, new Vector3(0f, 0.02f, 0f), new Vector3(0.6f, 0.05f, 0.14f), mid);
        spinners.Add(new Spinner { transform = radar, speed = 120f });

        ColliderCenter = new Vector3(0f, 0.9f, 0f);
        ColliderSize = new Vector3(3f, 1.8f, 2.3f);
        FrontZ = -1.3f;
        CannonOffsets = new[] { new Vector3(-1.2f, 0f, -1.15f), new Vector3(1.2f, 0f, -1.15f) };
    }

    private void BuildVortexSpire(BossDefinition d) {
        Material main = VisualFactory.Solid(d.primaryColor);
        Material dark = VisualFactory.Solid(VisualFactory.Shade(d.primaryColor, 0.55f));
        Material light = VisualFactory.Solid(Color.Lerp(d.primaryColor, Color.white, 0.25f));

        VisualFactory.Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.12f, 0f), new Vector3(2.2f, 0.12f, 2.2f), dark);
        VisualFactory.Part(PrimitiveType.Cylinder, root, new Vector3(0f, 0.52f, 0f), new Vector3(1.5f, 0.28f, 1.5f), main);
        VisualFactory.GlowPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0.8f, 0f), new Vector3(1.56f, 0.025f, 1.56f), d.accentColor, 2f);
        VisualFactory.Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.04f, 0f), new Vector3(1.15f, 0.24f, 1.15f), light);
        VisualFactory.GlowPart(PrimitiveType.Cylinder, root, new Vector3(0f, 1.28f, 0f), new Vector3(1.2f, 0.025f, 1.2f), d.accentColor, 2f);
        VisualFactory.Part(PrimitiveType.Cylinder, root, new Vector3(0f, 1.45f, 0f), new Vector3(0.8f, 0.2f, 0.8f), main);
        core = VisualFactory.GlowPart(PrimitiveType.Sphere, root, new Vector3(0f, 1.85f, 0f), Vector3.one * 0.55f, d.accentColor, 3.5f);

        for (int i = 0; i < 4; i++) {
            float angle = (45f + i * 90f) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.05f;
            Vector3 tilt = new Vector3(Mathf.Sin(angle) * 25f, 0f, -Mathf.Cos(angle) * 25f);
            VisualFactory.Part(PrimitiveType.Cube, root, offset + Vector3.up * 0.35f, new Vector3(0.14f, 0.5f, 0.14f), dark, tilt);
        }

        // Etrafında dönen taş halkası
        Transform orbit = VisualFactory.CreateRoot(root, "Orbit");
        orbit.localPosition = new Vector3(0f, 1f, 0f);
        for (int i = 0; i < 8; i++) {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.35f;
            VisualFactory.Part(PrimitiveType.Cube, orbit, offset, Vector3.one * 0.22f, light, new Vector3(0f, -i * 45f, 45f));

            float glowAngle = (i * 45f + 22.5f) * Mathf.Deg2Rad;
            if (i % 2 == 0) {
                VisualFactory.GlowPart(PrimitiveType.Sphere, orbit, new Vector3(Mathf.Cos(glowAngle), 0f, Mathf.Sin(glowAngle)) * 1.35f, Vector3.one * 0.12f, d.accentColor, 3f);
            }
        }
        spinners.Add(new Spinner { transform = orbit, speed = 70f });

        ColliderCenter = new Vector3(0f, 1f, 0f);
        ColliderSize = new Vector3(2.6f, 2.1f, 2.6f);
        FrontZ = -1.35f;
        CannonOffsets = new[] { new Vector3(0f, 0f, -1.35f) };
    }

    private void BuildEmberLord(BossDefinition d) {
        Material main = VisualFactory.Solid(d.primaryColor);
        Material dark = VisualFactory.Solid(VisualFactory.Shade(d.primaryColor, 0.5f));
        Color crownColor = Color.Lerp(d.accentColor, Color.yellow, 0.5f);

        VisualFactory.GlowPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0.03f, 0f), new Vector3(2f, 0.02f, 2f), d.accentColor, 1.5f);
        VisualFactory.Part(PrimitiveType.Sphere, root, new Vector3(0f, 1f, 0f), new Vector3(1.5f, 1.5f, 1.4f), main);
        core = VisualFactory.GlowPart(PrimitiveType.Sphere, root, new Vector3(0f, 1f, -0.45f), Vector3.one * 0.75f, d.accentColor, 3.5f);

        for (int side = -1; side <= 1; side += 2) {
            VisualFactory.Part(PrimitiveType.Cube, root, new Vector3(side * 0.5f, 1.72f, 0f), new Vector3(0.14f, 0.6f, 0.14f), dark, new Vector3(0f, 0f, -side * 28f));
        }
        for (int i = 0; i < 5; i++) {
            float x = Mathf.Lerp(-0.3f, 0.3f, i / 4f);
            float height = i == 2 ? 0.34f : 0.24f;
            VisualFactory.GlowPart(PrimitiveType.Cube, root, new Vector3(x, 1.75f + height / 2f, 0.15f), new Vector3(0.09f, height, 0.09f), crownColor, 3f);
        }

        // Gövdenin etrafında dönen zırh plakaları
        Transform orbit = VisualFactory.CreateRoot(root, "ArmorOrbit");
        orbit.localPosition = new Vector3(0f, 0.9f, 0f);
        for (int i = 0; i < 4; i++) {
            float angleDeg = i * 90f;
            float angle = angleDeg * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * 1.15f;
            VisualFactory.Part(PrimitiveType.Cube, orbit, offset, new Vector3(0.5f, 0.7f, 0.1f), dark, new Vector3(0f, angleDeg, 0f));
            VisualFactory.GlowPart(PrimitiveType.Cube, orbit, offset * 1.06f, new Vector3(0.3f, 0.05f, 0.05f), d.accentColor, 2.5f, new Vector3(0f, angleDeg, 0f));
        }
        spinners.Add(new Spinner { transform = orbit, speed = -45f });

        ColliderCenter = new Vector3(0f, 0.95f, 0f);
        ColliderSize = new Vector3(2.4f, 1.9f, 2.2f);
        FrontZ = -1.15f;
        CannonOffsets = new[] { new Vector3(0f, 0f, -1.15f) };
    }

    public void SetEnraged() {
        spinMultiplier = 2.2f;
        root.DOPunchScale(Vector3.one * 0.2f, 0.5f, 6).SetLink(gameObject);
    }

    // Saldırı öncesi uyarı: çekirdek büyüyüp parlar
    public void Telegraph(float duration) {
        if (core == null) return;

        core.DOKill();
        core.localScale = coreBaseScale;
        core.DOScale(coreBaseScale * 1.6f, duration * 0.5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad).SetLink(gameObject);
    }

    private void Update() {
        foreach (Spinner spinner in spinners) {
            spinner.transform.Rotate(0f, spinner.speed * spinMultiplier * Time.deltaTime, 0f, Space.Self);
        }

        if (root != null) {
            root.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 1.6f) * 0.05f, 0f);
        }
    }

    private void OnDestroy() {
        if (core != null) core.DOKill();
        if (root != null) root.DOKill();
    }
}
