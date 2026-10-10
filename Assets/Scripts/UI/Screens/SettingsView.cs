using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Ses, ekran sarsıntısı, hasar sayıları ve dil ayarları
public class SettingsView : UIScreen {

    private const float RowWidth = 640f;

    private RectTransform panel;
    private UIButton shakeToggle;
    private UIButton numbersToggle;
    private UIButton languageButton;
    private UIButton resetButton;
    private bool resetArmed;

    protected override void Build() {
        RectTransform content = CreateOverlay(UITheme.Overlay);

        Image panelImage = UIFactory.BorderedPanel("Panel", content, UITheme.Panel, UITheme.PanelBorder, 3f);
        panel = panelImage.rectTransform;
        panel.Center(Vector2.zero, new Vector2(760f, 720f));

        TextMeshProUGUI title = UIFactory.Text("Title", panel, Loc.T("settings"), 52f, UITheme.Text);
        title.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(600f, 64f));
        title.characterSpacing = 10f;

        SaveData save = SaveSystem.Data;
        float y = -130f;

        AddSliderRow(Loc.T("master_volume"), save.masterVolume, y, v => save.masterVolume = v);
        y -= 84f;
        AddSliderRow(Loc.T("sfx_volume"), save.sfxVolume, y, v => save.sfxVolume = v);
        y -= 84f;

        shakeToggle = AddButtonRow(Loc.T("screen_shake"), y, () => {
            save.screenShake = !save.screenShake;
            RefreshToggles();
        });
        y -= 84f;

        numbersToggle = AddButtonRow(Loc.T("damage_numbers"), y, () => {
            save.damageNumbers = !save.damageNumbers;
            RefreshToggles();
        });
        y -= 84f;

        languageButton = AddButtonRow(Loc.T("language"), y, () => {
            SaveSystem.Save();
            Loc.SetLanguage(Loc.Current == Language.Turkish ? Language.English : Language.Turkish);
        });

        resetButton = UIFactory.Button("Reset", panel, Loc.T("reset_progress"), new Color(0.45f, 0.13f, 0.17f), new Vector2(460f, 54f), OnResetPressed, 22f);
        resetButton.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 118f), new Vector2(460f, 54f));

        UIButton back = UIFactory.Button("Back", panel, Loc.T("back"), UITheme.Gold, new Vector2(300f, 66f), () => Manager.CloseSettings(), 28f);
        back.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 32f), new Vector2(300f, 66f));

        RefreshToggles();
    }

    private TextMeshProUGUI AddLabel(string label, float y) {
        TextMeshProUGUI text = UIFactory.Text("Label", panel, label, 26f, UITheme.Text, TextAlignmentOptions.Left, false);
        text.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0f, 0.5f), new Vector2(-RowWidth / 2f, y), new Vector2(300f, 40f));
        return text;
    }

    private void AddSliderRow(string label, float value, float y, Action<float> onChanged) {
        AddLabel(label, y);

        TextMeshProUGUI valueText = UIFactory.Text("Value", panel, Mathf.RoundToInt(value * 100f) + "%", 22f, UITheme.TextMuted, TextAlignmentOptions.Right);
        valueText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), new Vector2(RowWidth / 2f, y), new Vector2(80f, 36f));

        Slider slider = CreateSlider(panel, new Vector2(260f, 14f));
        slider.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), new Vector2(RowWidth / 2f - 100f, y), new Vector2(260f, 14f));
        slider.value = value;
        slider.onValueChanged.AddListener(v => {
            onChanged(v);
            valueText.text = Mathf.RoundToInt(v * 100f) + "%";
        });
    }

    private UIButton AddButtonRow(string label, float y, Action onClick) {
        AddLabel(label, y);

        UIButton button = UIFactory.Button("Toggle", panel, "", UITheme.Neutral, new Vector2(220f, 52f), onClick, 22f);
        button.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), new Vector2(RowWidth / 2f, y), new Vector2(220f, 52f));
        return button;
    }

    private static Slider CreateSlider(Transform parent, Vector2 size) {
        RectTransform root = UIFactory.Rect("Slider", parent);
        root.sizeDelta = size;

        Image background = UIFactory.Panel("Background", root, UITheme.BarBackground, 3f);
        background.rectTransform.Stretch();
        background.raycastTarget = true;

        RectTransform fillArea = UIFactory.Rect("FillArea", root);
        fillArea.Stretch();
        Image fill = UIFactory.Panel("Fill", fillArea, UITheme.Cyan, 3f);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;

        RectTransform handleArea = UIFactory.Rect("HandleArea", root);
        handleArea.Stretch();
        handleArea.offsetMin = new Vector2(12f, 0f);
        handleArea.offsetMax = new Vector2(-12f, 0f);
        Image handle = UIFactory.Image("Handle", handleArea, Color.white, UIFactory.Circle);
        handle.raycastTarget = true;
        handle.rectTransform.sizeDelta = new Vector2(28f, 14f);

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        return slider;
    }

    private void RefreshToggles() {
        SaveData save = SaveSystem.Data;
        SetToggle(shakeToggle, save.screenShake);
        SetToggle(numbersToggle, save.damageNumbers);

        languageButton.SetLabel(Loc.Current == Language.Turkish ? "TÜRKÇE" : "ENGLISH");
        languageButton.SetColor(UITheme.Neutral);
    }

    private static void SetToggle(UIButton button, bool on) {
        button.SetLabel(on ? Loc.T("on") : Loc.T("off"));
        button.SetColor(on ? UITheme.Success : UITheme.Disabled);
    }

    private void OnResetPressed() {
        // Yanlışlıkla silinmesin diye iki kez basmak gerekir
        if (!resetArmed) {
            resetArmed = true;
            resetButton.SetLabel(Loc.T("reset_confirm"));
            resetButton.SetColor(UITheme.Danger);
            return;
        }

        SaveSystem.ResetProgress();
        PlayerStats.Instance.EquipWeapon(SaveSystem.Data.equippedWeapon);
        resetArmed = false;
        resetButton.SetLabel(Loc.T("reset_progress"));
        resetButton.SetColor(new Color(0.45f, 0.13f, 0.17f));
        Manager.ShowToast(Loc.T("progress_reset"));
    }

    protected override void OnShow() {
        resetArmed = false;
        resetButton.SetLabel(Loc.T("reset_progress"));
        resetButton.SetColor(new Color(0.45f, 0.13f, 0.17f));
        RefreshToggles();
        UITweens.PopIn(panel);
    }

    protected override void OnHide() {
        SaveSystem.Save();
    }
}
