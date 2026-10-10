using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Cephanelik: altınla silah açma/kuşanma ve kalıcı geliştirmeler.
public class ArmoryView : UIScreen {

    private class WeaponCard {
        public WeaponDefinition weapon;
        public Image border;
        public UIButton button;
    }

    private class UpgradeRow {
        public MetaUpgradeDefinition upgrade;
        public List<Image> pips = new List<Image>();
        public UIButton button;
    }

    private RectTransform weaponsPage;
    private RectTransform upgradesPage;
    private UIButton weaponsTab;
    private UIButton upgradesTab;
    private TextMeshProUGUI goldValue;
    private RectTransform goldPill;

    private readonly List<WeaponCard> weaponCards = new List<WeaponCard>();
    private readonly List<UpgradeRow> upgradeRows = new List<UpgradeRow>();

    protected override void Build() {
        RectTransform content = CreateOverlay(new Color(0.03f, 0.04f, 0.08f, 0.94f));

        TextMeshProUGUI title = UIFactory.Text("Title", content, Loc.T("armory"), 64f, UITheme.Text);
        title.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(900f, 80f));
        title.characterSpacing = 12f;
        UIFactory.AddShadow(title);

        UIButton back = UIFactory.Button("Back", content, "‹  " + Loc.T("back"), UITheme.Neutral, new Vector2(200f, 60f), () => Manager.CloseArmory(), 26f);
        back.GetComponent<RectTransform>().Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(200f, 60f));

        Image pill = UIFactory.BorderedPanel("GoldPill", content, UITheme.Panel, UITheme.GoldDark, 2f, 0.85f);
        goldPill = pill.rectTransform;
        goldPill.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(220f, 60f));
        RectTransform coin = UIFactory.CoinIcon(pill.transform, 36f);
        coin.Place(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(34f, 0f), new Vector2(36f, 36f));
        goldValue = UIFactory.Text("Gold", pill.transform, "0", 30f, UITheme.Gold, TextAlignmentOptions.Right);
        goldValue.rectTransform.Place(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(150f, 40f));

        weaponsTab = UIFactory.Button("WeaponsTab", content, Loc.T("tab_weapons"), UITheme.Gold, new Vector2(300f, 58f), () => SelectTab(true), 26f);
        weaponsTab.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-160f, -140f), new Vector2(300f, 58f));
        upgradesTab = UIFactory.Button("UpgradesTab", content, Loc.T("tab_upgrades"), UITheme.Neutral, new Vector2(300f, 58f), () => SelectTab(false), 26f);
        upgradesTab.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(160f, -140f), new Vector2(300f, 58f));

        weaponsPage = UIFactory.Rect("WeaponsPage", content);
        weaponsPage.Center(new Vector2(0f, -70f), new Vector2(1660f, 560f));
        BuildWeaponCards();

        upgradesPage = UIFactory.Rect("UpgradesPage", content);
        upgradesPage.Center(new Vector2(0f, -60f), new Vector2(1040f, 600f));
        BuildUpgradeRows();

        TextMeshProUGUI hint = UIFactory.Text("Hint", content, Loc.T("armory_hint"), 20f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
        hint.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(1200f, 30f));

        SelectTab(true);
    }

    #region Silahlar

    private void BuildWeaponCards() {
        List<WeaponDefinition> weapons = GameDatabase.Instance.weapons;
        const float cardWidth = 300f;
        const float spacing = 24f;
        float totalWidth = weapons.Count * cardWidth + (weapons.Count - 1) * spacing;

        for (int i = 0; i < weapons.Count; i++) {
            WeaponDefinition weapon = weapons[i];
            float x = -totalWidth / 2f + cardWidth / 2f + i * (cardWidth + spacing);

            Image cardPanel = UIFactory.BorderedPanel("Card_" + weapon.id, weaponsPage, UITheme.Panel, UITheme.PanelBorder, 3f);
            cardPanel.rectTransform.Center(new Vector2(x, 0f), new Vector2(cardWidth, 540f));
            Transform card = cardPanel.transform;
            Image border = UIFactory.GetBorder(cardPanel);

            Image topGlow = UIFactory.Image("TopGlow", card, new Color(weapon.trailColor.r, weapon.trailColor.g, weapon.trailColor.b, 0.18f), UIFactory.Glow);
            topGlow.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(340f, 260f));

            RectTransform icon = WeaponIcon.Build(card, weapon, 170f);
            icon.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -116f), new Vector2(170f, 170f));

            TextMeshProUGUI name = UIFactory.Text("Name", card, weapon.displayName.Get(), 30f, UITheme.Text);
            name.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -218f), new Vector2(280f, 40f));

            TextMeshProUGUI description = UIFactory.Text("Description", card, weapon.description.Get(), 18f, UITheme.TextMuted, TextAlignmentOptions.Top, false);
            description.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -262f), new Vector2(256f, 72f));
            description.textWrappingMode = TextWrappingModes.Normal;

            AddStatRow(card, Loc.T("stat_damage"), Mathf.InverseLerp(0.5f, 1.7f, weapon.damageMultiplier), -350f, weapon.trailColor);
            AddStatRow(card, Loc.T("stat_speed"), Mathf.InverseLerp(0.19f, 0.055f, weapon.swingDuration), -380f, weapon.trailColor);
            AddStatRow(card, Loc.T("stat_reach"), Mathf.InverseLerp(0.6f, 1.7f, weapon.hitboxScale.y), -410f, weapon.trailColor);

            TextMeshProUGUI special = UIFactory.Text("Special", card, GetSpecialText(weapon), 18f, weapon.trailColor);
            special.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -434f), new Vector2(270f, 26f));

            WeaponCard entry = new WeaponCard { weapon = weapon, border = border };
            entry.button = UIFactory.Button("Action", card, "", UITheme.Gold, new Vector2(252f, 56f), () => OnWeaponButton(entry), 24f);
            entry.button.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(252f, 56f));
            weaponCards.Add(entry);
        }
    }

    private static void AddStatRow(Transform card, string label, float value, float y, Color color) {
        TextMeshProUGUI text = UIFactory.Text("StatLabel", card, label, 17f, UITheme.TextMuted, TextAlignmentOptions.Left, false);
        text.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-70f, y), new Vector2(120f, 24f));

        UIBar bar = UIFactory.Bar("StatBar", card, new Vector2(140f, 10f), color);
        bar.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(60f, y - 7f), new Vector2(140f, 10f));
        bar.SetValue(Mathf.Clamp(value, 0.08f, 1f), true);
    }

    private static string GetSpecialText(WeaponDefinition weapon) {
        if (weapon.extraProjectiles > 0) return Loc.Format("special_split", weapon.extraProjectiles);
        if (weapon.pierce > 0) return Loc.Format("special_pierce", weapon.pierce);
        if (weapon.critChance >= 0.15f) return Loc.Format("special_crit", Mathf.RoundToInt(weapon.critChance * 100f));
        if (weapon.damageMultiplier >= 1.5f) return Loc.T("special_heavy");
        return Loc.T("special_balanced");
    }

    private void OnWeaponButton(WeaponCard card) {
        WeaponDefinition weapon = card.weapon;

        if (SaveSystem.IsWeaponUnlocked(weapon.id)) {
            PlayerStats.Instance.EquipWeapon(weapon.id);
            AudioManager.Instance.Play(Sfx.Purchase, 0.6f, 0f);
        }
        else if (SaveSystem.TrySpendGold(weapon.cost)) {
            SaveSystem.Data.unlockedWeapons.Add(weapon.id);
            PlayerStats.Instance.EquipWeapon(weapon.id);
            AudioManager.Instance.Play(Sfx.Purchase);
            UITweens.Pulse(card.border.transform.parent, 0.08f, 0.35f);
        }
        else {
            AudioManager.Instance.Play(Sfx.Error);
            Manager.ShowToast(Loc.T("not_enough_gold"));
            UITweens.Pulse(goldPill, 0.15f, 0.3f);
        }

        Refresh();
    }

    #endregion

    #region Geliştirmeler

    private void BuildUpgradeRows() {
        List<MetaUpgradeDefinition> upgrades = GameDatabase.Instance.upgrades;
        const float rowHeight = 100f;
        const float spacing = 14f;
        float totalHeight = upgrades.Count * rowHeight + (upgrades.Count - 1) * spacing;

        for (int i = 0; i < upgrades.Count; i++) {
            MetaUpgradeDefinition upgrade = upgrades[i];
            float y = totalHeight / 2f - rowHeight / 2f - i * (rowHeight + spacing);

            Image row = UIFactory.BorderedPanel("Upgrade_" + upgrade.id, upgradesPage, UITheme.Panel, UITheme.PanelBorder);
            row.rectTransform.Center(new Vector2(0f, y), new Vector2(1000f, rowHeight));

            Image iconBg = UIFactory.Image("IconBg", row.transform, UITheme.PanelLight, UIFactory.Circle);
            iconBg.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(58f, 0f), new Vector2(66f, 66f));
            TextMeshProUGUI icon = UIFactory.Text("Icon", iconBg.transform, upgrade.icon, 30f, UITheme.Gold);
            icon.rectTransform.Stretch();

            TextMeshProUGUI name = UIFactory.Text("Name", row.transform, upgrade.displayName.Get(), 28f, UITheme.Text, TextAlignmentOptions.Left);
            name.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(110f, 16f), new Vector2(420f, 36f));

            TextMeshProUGUI description = UIFactory.Text("Description", row.transform, upgrade.GetDescription(), 19f, UITheme.TextMuted, TextAlignmentOptions.Left, false);
            description.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(110f, -18f), new Vector2(460f, 28f));

            UpgradeRow entry = new UpgradeRow { upgrade = upgrade };
            for (int p = 0; p < upgrade.maxLevel; p++) {
                Image pip = UIFactory.Panel("Pip", row.transform, UITheme.Disabled, 3f);
                pip.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(580f + p * 34f, 0f), new Vector2(28f, 12f));
                entry.pips.Add(pip);
            }

            entry.button = UIFactory.Button("Buy", row.transform, "", UITheme.Gold, new Vector2(220f, 60f), () => OnUpgradeButton(entry), 24f);
            entry.button.GetComponent<RectTransform>().Place(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(220f, 60f));
            upgradeRows.Add(entry);
        }
    }

    private void OnUpgradeButton(UpgradeRow row) {
        int level = SaveSystem.GetUpgradeLevel(row.upgrade.id);
        if (level >= row.upgrade.maxLevel) return;

        if (SaveSystem.TrySpendGold(row.upgrade.GetCost(level))) {
            SaveSystem.SetUpgradeLevel(row.upgrade.id, level + 1);
            SaveSystem.Save();
            AudioManager.Instance.Play(Sfx.Purchase);
            UITweens.Pulse(row.pips[level].transform, 0.6f, 0.35f);
        }
        else {
            AudioManager.Instance.Play(Sfx.Error);
            Manager.ShowToast(Loc.T("not_enough_gold"));
            UITweens.Pulse(goldPill, 0.15f, 0.3f);
        }

        Refresh();
    }

    #endregion

    private void SelectTab(bool weapons) {
        weaponsPage.gameObject.SetActive(weapons);
        upgradesPage.gameObject.SetActive(!weapons);
        weaponsTab.SetColor(weapons ? UITheme.Gold : UITheme.Neutral);
        upgradesTab.SetColor(weapons ? UITheme.Neutral : UITheme.Gold);

        UITweens.PopIn(weapons ? weaponsPage : upgradesPage, 0.96f, 0.25f);
    }

    protected override void OnShow() {
        Refresh();
    }

    public void Refresh() {
        int gold = SaveSystem.Data.gold;
        goldValue.text = gold.ToString();
        string equipped = PlayerStats.Instance.Weapon.id;

        foreach (WeaponCard card in weaponCards) {
            bool unlocked = SaveSystem.IsWeaponUnlocked(card.weapon.id);
            bool isEquipped = card.weapon.id == equipped;

            if (isEquipped) {
                card.button.SetLabel(Loc.T("equipped_btn"));
                card.button.SetColor(UITheme.Success);
                card.button.SetInteractable(false);
                card.border.color = UITheme.Gold;
            }
            else if (unlocked) {
                card.button.SetLabel(Loc.T("equip"));
                card.button.SetColor(UITheme.Cyan);
                card.button.SetInteractable(true);
                card.border.color = UITheme.PanelBorder;
            }
            else {
                card.button.SetLabel(Loc.T("buy") + "  " + card.weapon.cost);
                card.button.SetColor(gold >= card.weapon.cost ? UITheme.Gold : UITheme.Disabled);
                card.button.SetInteractable(true);
                card.border.color = UITheme.PanelBorder;
            }
        }

        foreach (UpgradeRow row in upgradeRows) {
            int level = SaveSystem.GetUpgradeLevel(row.upgrade.id);
            for (int i = 0; i < row.pips.Count; i++) {
                row.pips[i].color = i < level ? UITheme.Gold : UITheme.Disabled;
            }

            if (level >= row.upgrade.maxLevel) {
                row.button.SetLabel(Loc.T("maxed"));
                row.button.SetColor(UITheme.Success);
                row.button.SetInteractable(false);
            }
            else {
                int cost = row.upgrade.GetCost(level);
                row.button.SetLabel(cost + " " + Loc.T("gold"));
                row.button.SetColor(gold >= cost ? UITheme.Gold : UITheme.Disabled);
                row.button.SetInteractable(true);
            }
        }
    }
}
