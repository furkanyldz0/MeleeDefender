using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Oyun sırasında görünen bilgiler.
// Arena ekranın ortasını kapladığı için paneller sol ve sağ boşluklara yerleştirildi;
// üst ortada sadece boss çubuğu (boss dalgalarında) görünür.
public class HUDView : UIScreen {

    private const int MaxPerkChips = 11;

    private TextMeshProUGUI waveTitle;
    private TextMeshProUGUI waveSubtitle;

    private TextMeshProUGUI scoreValue;
    private TextMeshProUGUI bestValue;

    private RectTransform goldPill;
    private TextMeshProUGUI goldValue;

    private UIBar castleBar;
    private TextMeshProUGUI castleValue;

    private TextMeshProUGUI weaponName;
    private Image weaponStripe;

    private TextMeshProUGUI levelValue;
    private RectTransform levelBadge;
    private UIBar xpBar;
    private TextMeshProUGUI xpValue;
    private RectTransform perkRow;
    private readonly Dictionary<PerkType, TextMeshProUGUI> perkChipCounts = new Dictionary<PerkType, TextMeshProUGUI>();

    private CanvasGroup bossGroup;
    private TextMeshProUGUI bossName;
    private TextMeshProUGUI bossEnraged;
    private UIBar bossBar;
    private Boss currentBoss;

    private CanvasGroup skillHint;
    private Tween skillHintTween;

    private CanvasGroup tutorialHint;
    private TextMeshProUGUI tutorialText;
    private int tutorialParries;

    private int displayedGold;
    private int targetGold;
    private Tween goldTween;

    public RectTransform GoldTarget => goldPill;

    protected override void Build() {
        BuildWavePanel();
        BuildScorePanel();
        BuildCastlePanel();
        BuildLevelPanel();
        BuildBossBar();
        BuildSkillHint();
        BuildTutorialHint();
    }

    #region Kurulum

    private void BuildWavePanel() {
        Image panel = UIFactory.BorderedPanel("WavePanel", Root, UITheme.Panel, UITheme.PanelBorder);
        panel.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -28f), new Vector2(340f, 100f));

        Image stripe = UIFactory.Panel("Stripe", panel.transform, UITheme.Gold, 4f);
        stripe.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(6f, 64f));

        waveTitle = UIFactory.Text("WaveTitle", panel.transform, "", 40f, UITheme.Text, TextAlignmentOptions.Left);
        waveTitle.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -10f), new Vector2(290f, 50f));
        waveTitle.characterSpacing = 3f;

        waveSubtitle = UIFactory.Text("WaveSubtitle", panel.transform, "", 22f, UITheme.TextMuted, TextAlignmentOptions.Left, false);
        waveSubtitle.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -60f), new Vector2(290f, 30f));
    }

    private void BuildScorePanel() {
        Image panel = UIFactory.BorderedPanel("ScorePanel", Root, UITheme.Panel, UITheme.PanelBorder);
        panel.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -28f), new Vector2(300f, 116f));

        TextMeshProUGUI label = UIFactory.Text("Label", panel.transform, Loc.T("score"), 20f, UITheme.TextMuted, TextAlignmentOptions.Right);
        label.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -10f), new Vector2(260f, 26f));
        label.characterSpacing = 6f;

        scoreValue = UIFactory.Text("Score", panel.transform, "0", 46f, UITheme.Text, TextAlignmentOptions.Right);
        scoreValue.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -32f), new Vector2(260f, 50f));

        bestValue = UIFactory.Text("Best", panel.transform, "", 18f, UITheme.TextMuted, TextAlignmentOptions.Right, false);
        bestValue.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -84f), new Vector2(260f, 24f));

        Image pill = UIFactory.BorderedPanel("GoldPill", Root, UITheme.Panel, UITheme.GoldDark, 2f, 0.85f);
        goldPill = pill.rectTransform;
        goldPill.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -158f), new Vector2(200f, 56f));

        RectTransform coin = UIFactory.CoinIcon(pill.transform, 34f);
        coin.Place(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(32f, 0f), new Vector2(34f, 34f));

        goldValue = UIFactory.Text("Gold", pill.transform, "0", 30f, UITheme.Gold, TextAlignmentOptions.Right);
        goldValue.rectTransform.Place(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(130f, 40f));
    }

    private void BuildCastlePanel() {
        Image panel = UIFactory.BorderedPanel("CastlePanel", Root, UITheme.Panel, UITheme.PanelBorder);
        panel.rectTransform.Place(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-32f, 28f), new Vector2(380f, 96f));

        TextMeshProUGUI label = UIFactory.Text("Label", panel.transform, Loc.T("base"), 22f, UITheme.TextMuted, TextAlignmentOptions.Left);
        label.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -12f), new Vector2(200f, 28f));
        label.characterSpacing = 6f;

        castleValue = UIFactory.Text("Value", panel.transform, "", 22f, UITheme.Text, TextAlignmentOptions.Right);
        castleValue.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -12f), new Vector2(200f, 28f));

        castleBar = UIFactory.Bar("CastleBar", panel.transform, new Vector2(340f, 24f), UITheme.Success);
        castleBar.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(340f, 24f));

        Image chip = UIFactory.BorderedPanel("WeaponChip", Root, UITheme.Panel, UITheme.PanelBorder);
        chip.rectTransform.Place(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-32f, 136f), new Vector2(380f, 54f));

        weaponStripe = UIFactory.Panel("Stripe", chip.transform, UITheme.Cyan, 4f);
        weaponStripe.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(6f, 30f));

        TextMeshProUGUI weaponLabel = UIFactory.Text("Label", chip.transform, Loc.T("weapon"), 18f, UITheme.TextMuted, TextAlignmentOptions.Left);
        weaponLabel.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(120f, 30f));
        weaponLabel.characterSpacing = 6f;

        weaponName = UIFactory.Text("Name", chip.transform, "", 26f, UITheme.Text, TextAlignmentOptions.Right);
        weaponName.rectTransform.Place(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(230f, 34f));
    }

    private void BuildLevelPanel() {
        Image panel = UIFactory.BorderedPanel("LevelPanel", Root, UITheme.Panel, UITheme.PanelBorder);
        panel.rectTransform.Place(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(32f, 28f), new Vector2(380f, 132f));

        Image badgeGlow = UIFactory.Image("BadgeGlow", panel.transform, new Color(UITheme.Xp.r, UITheme.Xp.g, UITheme.Xp.b, 0.35f), UIFactory.Glow);
        badgeGlow.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(50f, -48f), new Vector2(110f, 110f));

        Image badge = UIFactory.Image("Badge", panel.transform, UITheme.Xp, UIFactory.Circle);
        levelBadge = badge.rectTransform;
        levelBadge.Place(new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(50f, -48f), new Vector2(68f, 68f));

        levelValue = UIFactory.Text("Level", badge.transform, "1", 34f, UITheme.TextDark);
        levelValue.rectTransform.Stretch();

        TextMeshProUGUI label = UIFactory.Text("Label", panel.transform, Loc.T("level"), 20f, UITheme.TextMuted, TextAlignmentOptions.Left);
        label.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, -16f), new Vector2(140f, 26f));
        label.characterSpacing = 6f;

        xpValue = UIFactory.Text("Xp", panel.transform, "", 18f, UITheme.TextMuted, TextAlignmentOptions.Right, false);
        xpValue.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -18f), new Vector2(140f, 24f));

        xpBar = UIFactory.Bar("XpBar", panel.transform, new Vector2(262f, 16f), UITheme.Xp);
        xpBar.GetComponent<RectTransform>().Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, -50f), new Vector2(262f, 16f));

        perkRow = UIFactory.Rect("Perks", panel.transform);
        perkRow.Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -92f), new Vector2(348f, 30f));
        HorizontalLayoutGroup layout = perkRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void BuildBossBar() {
        Image panel = UIFactory.BorderedPanel("BossPanel", Root, new Color(0.08f, 0.04f, 0.07f, 0.9f), new Color(0.55f, 0.16f, 0.22f), 2f);
        panel.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(700f, 60f));
        bossGroup = UIFactory.Group(panel);

        bossName = UIFactory.Text("Name", panel.transform, "", 24f, UITheme.Text, TextAlignmentOptions.Left);
        bossName.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -4f), new Vector2(480f, 30f));
        bossName.characterSpacing = 3f;

        bossEnraged = UIFactory.Text("Enraged", panel.transform, Loc.T("boss_enraged"), 20f, UITheme.Danger, TextAlignmentOptions.Right);
        bossEnraged.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -5f), new Vector2(200f, 28f));

        bossBar = UIFactory.Bar("BossBar", panel.transform, new Vector2(664f, 16f), UITheme.Danger);
        bossBar.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(664f, 16f));

        panel.gameObject.SetActive(false);
    }

    private void BuildSkillHint() {
        Image pill = UIFactory.BorderedPanel("SkillHint", Root, new Color(0.16f, 0.11f, 0.04f, 0.92f), UITheme.Gold, 2f, 0.85f);
        pill.rectTransform.Place(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(32f, 172f), new Vector2(380f, 46f));

        TextMeshProUGUI text = UIFactory.Text("Text", pill.transform, Loc.T("skill_ready"), 22f, UITheme.Gold);
        text.rectTransform.Stretch();
        text.characterSpacing = 3f;

        skillHint = UIFactory.Group(pill);
        skillHint.alpha = 0f;
    }

    // İlk oyunlarda temel mekaniği anlatan ipucu (birkaç savuşturmadan sonra kaybolur)
    private void BuildTutorialHint() {
        Image pill = UIFactory.BorderedPanel("TutorialHint", Root, new Color(0.05f, 0.08f, 0.14f, 0.9f), UITheme.Cyan, 2f, 0.85f);
        pill.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), new Vector2(1000f, 56f));

        tutorialText = UIFactory.Text("Text", pill.transform, Loc.T("tutorial_parry"), 24f, UITheme.Text, TextAlignmentOptions.Center, false);
        tutorialText.rectTransform.Stretch();

        tutorialHint = UIFactory.Group(pill);
        tutorialHint.alpha = 0f;
    }

    #endregion

    #region Bağlantılar

    // UIManager tüm sistemler hazır olduktan sonra çağırır
    public void Bind() {
        LevelManager.Instance.OnScoreChanged += LevelManager_OnScoreChanged;
        RunProgression.Instance.OnXpChanged += RunProgression_OnXpChanged;
        PlayerStats.Instance.OnPerkAdded += PlayerStats_OnPerkAdded;
        PlayerStats.Instance.OnStatsChanged += RefreshWeapon;
        Base.Instance.OnHealthValuesChanged += Base_OnHealthValuesChanged;
        WaveManager.Instance.OnWaveAnnounced += WaveManager_OnWaveAnnounced;
        WaveManager.Instance.OnBossSpawned += WaveManager_OnBossSpawned;
        Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
        Manager.OnSkillProgressChanged += Manager_OnSkillProgressChanged;
        GameFlow.Instance.OnRunStarted += GameFlow_OnRunStarted;
        Player.Instance.GetMelee().OnParried += Melee_OnParried;
        EnemyShield.OnAnyShieldSpawned += EnemyShield_OnAnyShieldSpawned;

        LevelManager_OnScoreChanged(LevelManager.Instance.Score);
        RunProgression_OnXpChanged(RunProgression.Instance.Xp, RunProgression.Instance.XpToNext, RunProgression.Instance.Level);
        Base_OnHealthValuesChanged(Base.Instance.CurrentHealth, Base.Instance.Health);
        RefreshWeapon();
        RefreshWave();
        SetGold(0, true);
    }

    private void OnDestroy() {
        Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;

        if (LevelManager.Instance != null) LevelManager.Instance.OnScoreChanged -= LevelManager_OnScoreChanged;
        if (RunProgression.Instance != null) RunProgression.Instance.OnXpChanged -= RunProgression_OnXpChanged;
        if (PlayerStats.Instance != null) {
            PlayerStats.Instance.OnPerkAdded -= PlayerStats_OnPerkAdded;
            PlayerStats.Instance.OnStatsChanged -= RefreshWeapon;
        }
        if (Base.Instance != null) Base.Instance.OnHealthValuesChanged -= Base_OnHealthValuesChanged;
        if (WaveManager.Instance != null) {
            WaveManager.Instance.OnWaveAnnounced -= WaveManager_OnWaveAnnounced;
            WaveManager.Instance.OnBossSpawned -= WaveManager_OnBossSpawned;
        }
        if (Manager != null) Manager.OnSkillProgressChanged -= Manager_OnSkillProgressChanged;
        if (GameFlow.Instance != null) GameFlow.Instance.OnRunStarted -= GameFlow_OnRunStarted;
        if (Player.Instance != null) Player.Instance.GetMelee().OnParried -= Melee_OnParried;
        EnemyShield.OnAnyShieldSpawned -= EnemyShield_OnAnyShieldSpawned;

        UnbindBoss();
        skillHintTween?.Kill();
        goldTween?.Kill();
    }

    #endregion

    #region Güncellemeler

    private void GameFlow_OnRunStarted() {
        tutorialParries = 0;
        if (SaveSystem.Data.totalRuns <= 2) {
            tutorialHint.DOKill();
            tutorialHint.DOFade(1f, 0.4f).SetDelay(2.2f).SetUpdate(true).SetLink(gameObject);
        }
    }

    // Kalkanlı düşman ilk kez görülünce bir kerelik ipucu
    private void EnemyShield_OnAnyShieldSpawned(EnemyShield shield) {
        if (SaveSystem.Data.seenShieldTip) return;

        SaveSystem.Data.seenShieldTip = true;
        SaveSystem.Save();

        tutorialText.text = Loc.T("tutorial_shield");
        tutorialHint.DOKill();
        Sequence sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Append(tutorialHint.DOFade(1f, 0.3f).SetDelay(1.2f));
        sequence.AppendInterval(5f);
        sequence.Append(tutorialHint.DOFade(0f, 0.5f));
    }

    private void Melee_OnParried(object sender, System.EventArgs e) {
        tutorialParries++;
        if (tutorialParries == 3 && tutorialHint.alpha > 0f) {
            tutorialHint.DOKill();
            tutorialHint.DOFade(0f, 0.5f).SetUpdate(true).SetLink(gameObject);
        }
    }

    private void LevelManager_OnScoreChanged(int score) {
        scoreValue.text = score.ToString();
        int best = Mathf.Max(score, SaveSystem.Data.bestScore);
        bestValue.text = Loc.T("best") + "  " + best;

        if (score > 0) UITweens.Pulse(scoreValue.transform, 0.15f, 0.2f);
    }

    private void RunProgression_OnXpChanged(int xp, int xpToNext, int level) {
        string oldLevel = levelValue.text;
        levelValue.text = level.ToString();
        xpValue.text = xp + " / " + xpToNext + " XP";
        xpBar.SetValue(xpToNext > 0 ? xp / (float)xpToNext : 0f);

        if (oldLevel != levelValue.text) {
            UITweens.Pulse(levelBadge, 0.35f, 0.4f);
        }
    }

    private void PlayerStats_OnPerkAdded(PerkDefinition perk) {
        if (perkChipCounts.TryGetValue(perk.type, out TextMeshProUGUI count)) {
            count.text = PlayerStats.Instance.GetStacks(perk.type).ToString();
            UITweens.Pulse(count.transform.parent, 0.3f, 0.3f);
            return;
        }

        if (perkChipCounts.Count >= MaxPerkChips) return;

        Color rarityColor = UITheme.RarityColor(perk.rarity);
        Image chip = UIFactory.BorderedPanel("Perk_" + perk.type, perkRow, UITheme.PanelLight, rarityColor, 2f, 2.5f);
        chip.rectTransform.sizeDelta = new Vector2(30f, 30f);

        TextMeshProUGUI icon = UIFactory.Text("Icon", chip.transform, perk.icon, 16f, rarityColor);
        icon.rectTransform.Stretch();

        TextMeshProUGUI stack = UIFactory.Text("Count", chip.transform, PlayerStats.Instance.GetStacks(perk.type).ToString(), 12f, UITheme.Text);
        stack.rectTransform.Place(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(2f, -3f), new Vector2(14f, 14f));
        perkChipCounts[perk.type] = stack;

        UITweens.PopIn(chip.transform, 0.3f, 0.35f);
    }

    private void RefreshWeapon() {
        WeaponDefinition weapon = PlayerStats.Instance.Weapon;
        weaponName.text = weapon.displayName.Get();
        weaponStripe.color = weapon.trailColor;
    }

    private void Base_OnHealthValuesChanged(float current, float max) {
        float ratio = max > 0f ? current / max : 0f;
        castleBar.SetValue(ratio);
        castleBar.SetColor(Color.Lerp(UITheme.Danger, UITheme.Success, Mathf.InverseLerp(0.2f, 0.7f, ratio)));
        castleValue.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);

        if (ratio < 1f && current < max) {
            UITweens.Pulse(castleBar.transform, 0.06f, 0.2f);
        }
    }

    private void WaveManager_OnWaveAnnounced(int wave) {
        RefreshWave();

        // Boss sahneden normal ölüm dışında kalktıysa (ör. test kısayolu) çubuğu gizle
        if (WaveManager.Instance.CurrentBoss == null && bossGroup.gameObject.activeSelf) {
            UnbindBoss();
            bossGroup.gameObject.SetActive(false);
        }
        UITweens.Pulse(waveTitle.transform, 0.2f, 0.3f);
    }

    private void Enemy_OnAnyEnemyDied(object sender, Enemy.OnAnyEnemyDiedEventArgs e) {
        RefreshWave();
    }

    private void RefreshWave() {
        int wave = Mathf.Max(1, WaveManager.Instance.CurrentWave);
        waveTitle.text = Loc.T("wave") + " " + wave;

        if (WaveManager.Instance.IsBossWaveNumber(wave)) {
            waveSubtitle.text = "<color=" + UITheme.Hex(UITheme.Danger) + ">" + Loc.T("boss_wave") + "</color>";
        }
        else if (WaveManager.Instance.IsWaveActive) {
            waveSubtitle.text = Loc.Format("enemies_left", WaveManager.Instance.EnemiesRemaining);
        }
        else {
            waveSubtitle.text = Loc.Format("enemies_left", GameDatabase.Instance.balance.GetEnemyCountForWave(wave));
        }
    }

    private void Update() {
        // Kalan düşman sayısı doğum anında da değişir; ucuz olduğu için her karede yenile
        if (WaveManager.Instance != null && WaveManager.Instance.IsWaveActive && !WaveManager.Instance.IsBossWave) {
            waveSubtitle.text = Loc.Format("enemies_left", WaveManager.Instance.EnemiesRemaining);
        }
    }

    private void Manager_OnSkillProgressChanged(float progress) {
        bool ready = progress >= 1f && !IsSkillActive();
        if (ready && skillHint.alpha < 0.5f) {
            skillHintTween?.Kill();
            skillHint.alpha = 1f;
            skillHintTween = skillHint.DOFade(0.55f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(gameObject);
            UITweens.PopIn(skillHint.transform, 0.8f, 0.3f);
        }
        else if (!ready && skillHint.alpha > 0f) {
            skillHintTween?.Kill();
            skillHint.alpha = 0f;
        }
    }

    private static bool IsSkillActive() {
        ProjectileWaveSkill skill = Player.Instance != null ? Player.Instance.GetComponent<ProjectileWaveSkill>() : null;
        return skill != null && skill.IsActive;
    }

    // Altın sayacı: sikkeler HUD'a uçtukça yumuşakça artar
    public void SetGold(int amount, bool instant = false) {
        targetGold = amount;
        goldTween?.Kill();

        if (instant) {
            displayedGold = amount;
            goldValue.text = amount.ToString();
            return;
        }

        goldTween = DOVirtual.Int(displayedGold, targetGold, 0.35f, v => {
            displayedGold = v;
            goldValue.text = v.ToString();
        }).SetUpdate(true).SetLink(gameObject);
        UITweens.Pulse(goldPill, 0.08f, 0.2f);
    }

    #endregion

    #region Boss

    private void WaveManager_OnBossSpawned(Boss boss) {
        UnbindBoss();
        currentBoss = boss;

        boss.OnHealthChanged += Boss_OnHealthChanged;
        boss.OnEnraged += Boss_OnEnraged;
        boss.OnDeathStarted += Boss_OnDeathStarted;

        bossName.text = boss.Definition.displayName.Get().ToUpper(Loc.Current == Language.Turkish ? new System.Globalization.CultureInfo("tr-TR") : System.Globalization.CultureInfo.InvariantCulture)
            + "  <size=17><color=" + UITheme.Hex(UITheme.TextMuted) + ">" + boss.Definition.title.Get() + "</color></size>";
        bossEnraged.alpha = 0f;
        bossBar.SetColor(UITheme.Danger);
        bossBar.SetValue(1f, true);

        GameObject panel = bossGroup.gameObject;
        panel.SetActive(true);
        bossGroup.alpha = 0f;
        bossGroup.DOFade(1f, 0.5f).SetUpdate(true).SetLink(panel);
        UITweens.PopIn(panel.transform, 0.85f, 0.4f);
    }

    private void Boss_OnHealthChanged(float normalized) {
        bossBar.SetValue(normalized);
    }

    private void Boss_OnEnraged() {
        bossEnraged.alpha = 1f;
        UITweens.Pulse(bossEnraged.transform, 0.4f, 0.4f);
        bossBar.SetColor(new Color(1f, 0.45f, 0.15f));
    }

    private void Boss_OnDeathStarted() {
        UnbindBoss();
        GameObject panel = bossGroup.gameObject;
        bossGroup.DOFade(0f, 0.6f).SetUpdate(true).SetLink(panel).OnComplete(() => panel.SetActive(false));
    }

    private void UnbindBoss() {
        if (currentBoss == null) return;

        currentBoss.OnHealthChanged -= Boss_OnHealthChanged;
        currentBoss.OnEnraged -= Boss_OnEnraged;
        currentBoss.OnDeathStarted -= Boss_OnDeathStarted;
        currentBoss = null;
    }

    #endregion
}
