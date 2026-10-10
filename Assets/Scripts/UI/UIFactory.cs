using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Arayüz elemanlarını kodla oluşturmak için yardımcılar.
// Köşeleri yuvarlatılmış paneller, daireler ve parıltı efektleri için gereken sprite'lar da burada üretilir.
public static class UIFactory {

    private static Sprite roundedSprite;
    private static Sprite outlineSprite;
    private static Sprite circleSprite;
    private static Sprite glowSprite;
    private static TMP_FontAsset defaultFont;
    private static bool fontLookupFailed;

    private const int RoundedTextureSize = 64;
    private const int RoundedRadius = 24;

    #region Sprite üretimi

    public static Sprite Rounded {
        get {
            if (roundedSprite == null) {
                Texture2D texture = CreateTexture(RoundedTextureSize, RoundedTextureSize, (x, y) => {
                    float half = RoundedTextureSize / 2f;
                    float dx = Mathf.Max(Mathf.Abs(x - half) - (half - RoundedRadius), 0f);
                    float dy = Mathf.Max(Mathf.Abs(y - half) - (half - RoundedRadius), 0f);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) - RoundedRadius;
                    return Mathf.Clamp01(0.5f - distance);
                });
                float border = RoundedRadius + 1;
                roundedSprite = Sprite.Create(texture, new Rect(0, 0, RoundedTextureSize, RoundedTextureSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
                roundedSprite.name = "UI_Rounded";
            }
            return roundedSprite;
        }
    }

    // İçi boş yuvarlak çerçeve (kenarlıklar için; dolgu yarı saydam olsa bile kenarlık rengi içeri sızmaz)
    public static Sprite RoundedOutline {
        get {
            if (outlineSprite == null) {
                const float thickness = 3f;
                Texture2D texture = CreateTexture(RoundedTextureSize, RoundedTextureSize, (x, y) => {
                    float half = RoundedTextureSize / 2f;
                    float dx = Mathf.Max(Mathf.Abs(x - half) - (half - RoundedRadius), 0f);
                    float dy = Mathf.Max(Mathf.Abs(y - half) - (half - RoundedRadius), 0f);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) - RoundedRadius;
                    float outer = Mathf.Clamp01(0.5f - distance);
                    float inner = Mathf.Clamp01(0.5f - (distance + thickness));
                    return outer - inner;
                });
                float border = RoundedRadius + 1;
                outlineSprite = Sprite.Create(texture, new Rect(0, 0, RoundedTextureSize, RoundedTextureSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
                outlineSprite.name = "UI_RoundedOutline";
            }
            return outlineSprite;
        }
    }

    public static Sprite Circle {
        get {
            if (circleSprite == null) {
                const int size = 128;
                Texture2D texture = CreateTexture(size, size, (x, y) => {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f)) - (size / 2f - 1f);
                    return Mathf.Clamp01(0.5f - distance);
                });
                circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
                circleSprite.name = "UI_Circle";
            }
            return circleSprite;
        }
    }

    public static Sprite Glow {
        get {
            if (glowSprite == null) {
                const int size = 128;
                Texture2D texture = CreateTexture(size, size, (x, y) => {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                    float a = Mathf.Clamp01(1f - distance);
                    return a * a;
                });
                glowSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
                glowSprite.name = "UI_Glow";
            }
            return glowSprite;
        }
    }

    private static Texture2D CreateTexture(int width, int height, Func<float, float, float> alpha) {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                byte a = (byte)Mathf.RoundToInt(alpha(x + 0.5f, y + 0.5f) * 255f);
                pixels[y * width + x] = new Color32(255, 255, 255, a);
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    #endregion

    #region Font

    // TMP varsayılan fontu projede yoksa (TMP Essential Resources içe aktarılmamışsa)
    // yazılar görünmez olmasın diye sistem fontundan çalışma anında bir font üretilir.
    public static TMP_FontAsset DefaultFont {
        get {
            if (defaultFont != null) return defaultFont;
            if (fontLookupFailed) return null;

            try {
                defaultFont = TMP_Settings.defaultFontAsset;
            }
            catch (Exception) {
                defaultFont = null; // TMP Settings yok
            }

            if (defaultFont == null) {
                defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

            // Son çare: işletim sistemindeki yaygın bir font dosyasından dinamik font (WebGL'de çalışmaz)
            string[] systemFontFiles = {
                "/System/Library/Fonts/Supplemental/Arial.ttf",
                "/Library/Fonts/Arial.ttf",
                "C:/Windows/Fonts/arial.ttf",
                "C:/Windows/Fonts/segoeui.ttf",
                "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
            };
            for (int i = 0; i < systemFontFiles.Length && defaultFont == null; i++) {
                if (!System.IO.File.Exists(systemFontFiles[i])) continue;
                try {
                    defaultFont = TMP_FontAsset.CreateFontAsset(systemFontFiles[i], 0, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
                }
                catch (Exception) {
                    defaultFont = null;
                }
            }

            if (defaultFont == null) {
                fontLookupFailed = true;
                Debug.LogWarning("TextMeshPro fontu bulunamadı. Window > TextMeshPro > Import TMP Essential Resources ile içe aktarın.");
            }

            return defaultFont;
        }
    }

    #endregion

    #region Yerleşim

    public static RectTransform Rect(string name, Transform parent) {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    // anchor: ebeveyndeki tutunma noktası (0-1), pivot: kendi merkez noktası
    public static RectTransform Place(this RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size) {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return rt;
    }

    public static RectTransform Center(this RectTransform rt, Vector2 position, Vector2 size) {
        return rt.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
    }

    public static RectTransform Stretch(this RectTransform rt, float inset = 0f) {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
        return rt;
    }

    #endregion

    #region Elemanlar

    public static Image Image(string name, Transform parent, Color color, Sprite sprite = null) {
        RectTransform rt = Rect(name, parent);
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    // Köşeleri yuvarlatılmış panel. cornerScale büyüdükçe köşeler küçülür.
    public static Image Panel(string name, Transform parent, Color color, float cornerScale = 1f) {
        Image image = Image(name, parent, color, Rounded);
        image.type = UnityEngine.UI.Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = cornerScale;
        return image;
    }

    // Kenarlıklı panel: dolgu + üstünde içi boş çerçeve. Dönen Image dolgudur;
    // kenarlık rengini değiştirmek için GetBorder kullan.
    public static Image BorderedPanel(string name, Transform parent, Color fill, Color border, float borderWidth = 2f, float cornerScale = 1f) {
        Image background = Panel(name, parent, fill, cornerScale);

        Image outline = Image(BorderName, background.transform, border, RoundedOutline);
        outline.type = UnityEngine.UI.Image.Type.Sliced;
        outline.pixelsPerUnitMultiplier = cornerScale; // köşe yarıçapı dolguyla aynı kalsın
        outline.rectTransform.Stretch();
        outline.color = new Color(border.r, border.g, border.b, border.a * Mathf.Clamp01(borderWidth / 2f));

        return background;
    }

    public static Image GetBorder(Image borderedPanel) {
        Transform border = borderedPanel.transform.Find(BorderName);
        return border != null ? border.GetComponent<Image>() : borderedPanel;
    }

    private const string BorderName = "Border";

    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center, bool bold = true) {
        RectTransform rt = Rect(name, parent);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (tmp.font == null && DefaultFont != null) {
            tmp.font = DefaultFont;
        }
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.richText = true;
        return tmp;
    }

    // Başlıklar için yumuşak gölge (materyal kopyası oluşturur, çok sayıda yazıda kullanma)
    public static void AddShadow(TMP_Text text, float offset = -1.2f, float softness = 0.4f) {
        if (text.font == null || text.fontSharedMaterial == null) return;

        Material material = text.fontMaterial;
        material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.6f));
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, offset);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, softness);
        text.fontMaterial = material;
    }

    public static UIButton Button(string name, Transform parent, string label, Color color, Vector2 size, Action onClick, float fontSize = 30f) {
        Image background = Panel(name, parent, color);
        background.raycastTarget = true;
        background.rectTransform.sizeDelta = size;

        TextMeshProUGUI text = Text("Label", background.transform, label, fontSize, IsLight(color) ? UITheme.TextDark : UITheme.Text);
        text.rectTransform.Stretch(8f);
        text.characterSpacing = 4f;

        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);
        colors.colorMultiplier = 1.2f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        UIButton uiButton = background.gameObject.AddComponent<UIButton>();
        uiButton.Setup(button, background, text, onClick);
        return uiButton;
    }

    // Tek satırlık yazı kutusu (oyuncu adı vb.)
    public static TMP_InputField InputField(string name, Transform parent, Vector2 size, string placeholderText, int characterLimit, float fontSize = 24f) {
        Image background = Panel(name, parent, UITheme.BarBackground, 1.5f);
        background.raycastTarget = true;
        background.rectTransform.sizeDelta = size;

        // Bileşenler bağlanana kadar pasif tut (TMP_InputField OnEnable'da metin bileşenini arar)
        GameObject go = background.gameObject;
        go.SetActive(false);

        RectTransform area = Rect("TextArea", background.transform);
        area.Stretch();
        area.offsetMin = new Vector2(16f, 6f);
        area.offsetMax = new Vector2(-16f, -6f);
        area.gameObject.AddComponent<RectMask2D>();

        TextMeshProUGUI placeholder = Text("Placeholder", area, placeholderText, fontSize, UITheme.TextMuted, TextAlignmentOptions.Left, false);
        placeholder.rectTransform.Stretch();
        placeholder.fontStyle = FontStyles.Italic;

        TextMeshProUGUI text = Text("Text", area, "", fontSize, UITheme.Text, TextAlignmentOptions.Left, false);
        text.rectTransform.Stretch();
        text.richText = false;

        TMP_InputField input = go.AddComponent<TMP_InputField>();
        input.textViewport = area;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.targetGraphic = background;
        input.characterLimit = characterLimit;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.richText = false;
        input.caretColor = UITheme.Gold;
        input.selectionColor = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.35f);

        go.SetActive(true);
        return input;
    }

    public static UIBar Bar(string name, Transform parent, Vector2 size, Color fill, Color? background = null) {
        Image bg = Panel(name, parent, background ?? UITheme.BarBackground, 3f);
        bg.rectTransform.sizeDelta = size;

        Image trail = Panel("Trail", bg.transform, Color.white, 3f);
        trail.rectTransform.Stretch(2f);

        Image fillImage = Panel("Fill", bg.transform, fill, 3f);
        fillImage.rectTransform.Stretch(2f);

        UIBar bar = bg.gameObject.AddComponent<UIBar>();
        bar.Setup(fillImage, trail);
        return bar;
    }

    public static CanvasGroup Group(Component target) {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null) group = target.gameObject.AddComponent<CanvasGroup>();
        return group;
    }

    // Yarı saydam altın sikke simgesi
    public static RectTransform CoinIcon(Transform parent, float size) {
        Image outer = Image("Coin", parent, UITheme.Gold, Circle);
        outer.rectTransform.sizeDelta = new Vector2(size, size);
        Image inner = Image("Inner", outer.transform, UITheme.GoldDark, Circle);
        inner.rectTransform.Stretch(size * 0.18f);
        Image shine = Image("Shine", outer.transform, new Color(1f, 0.92f, 0.65f), Circle);
        shine.rectTransform.Center(new Vector2(-size * 0.12f, size * 0.12f), Vector2.one * size * 0.22f);
        return outer.rectTransform;
    }

    #endregion

    private static bool IsLight(Color color) {
        return color.r * 0.299f + color.g * 0.587f + color.b * 0.114f > 0.6f;
    }
}
