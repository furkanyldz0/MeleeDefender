using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Seviye atlayınca çıkan 3 kartlık güçlendirme seçimi
public class LevelUpView : UIScreen {

    private const float CardWidth = 360f;
    private const float CardHeight = 500f;
    private const float CardSpacing = 44f;

    private RectTransform titleBlock;
    private TextMeshProUGUI subtitle;
    private RectTransform cardRoot;
    private readonly List<GameObject> cards = new List<GameObject>();

    protected override void Build() {
        RectTransform content = CreateOverlay(new Color(0.03f, 0.04f, 0.08f, 0.72f));

        titleBlock = UIFactory.Rect("TitleBlock", content);
        titleBlock.Center(new Vector2(0f, 340f), new Vector2(1200f, 160f));

        Image glow = UIFactory.Image("Glow", titleBlock, new Color(UITheme.Xp.r, UITheme.Xp.g, UITheme.Xp.b, 0.25f), UIFactory.Glow);
        glow.rectTransform.Center(Vector2.zero, new Vector2(900f, 260f));

        TextMeshProUGUI title = UIFactory.Text("Title", titleBlock, Loc.T("level_up"), 78f, UITheme.Gold);
        title.rectTransform.Center(new Vector2(0f, 20f), new Vector2(1200f, 96f));
        title.characterSpacing = 10f;
        UIFactory.AddShadow(title, -1.5f, 0.5f);

        subtitle = UIFactory.Text("Subtitle", titleBlock, "", 26f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
        subtitle.rectTransform.Center(new Vector2(0f, -50f), new Vector2(1200f, 36f));
        subtitle.characterSpacing = 3f;

        cardRoot = UIFactory.Rect("Cards", content);
        cardRoot.Center(new Vector2(0f, -40f), new Vector2(3 * CardWidth + 2 * CardSpacing, CardHeight));
    }

    public void ShowChoices(int level, List<PerkDefinition> choices) {
        foreach (GameObject card in cards) {
            Destroy(card);
        }
        cards.Clear();

        subtitle.text = Loc.T("level") + " " + level + "   •   " + Loc.T("choose_perk");

        float totalWidth = choices.Count * CardWidth + (choices.Count - 1) * CardSpacing;
        for (int i = 0; i < choices.Count; i++) {
            float x = -totalWidth / 2f + CardWidth / 2f + i * (CardWidth + CardSpacing);
            GameObject card = BuildCard(choices[i], i, x);
            cards.Add(card);

            // Kartlar aşağıdan sırayla gelir
            RectTransform rt = (RectTransform)card.transform;
            Vector2 target = rt.anchoredPosition;
            rt.anchoredPosition = target + new Vector2(0f, -120f);
            CanvasGroup group = UIFactory.Group(rt);
            group.alpha = 0f;

            float delay = 0.08f + i * 0.08f;
            rt.DOAnchorPos(target, 0.4f).SetDelay(delay).SetEase(Ease.OutBack).SetUpdate(true).SetLink(card);
            group.DOFade(1f, 0.25f).SetDelay(delay).SetUpdate(true).SetLink(card);
        }

        if (!IsVisible) Show();
        UITweens.PopIn(titleBlock, 0.8f, 0.4f);
    }

    private GameObject BuildCard(PerkDefinition perk, int index, float x) {
        Color rarityColor = UITheme.RarityColor(perk.rarity);
        int currentStacks = PlayerStats.Instance.GetStacks(perk.type);

        UIButton button = UIFactory.Button("Card_" + perk.type, cardRoot, "", new Color(0.08f, 0.1f, 0.18f, 0.97f), new Vector2(CardWidth, CardHeight), () => RunProgression.Instance.ChoosePerk(perk));
        button.SetHoverScale(1.05f);
        RectTransform card = button.GetComponent<RectTransform>();
        card.Center(new Vector2(x, 0f), new Vector2(CardWidth, CardHeight));
        button.Label.gameObject.SetActive(false);

        // Nadirlik rengi kenarlık olarak
        Image border = UIFactory.Image("Border", card, rarityColor, UIFactory.RoundedOutline);
        border.type = Image.Type.Sliced;
        border.rectTransform.Stretch();

        Image glow = UIFactory.Image("Glow", card, new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.28f), UIFactory.Glow);
        glow.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(380f, 300f));

        Image iconRing = UIFactory.Image("IconRing", card, rarityColor, UIFactory.Circle);
        iconRing.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -112f), new Vector2(124f, 124f));
        Image iconBg = UIFactory.Image("IconBg", iconRing.transform, UITheme.PanelLight, UIFactory.Circle);
        iconBg.rectTransform.Stretch(4f);
        TextMeshProUGUI icon = UIFactory.Text("Icon", iconBg.transform, perk.icon, 54f, rarityColor);
        icon.rectTransform.Stretch();

        TextMeshProUGUI rarity = UIFactory.Text("Rarity", card, UITheme.RarityLabel(perk.rarity), 18f, rarityColor);
        rarity.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(320f, 26f));
        rarity.characterSpacing = 8f;

        TextMeshProUGUI name = UIFactory.Text("Name", card, perk.displayName.Get(), 34f, UITheme.Text);
        name.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -228f), new Vector2(330f, 46f));

        TextMeshProUGUI description = UIFactory.Text("Description", card, perk.GetDescription(), 23f, UITheme.TextMuted, TextAlignmentOptions.Top, false);
        description.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(300f, 110f));
        description.textWrappingMode = TextWrappingModes.Normal;

        if (perk.maxStacks < 99) {
            TextMeshProUGUI rank = UIFactory.Text("Rank", card, Loc.Format("stack", currentStacks + 1, perk.maxStacks), 18f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
            rank.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 92f), new Vector2(300f, 26f));
        }

        Image keyBg = UIFactory.Image("Key", card, UITheme.PanelLight, UIFactory.Circle);
        keyBg.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(48f, 48f));
        TextMeshProUGUI key = UIFactory.Text("KeyText", keyBg.transform, (index + 1).ToString(), 24f, rarityColor);
        key.rectTransform.Stretch();

        return card.gameObject;
    }
}
