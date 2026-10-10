using System.Collections.Generic;
using UnityEngine;

// Kuşanılan silahın modelini Melee objesinin altına kurar.
// Bıçak Melee'nin yerel +Y ekseni boyunca uzanır (orijinal küp kılıç gibi);
// keskin kenar yerel Z ekseninde durur ki yatay savuruşta kenar önde gitsin.
public class WeaponVisual : MonoBehaviour {

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    public const float BaseBladeLength = 1.35f; // orijinal kılıcın boyu

    private static readonly Color Wood = new Color(0.45f, 0.29f, 0.17f);
    private static readonly Color Leather = new Color(0.3f, 0.18f, 0.12f);
    private static readonly Color Gold = new Color(0.95f, 0.76f, 0.31f);
    private static readonly Color DarkIron = new Color(0.25f, 0.27f, 0.32f);
    private static readonly Color Crimson = new Color(0.72f, 0.12f, 0.18f);

    private Transform modelRoot;

    public void Build(WeaponDefinition weapon, List<TrailRenderer> trails, float reachMultiplier = 1f) {
        VisualFactory.HideRenderer(transform.Find("Mesh"));

        if (modelRoot != null) {
            Destroy(modelRoot.gameObject);
        }
        modelRoot = VisualFactory.CreateRoot(transform, "WeaponModel");

        float length = BaseBladeLength * weapon.hitboxScale.y * reachMultiplier;
        Material blade = VisualFactory.Solid(weapon.bladeColor);

        switch (weapon.style) {
            case WeaponStyle.Katana: BuildKatana(length, blade, weapon.trailColor); break;
            case WeaponStyle.Axe: BuildAxe(length, blade, weapon.trailColor); break;
            case WeaponStyle.Spear: BuildSpear(length, blade, weapon.trailColor); break;
            case WeaponStyle.Daggers: BuildDaggers(length, blade, weapon.trailColor); break;
            default: BuildSword(length, blade, weapon.trailColor); break;
        }

        foreach (TrailRenderer trail in trails) {
            Material trailMaterial = trail.material; // örneğe özel kopya
            Color baseColor = weapon.trailColor;
            baseColor.a = trailMaterial.HasProperty(BaseColorId) ? trailMaterial.GetColor(BaseColorId).a : 1f;
            trailMaterial.SetColor(BaseColorId, baseColor);
            if (trailMaterial.HasProperty(EmissionColorId)) {
                trailMaterial.SetColor(EmissionColorId, weapon.trailColor * 4f);
            }
        }
    }

    private void BuildSword(float length, Material blade, Color glow) {
        float bladeLength = length - 0.12f;

        VisualFactory.Part(PrimitiveType.Sphere, modelRoot, new Vector3(0f, -0.17f, 0f), Vector3.one * 0.08f, VisualFactory.Solid(Gold));
        VisualFactory.Part(PrimitiveType.Cylinder, modelRoot, new Vector3(0f, -0.06f, 0f), new Vector3(0.05f, 0.08f, 0.05f), VisualFactory.Solid(Leather));
        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, 0.04f, 0f), new Vector3(0.06f, 0.05f, 0.32f), VisualFactory.Solid(Gold));
        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, 0.06f + bladeLength / 2f, 0f), new Vector3(0.025f, bladeLength, 0.1f), blade);
        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, 0.06f + bladeLength, 0f), new Vector3(0.025f, 0.07f, 0.07f), blade, new Vector3(45f, 0f, 0f));
        VisualFactory.GlowPart(PrimitiveType.Cube, modelRoot, new Vector3(0f, 0.12f + bladeLength * 0.4f, 0f), new Vector3(0.03f, bladeLength * 0.7f, 0.014f), glow, 2.5f);
    }

    private void BuildKatana(float length, Material blade, Color glow) {
        float lower = length * 0.55f;
        float upper = length * 0.45f;

        VisualFactory.Part(PrimitiveType.Cylinder, modelRoot, new Vector3(0f, -0.12f, 0f), new Vector3(0.045f, 0.13f, 0.045f), VisualFactory.Solid(Crimson));
        VisualFactory.Part(PrimitiveType.Cylinder, modelRoot, new Vector3(0f, 0.03f, 0f), new Vector3(0.18f, 0.012f, 0.18f), VisualFactory.Solid(DarkIron));
        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, 0.07f, 0f), new Vector3(0.03f, 0.05f, 0.08f), VisualFactory.Solid(Gold));
        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, 0.08f + lower / 2f, 0f), new Vector3(0.02f, lower, 0.07f), blade);

        // Hafif kavisli üst kısım
        Transform tip = VisualFactory.CreateRoot(modelRoot, "Tip");
        tip.localPosition = new Vector3(0f, 0.08f + lower, 0f);
        tip.localRotation = Quaternion.Euler(-7f, 0f, 0f);
        VisualFactory.Part(PrimitiveType.Cube, tip, new Vector3(0f, upper / 2f, 0f), new Vector3(0.02f, upper, 0.065f), blade);
        VisualFactory.GlowPart(PrimitiveType.Cube, tip, new Vector3(0f, upper / 2f, -0.034f), new Vector3(0.022f, upper * 0.9f, 0.008f), glow, 2.5f);
        VisualFactory.GlowPart(PrimitiveType.Cube, modelRoot, new Vector3(0f, 0.1f + lower / 2f, -0.036f), new Vector3(0.022f, lower * 0.9f, 0.008f), glow, 2.5f);
    }

    private void BuildAxe(float length, Material blade, Color glow) {
        float headY = length - 0.25f;

        VisualFactory.Part(PrimitiveType.Cylinder, modelRoot, new Vector3(0f, length / 2f - 0.18f, 0f), new Vector3(0.055f, length / 2f, 0.055f), VisualFactory.Solid(Wood));
        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, headY, 0f), new Vector3(0.09f, 0.18f, 0.12f), VisualFactory.Solid(DarkIron));

        for (int side = -1; side <= 1; side += 2) {
            VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, headY, side * 0.2f), new Vector3(0.04f, 0.34f, 0.3f), blade);
            VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, headY, side * 0.33f), new Vector3(0.042f, 0.46f, 0.08f), blade);
            VisualFactory.GlowPart(PrimitiveType.Cube, modelRoot, new Vector3(0f, headY, side * 0.375f), new Vector3(0.045f, 0.44f, 0.02f), glow, 2.5f);
        }

        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, length - 0.02f, 0f), new Vector3(0.04f, 0.16f, 0.04f), VisualFactory.Solid(DarkIron), new Vector3(0f, 45f, 0f));
    }

    private void BuildSpear(float length, Material blade, Color glow) {
        float shaftLength = length - 0.32f;

        VisualFactory.Part(PrimitiveType.Cylinder, modelRoot, new Vector3(0f, shaftLength / 2f - 0.15f, 0f), new Vector3(0.042f, shaftLength / 2f, 0.042f), VisualFactory.Solid(Wood));
        VisualFactory.Part(PrimitiveType.Cylinder, modelRoot, new Vector3(0f, shaftLength - 0.15f, 0f), new Vector3(0.065f, 0.03f, 0.065f), VisualFactory.Solid(Gold));
        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, shaftLength - 0.22f, 0f), new Vector3(0.05f, 0.1f, 0.05f), VisualFactory.Solid(Crimson));

        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, length - 0.27f, 0f), new Vector3(0.025f, 0.26f, 0.13f), blade);
        VisualFactory.Part(PrimitiveType.Cube, modelRoot, new Vector3(0f, length - 0.12f, 0f), new Vector3(0.025f, 0.11f, 0.11f), blade, new Vector3(45f, 0f, 0f));
        VisualFactory.GlowPart(PrimitiveType.Cube, modelRoot, new Vector3(0f, length - 0.25f, 0f), new Vector3(0.03f, 0.3f, 0.02f), glow, 3f);
    }

    private void BuildDaggers(float length, Material blade, Color glow) {
        float bladeLength = length * 0.6f;

        for (int side = -1; side <= 1; side += 2) {
            Transform dagger = VisualFactory.CreateRoot(modelRoot, "Dagger");
            dagger.localPosition = new Vector3(0f, 0f, side * 0.14f);
            dagger.localRotation = Quaternion.Euler(side * 8f, 0f, 0f);

            VisualFactory.Part(PrimitiveType.Cylinder, dagger, new Vector3(0f, -0.06f, 0f), new Vector3(0.04f, 0.07f, 0.04f), VisualFactory.Solid(Leather));
            VisualFactory.Part(PrimitiveType.Cube, dagger, new Vector3(0f, 0.03f, 0f), new Vector3(0.045f, 0.035f, 0.17f), VisualFactory.Solid(DarkIron));
            VisualFactory.Part(PrimitiveType.Cube, dagger, new Vector3(0f, 0.05f + bladeLength / 2f, 0f), new Vector3(0.02f, bladeLength, 0.075f), blade);
            VisualFactory.Part(PrimitiveType.Cube, dagger, new Vector3(0f, 0.05f + bladeLength, 0f), new Vector3(0.02f, 0.055f, 0.055f), blade, new Vector3(45f, 0f, 0f));
            VisualFactory.GlowPart(PrimitiveType.Cube, dagger, new Vector3(0f, 0.08f + bladeLength * 0.45f, 0f), new Vector3(0.025f, bladeLength * 0.75f, 0.012f), glow, 3f);
        }
    }

    private void OnDestroy() {
        if (modelRoot != null) {
            Destroy(modelRoot.gameObject);
        }
    }
}
