using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Ana menü: arka planda arena canlı olarak görünür.
public class MainMenuView : UIScreen {

    private RectTransform titleBlock;
    private TextMeshProUGUI goldValue;
    private TextMeshProUGUI statsValues;
    private readonly List<Transform> animatedButtons = new List<Transform>();

    protected override void Build() {
        Image overlay = UIFactory.Image("Overlay", Root, new Color(0.03f, 0.04f, 0.08f, 0.55f));
        overlay.rectTransform.Stretch();
        overlay.raycastTarget = true;

        BuildTitle();
        BuildButtons();
        BuildStatsPanel();
        BuildCorners();
    }

    private void BuildTitle() {
        titleBlock = UIFactory.Rect("TitleBlock", Root);
        titleBlock.Center(new Vector2(0f, 320f), new Vector2(1500f, 300f));

        Image glow = UIFactory.Image("Glow", titleBlock, new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.16f), UIFactory.Glow);
        glow.rectTransform.Center(new Vector2(0f, 20f), new Vector2(1300f, 420f));

        TextMeshProUGUI title = UIFactory.Text("Title", titleBlock, "MELEE <color=" + UITheme.Hex(UITheme.Gold) + ">DEFENDER</color>", 116f, UITheme.Text);
        title.rectTransform.Center(new Vector2(0f, 30f), new Vector2(1500f, 150f));
        title.characterSpacing = 5f;
        UIFactory.AddShadow(title, -2f, 0.5f);

        Image line = UIFactory.Image("Divider", titleBlock, new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.6f));
        line.rectTransform.Center(new Vector2(0f, -54f), new Vector2(620f, 2f));
        Image diamond = UIFactory.Image("Diamond", titleBlock, UITheme.Gold);
        diamond.rectTransform.Center(new Vector2(0f, -54f), new Vector2(14f, 14f));
        diamond.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

        TextMeshProUGUI tagline = UIFactory.Text("Tagline", titleBlock, Loc.T("tagline"), 30f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
        tagline.rectTransform.Center(new Vector2(0f, -96f), new Vector2(1200f, 44f));
        tagline.characterSpacing = 6f;
    }

    private void BuildButtons() {
        RectTransform column = UIFactory.Rect("Buttons", Root);
        column.Center(new Vector2(0f, -50f), new Vector2(460f, 460f));

        UIButton play = UIFactory.Button("Play", column, Loc.T("play"), UITheme.Gold, new Vector2(440f, 92f), () => GameFlow.Instance.StartRun(), 42f);
        play.GetComponent<RectTransform>().Center(new Vector2(0f, 186f), new Vector2(440f, 92f));
        animatedButtons.Add(play.transform);

        UIButton leaderboard = UIFactory.Button("Leaderboard", column, Loc.T("leaderboard"), UITheme.Neutral, new Vector2(440f, 72f), () => Manager.OpenLeaderboard());
        leaderboard.GetComponent<RectTransform>().Center(new Vector2(0f, 82f), new Vector2(440f, 72f));
        animatedButtons.Add(leaderboard.transform);

        UIButton armory = UIFactory.Button("Armory", column, Loc.T("armory"), UITheme.Neutral, new Vector2(440f, 72f), () => Manager.OpenArmory());
        armory.GetComponent<RectTransform>().Center(new Vector2(0f, -2f), new Vector2(440f, 72f));
        animatedButtons.Add(armory.transform);

        UIButton settings = UIFactory.Button("Settings", column, Loc.T("settings"), UITheme.Neutral, new Vector2(440f, 72f), () => Manager.OpenSettings(this));
        settings.GetComponent<RectTransform>().Center(new Vector2(0f, -86f), new Vector2(440f, 72f));
        animatedButtons.Add(settings.transform);

#if !UNITY_WEBGL
        UIButton quit = UIFactory.Button("Quit", column, Loc.T("quit"), UITheme.Neutral, new Vector2(440f, 72f), () => GameFlow.Instance.QuitGame());
        quit.GetComponent<RectTransform>().Center(new Vector2(0f, -170f), new Vector2(440f, 72f));
        animatedButtons.Add(quit.transform);
#endif
    }

    private void BuildStatsPanel() {
        Image panel = UIFactory.BorderedPanel("Stats", Root, UITheme.Panel, UITheme.PanelBorder);
        panel.rectTransform.Place(Vector2.zero, Vector2.zero, new Vector2(32f, 32f), new Vector2(420f, 206f));

        string labels = Loc.T("best_wave") + "\n" + Loc.T("best_score") + "\n" + Loc.T("total_kills") + "\n" + Loc.T("bosses_defeated") + "\n" + Loc.T("weapon_label");
        TextMeshProUGUI labelText = UIFactory.Text("Labels", panel.transform, labels, 22f, UITheme.TextMuted, TextAlignmentOptions.TopLeft, false);
        labelText.rectTransform.Stretch(22f);
        labelText.lineSpacing = 18f;

        statsValues = UIFactory.Text("Values", panel.transform, "", 22f, UITheme.Text, TextAlignmentOptions.TopRight);
        statsValues.rectTransform.Stretch(22f);
        statsValues.lineSpacing = 18f;
    }

    private void BuildCorners() {
        Image pill = UIFactory.BorderedPanel("GoldPill", Root, UITheme.Panel, UITheme.GoldDark, 2f, 0.85f);
        pill.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -28f), new Vector2(220f, 60f));
        RectTransform coin = UIFactory.CoinIcon(pill.transform, 36f);
        coin.Place(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(34f, 0f), new Vector2(36f, 36f));
        goldValue = UIFactory.Text("Gold", pill.transform, "0", 30f, UITheme.Gold, TextAlignmentOptions.Right);
        goldValue.rectTransform.Place(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(150f, 40f));

        TextMeshProUGUI version = UIFactory.Text("Version", Root, "v" + Application.version, 18f, UITheme.TextMuted, TextAlignmentOptions.Right, false);
        version.rectTransform.Place(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-32f, 28f), new Vector2(300f, 26f));

        TextMeshProUGUI controls = UIFactory.Text("Controls", Root, Loc.T("controls_hint"), 19f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
        controls.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1100f, 28f));
    }

    protected override void OnShow() {
        Refresh();

        UITweens.PopIn(titleBlock, 0.9f, 0.5f);
        for (int i = 0; i < animatedButtons.Count; i++) {
            Transform button = animatedButtons[i];
            button.DOKill();
            button.localScale = Vector3.one * 0.85f;
            button.DOScale(1f, 0.35f).SetDelay(0.06f * i).SetEase(Ease.OutBack).SetUpdate(true).SetLink(button.gameObject);
        }
    }

    public void Refresh() {
        SaveData save = SaveSystem.Data;
        goldValue.text = save.gold.ToString();
        statsValues.text = save.bestWave + "\n" + save.bestScore + "\n" + save.totalKills + "\n" + save.bossesDefeated
                         + "\n" + PlayerStats.Instance.Weapon.displayName.Get();
    }
}
