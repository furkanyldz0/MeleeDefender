using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseView : UIScreen {

    private RectTransform panel;
    private TextMeshProUGUI runInfo;
    private TextMeshProUGUI perkList;

    protected override void Build() {
        RectTransform content = CreateOverlay(UITheme.Overlay);

        Image panelImage = UIFactory.BorderedPanel("Panel", content, UITheme.Panel, UITheme.PanelBorder, 3f);
        panel = panelImage.rectTransform;
        panel.Center(Vector2.zero, new Vector2(620f, 760f));

        TextMeshProUGUI title = UIFactory.Text("Title", panel, Loc.T("paused"), 54f, UITheme.Text);
        title.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(560f, 64f));
        title.characterSpacing = 10f;

        runInfo = UIFactory.Text("RunInfo", panel, "", 24f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
        runInfo.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(560f, 32f));

        float y = -168f;
        AddButton(Loc.T("resume"), UITheme.Gold, new Vector2(420f, 80f), y, () => GameFlow.Instance.TogglePause(), 34f);
        y -= 96f;
        AddButton(Loc.T("restart"), UITheme.Neutral, new Vector2(420f, 66f), y, () => GameFlow.Instance.RestartRun(), 26f);
        y -= 82f;
        AddButton(Loc.T("settings"), UITheme.Neutral, new Vector2(420f, 66f), y, () => Manager.OpenSettings(this), 26f);
        y -= 82f;
        AddButton(Loc.T("main_menu"), UITheme.Neutral, new Vector2(420f, 66f), y, () => GameFlow.Instance.ReturnToMenu(), 26f);

        TextMeshProUGUI perkTitle = UIFactory.Text("PerkTitle", panel, Loc.T("current_perks"), 22f, UITheme.TextMuted);
        perkTitle.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(560f, 30f));
        perkTitle.characterSpacing = 6f;

        perkList = UIFactory.Text("Perks", panel, "", 20f, UITheme.Text, TextAlignmentOptions.Top, false);
        perkList.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(540f, 112f));
        perkList.textWrappingMode = TextWrappingModes.Normal;
        perkList.lineSpacing = 10f;
    }

    private void AddButton(string label, Color color, Vector2 size, float y, System.Action onClick, float fontSize) {
        UIButton button = UIFactory.Button(label, panel, label, color, size, onClick, fontSize);
        button.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), size);
    }

    protected override void OnShow() {
        runInfo.text = Loc.T("wave") + " " + WaveManager.Instance.CurrentWave + "   •   " + Loc.T("score") + " " + LevelManager.Instance.Score
                     + "   •   " + Loc.T("level_short") + " " + RunProgression.Instance.Level;

        StringBuilder builder = new StringBuilder();
        foreach (PerkDefinition perk in PlayerStats.Instance.AcquiredPerks) {
            if (builder.Length > 0) builder.Append("    ");
            builder.Append("<color=").Append(UITheme.Hex(UITheme.RarityColor(perk.rarity))).Append(">")
                   .Append(perk.icon).Append("</color> ")
                   .Append(perk.displayName.Get())
                   .Append(" <color=").Append(UITheme.Hex(UITheme.TextMuted)).Append(">x").Append(PlayerStats.Instance.GetStacks(perk.type)).Append("</color>");
        }
        perkList.text = builder.Length > 0 ? builder.ToString() : "<color=" + UITheme.Hex(UITheme.TextMuted) + ">" + Loc.T("no_perks") + "</color>";

        UITweens.PopIn(panel);
    }
}
