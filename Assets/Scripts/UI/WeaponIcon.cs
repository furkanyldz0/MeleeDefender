using UnityEngine;
using UnityEngine.UI;

// Cephanelikte gösterilen silah simgesi: oyundaki 3D modelin 2D karşılığı, UI parçalarından kurulur.
public static class WeaponIcon {

    private static readonly Color Wood = new Color(0.55f, 0.36f, 0.22f);
    private static readonly Color Leather = new Color(0.36f, 0.22f, 0.15f);
    private static readonly Color Gold = new Color(0.95f, 0.76f, 0.31f);
    private static readonly Color DarkIron = new Color(0.3f, 0.32f, 0.38f);
    private static readonly Color Crimson = new Color(0.78f, 0.16f, 0.22f);

    public static RectTransform Build(Transform parent, WeaponDefinition weapon, float size) {
        RectTransform root = UIFactory.Rect("WeaponIcon", parent);
        root.sizeDelta = new Vector2(size, size);

        Image glow = UIFactory.Image("Glow", root, new Color(weapon.trailColor.r, weapon.trailColor.g, weapon.trailColor.b, 0.35f), UIFactory.Glow);
        glow.rectTransform.Center(Vector2.zero, new Vector2(size * 1.2f, size * 1.2f));

        // Çapraz duruş (sağ üste bakan bıçak)
        RectTransform pivot = UIFactory.Rect("Pivot", root);
        pivot.Center(Vector2.zero, new Vector2(size, size));
        pivot.localRotation = Quaternion.Euler(0f, 0f, -45f);
        float s = size / 200f;

        switch (weapon.style) {
            case WeaponStyle.Katana: BuildKatana(pivot, weapon, s); break;
            case WeaponStyle.Axe: BuildAxe(pivot, weapon, s); break;
            case WeaponStyle.Spear: BuildSpear(pivot, weapon, s); break;
            case WeaponStyle.Daggers: BuildDaggers(pivot, weapon, s); break;
            default: BuildSword(pivot, weapon, s); break;
        }

        return root;
    }

    private static void Piece(Transform parent, Color color, Vector2 position, Vector2 size, float s, float rotation = 0f, float cornerScale = 4f) {
        Image image = UIFactory.Panel("Piece", parent, color, cornerScale);
        image.rectTransform.Center(position * s, size * s);
        image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    private static void Dot(Transform parent, Color color, Vector2 position, float diameter, float s) {
        Image image = UIFactory.Image("Dot", parent, color, UIFactory.Circle);
        image.rectTransform.Center(position * s, Vector2.one * diameter * s);
    }

    private static void BuildSword(Transform p, WeaponDefinition w, float s) {
        Piece(p, w.bladeColor, new Vector2(0f, 30f), new Vector2(18f, 150f), s);
        Piece(p, w.bladeColor, new Vector2(0f, 104f), new Vector2(13f, 13f), s, 45f, 8f);
        Piece(p, w.trailColor, new Vector2(0f, 24f), new Vector2(4f, 110f), s);
        Piece(p, Gold, new Vector2(0f, -48f), new Vector2(66f, 12f), s);
        Piece(p, Leather, new Vector2(0f, -72f), new Vector2(12f, 38f), s);
        Dot(p, Gold, new Vector2(0f, -94f), 18f, s);
    }

    private static void BuildKatana(Transform p, WeaponDefinition w, float s) {
        Piece(p, w.bladeColor, new Vector2(0f, 10f), new Vector2(12f, 110f), s);
        Piece(p, w.bladeColor, new Vector2(-5f, 95f), new Vector2(11f, 70f), s, 7f);
        Piece(p, w.trailColor, new Vector2(5f, 30f), new Vector2(3f, 150f), s, 2f);
        Dot(p, DarkIron, new Vector2(0f, -50f), 34f, s);
        Piece(p, Gold, new Vector2(0f, -50f), new Vector2(16f, 8f), s);
        Piece(p, Crimson, new Vector2(0f, -82f), new Vector2(12f, 52f), s);
    }

    private static void BuildAxe(Transform p, WeaponDefinition w, float s) {
        Piece(p, Wood, new Vector2(0f, -10f), new Vector2(13f, 190f), s);
        Piece(p, DarkIron, new Vector2(0f, 62f), new Vector2(22f, 34f), s);
        Piece(p, w.bladeColor, new Vector2(-32f, 62f), new Vector2(48f, 58f), s, 0f, 2f);
        Piece(p, w.bladeColor, new Vector2(32f, 62f), new Vector2(48f, 58f), s, 0f, 2f);
        Piece(p, w.trailColor, new Vector2(-56f, 62f), new Vector2(5f, 62f), s);
        Piece(p, w.trailColor, new Vector2(56f, 62f), new Vector2(5f, 62f), s);
        Piece(p, DarkIron, new Vector2(0f, 92f), new Vector2(10f, 22f), s);
    }

    private static void BuildSpear(Transform p, WeaponDefinition w, float s) {
        Piece(p, Wood, new Vector2(0f, -20f), new Vector2(10f, 180f), s);
        Piece(p, Gold, new Vector2(0f, 70f), new Vector2(18f, 8f), s);
        Piece(p, Crimson, new Vector2(0f, 58f), new Vector2(14f, 16f), s);
        Piece(p, w.bladeColor, new Vector2(0f, 96f), new Vector2(22f, 44f), s);
        Piece(p, w.bladeColor, new Vector2(0f, 118f), new Vector2(22f, 22f), s, 45f, 8f);
        Piece(p, w.trailColor, new Vector2(0f, 98f), new Vector2(4f, 40f), s);
    }

    private static void BuildDaggers(Transform p, WeaponDefinition w, float s) {
        for (int side = -1; side <= 1; side += 2) {
            RectTransform dagger = UIFactory.Rect("Dagger", p);
            dagger.Center(new Vector2(side * 26f * s, -10f * s), new Vector2(100f * s, 200f * s));
            dagger.localRotation = Quaternion.Euler(0f, 0f, side * 18f);

            Piece(dagger, w.bladeColor, new Vector2(0f, 30f), new Vector2(14f, 90f), s);
            Piece(dagger, w.bladeColor, new Vector2(0f, 76f), new Vector2(10f, 10f), s, 45f, 8f);
            Piece(dagger, w.trailColor, new Vector2(0f, 28f), new Vector2(3f, 66f), s);
            Piece(dagger, DarkIron, new Vector2(0f, -18f), new Vector2(40f, 9f), s);
            Piece(dagger, Leather, new Vector2(0f, -38f), new Vector2(10f, 30f), s);
        }
    }
}
