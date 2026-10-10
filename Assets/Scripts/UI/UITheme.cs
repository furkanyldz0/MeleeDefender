using UnityEngine;

// Arayüzün renk paleti ve ölçüleri tek yerde.
public static class UITheme {

    public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

    // Yüzeyler
    public static readonly Color Overlay = new Color(0.03f, 0.04f, 0.08f, 0.78f);
    public static readonly Color OverlayLight = new Color(0.03f, 0.04f, 0.08f, 0.55f);
    public static readonly Color Panel = new Color(0.07f, 0.09f, 0.16f, 0.92f);
    public static readonly Color PanelLight = new Color(0.12f, 0.15f, 0.25f, 0.95f);
    public static readonly Color PanelBorder = new Color(0.27f, 0.33f, 0.52f, 1f);
    public static readonly Color BarBackground = new Color(0.03f, 0.04f, 0.07f, 0.9f);

    // Vurgu renkleri
    public static readonly Color Gold = new Color(1f, 0.71f, 0.28f);
    public static readonly Color GoldDark = new Color(0.72f, 0.45f, 0.1f);
    public static readonly Color Cyan = new Color(0.31f, 0.82f, 1f);
    public static readonly Color Danger = new Color(1f, 0.3f, 0.37f);
    public static readonly Color Success = new Color(0.36f, 0.89f, 0.49f);
    public static readonly Color Xp = new Color(0.65f, 0.47f, 1f);
    public static readonly Color Neutral = new Color(0.22f, 0.27f, 0.42f);
    public static readonly Color Disabled = new Color(0.2f, 0.22f, 0.28f);

    // Yazı
    public static readonly Color Text = new Color(0.95f, 0.96f, 1f);
    public static readonly Color TextMuted = new Color(0.58f, 0.63f, 0.76f);
    public static readonly Color TextDark = new Color(0.08f, 0.07f, 0.1f);

    // Nadirlik
    public static readonly Color Common = new Color(0.62f, 0.67f, 0.78f);
    public static readonly Color Rare = new Color(0.33f, 0.66f, 1f);
    public static readonly Color Epic = new Color(0.8f, 0.5f, 1f);

    public static Color RarityColor(PerkRarity rarity) {
        switch (rarity) {
            case PerkRarity.Rare: return Rare;
            case PerkRarity.Epic: return Epic;
            default: return Common;
        }
    }

    public static string RarityLabel(PerkRarity rarity) {
        switch (rarity) {
            case PerkRarity.Rare: return Loc.T("rarity_rare");
            case PerkRarity.Epic: return Loc.T("rarity_epic");
            default: return Loc.T("rarity_common");
        }
    }

    public static string Hex(Color color) => "#" + ColorUtility.ToHtmlStringRGB(color);
}
