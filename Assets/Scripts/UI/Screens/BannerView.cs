using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Ekranın ortasında beliren büyük duyurular: "DALGA 3", "BOSS YAKLAŞIYOR", "DALGA TEMİZLENDİ"...
public class BannerView : MonoBehaviour {

    private CanvasGroup group;
    private RectTransform band;
    private TextMeshProUGUI title;
    private TextMeshProUGUI subtitle;
    private Image accentLine;
    private Sequence sequence;
    private Boss boundBoss;

    public void Initialize() {
        RectTransform root = (RectTransform)transform;
        root.Stretch();
        group = UIFactory.Group(this);
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        Image bandImage = UIFactory.Image("Band", root, new Color(0.02f, 0.03f, 0.06f, 0.6f));
        band = bandImage.rectTransform;
        band.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(UITheme.ReferenceResolution.x * 1.2f, 170f));

        accentLine = UIFactory.Image("Accent", band, UITheme.Gold);
        accentLine.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(520f, 3f));

        title = UIFactory.Text("Title", band, "", 84f, UITheme.Text);
        title.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(1600f, 100f));
        title.characterSpacing = 10f;
        UIFactory.AddShadow(title, -1.5f, 0.5f);

        subtitle = UIFactory.Text("Subtitle", band, "", 30f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
        subtitle.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -48f), new Vector2(1600f, 40f));
        subtitle.characterSpacing = 4f;
    }

    public void Bind() {
        WaveManager.Instance.OnWaveAnnounced += WaveManager_OnWaveAnnounced;
        WaveManager.Instance.OnWaveCleared += WaveManager_OnWaveCleared;
        WaveManager.Instance.OnBossSpawned += WaveManager_OnBossSpawned;
        Boss.OnAnyBossDefeated += Boss_OnAnyBossDefeated;
    }

    private void OnDestroy() {
        Boss.OnAnyBossDefeated -= Boss_OnAnyBossDefeated;
        if (WaveManager.Instance != null) {
            WaveManager.Instance.OnWaveAnnounced -= WaveManager_OnWaveAnnounced;
            WaveManager.Instance.OnWaveCleared -= WaveManager_OnWaveCleared;
            WaveManager.Instance.OnBossSpawned -= WaveManager_OnBossSpawned;
        }
        if (boundBoss != null) boundBoss.OnEnraged -= Boss_OnEnraged;
        sequence?.Kill();
    }

    public void Show(string titleText, string subtitleText, Color color, float holdDuration = 1.2f) {
        sequence?.Kill();

        title.text = titleText;
        title.color = color;
        subtitle.text = subtitleText;
        accentLine.color = color;

        group.alpha = 0f;
        title.rectTransform.localScale = Vector3.one * 1.35f;
        band.localScale = new Vector3(1f, 0.2f, 1f);
        accentLine.rectTransform.sizeDelta = new Vector2(0f, 3f);

        sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        sequence.Append(group.DOFade(1f, 0.18f));
        sequence.Join(band.DOScaleY(1f, 0.25f).SetEase(Ease.OutCubic));
        sequence.Join(title.rectTransform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        sequence.Join(accentLine.rectTransform.DOSizeDelta(new Vector2(520f, 3f), 0.45f).SetEase(Ease.OutCubic));
        sequence.AppendInterval(holdDuration);
        sequence.Append(group.DOFade(0f, 0.3f));
    }

    private void WaveManager_OnWaveAnnounced(int wave) {
        if (WaveManager.Instance.IsBossWaveNumber(wave)) {
            Show(Loc.T("boss_incoming"), Loc.T("wave") + " " + wave, UITheme.Danger, 1.3f);
        }
        else {
            Show(Loc.T("wave") + " " + wave, Loc.Format("enemies_left", GameDatabase.Instance.balance.GetEnemyCountForWave(wave)), UITheme.Text, 1.1f);
        }
    }

    private void WaveManager_OnWaveCleared(int wave) {
        // Boss dalgalarında "BOSS YENİLDİ" duyurusu ekranda kalsın
        if (WaveManager.Instance.IsBossWaveNumber(wave)) return;

        int bonus = GameDatabase.Instance.balance.GetWaveClearGold(wave);
        Show(Loc.T("wave_cleared"), Loc.Format("wave_bonus", bonus), UITheme.Success, 0.9f);
    }

    private void WaveManager_OnBossSpawned(Boss boss) {
        if (boundBoss != null) boundBoss.OnEnraged -= Boss_OnEnraged;
        boundBoss = boss;
        boss.OnEnraged += Boss_OnEnraged;
    }

    private void Boss_OnEnraged() {
        if (boundBoss == null) return;
        Show(boundBoss.Definition.displayName.Get(), Loc.T("boss_enraged"), new Color(1f, 0.45f, 0.15f), 0.8f);
    }

    private void Boss_OnAnyBossDefeated(Boss boss) {
        if (boundBoss == boss) {
            boss.OnEnraged -= Boss_OnEnraged;
            boundBoss = null;
        }
        Show(Loc.T("boss_defeated"), boss.Definition.displayName.Get(), UITheme.Gold, 1.4f);
    }
}
