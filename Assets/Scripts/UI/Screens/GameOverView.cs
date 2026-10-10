using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverView : UIScreen {

    private RectTransform panel;
    private TextMeshProUGUI scoreValue;
    private TextMeshProUGUI rankText;
    private TextMeshProUGUI recordBadge;
    private RectTransform recordRoot;
    private TextMeshProUGUI[] statValues;

    protected override void Build() {
        RectTransform content = CreateOverlay(new Color(0.06f, 0.02f, 0.04f, 0.8f));

        Image panelImage = UIFactory.BorderedPanel("Panel", content, UITheme.Panel, new Color(0.55f, 0.16f, 0.22f), 3f);
        panel = panelImage.rectTransform;
        panel.Center(Vector2.zero, new Vector2(760f, 780f));

        TextMeshProUGUI title = UIFactory.Text("Title", panel, Loc.T("game_over"), 64f, UITheme.Danger);
        title.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(700f, 76f));
        title.characterSpacing = 8f;
        UIFactory.AddShadow(title);

        Image badge = UIFactory.Panel("RecordBadge", panel, UITheme.Gold, 0.85f);
        recordRoot = badge.rectTransform;
        recordRoot.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(280f, 44f));
        recordBadge = UIFactory.Text("Text", badge.transform, Loc.T("new_record"), 24f, UITheme.TextDark);
        recordBadge.rectTransform.Stretch();
        recordBadge.characterSpacing = 6f;

        TextMeshProUGUI scoreLabel = UIFactory.Text("ScoreLabel", panel, Loc.T("score"), 22f, UITheme.TextMuted);
        scoreLabel.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -184f), new Vector2(400f, 30f));
        scoreLabel.characterSpacing = 8f;

        scoreValue = UIFactory.Text("Score", panel, "0", 84f, UITheme.Text);
        scoreValue.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -206f), new Vector2(600f, 96f));

        rankText = UIFactory.Text("Rank", panel, "", 22f, UITheme.Gold, TextAlignmentOptions.Center, false);
        rankText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -296f), new Vector2(600f, 30f));

        string[] labels = {
            Loc.T("stat_wave"), Loc.T("best_score"),
            Loc.T("stat_kills"), Loc.T("stat_parries"),
            Loc.T("stat_gold"), Loc.T("stat_level"),
            Loc.T("stat_bosses"), Loc.T("stat_time")
        };
        statValues = new TextMeshProUGUI[labels.Length];

        for (int i = 0; i < labels.Length; i++) {
            int column = i % 2;
            int row = i / 2;
            Vector2 position = new Vector2(column == 0 ? -170f : 170f, -340f - row * 72f);

            Image cell = UIFactory.Panel("Cell", panel, UITheme.PanelLight, 1.5f);
            cell.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, new Vector2(320f, 62f));

            TextMeshProUGUI label = UIFactory.Text("Label", cell.transform, labels[i], 19f, UITheme.TextMuted, TextAlignmentOptions.Left, false);
            label.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(200f, 30f));

            statValues[i] = UIFactory.Text("Value", cell.transform, "", 28f, UITheme.Text, TextAlignmentOptions.Right);
            statValues[i].rectTransform.Place(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(120f, 36f));
        }

        UIButton again = UIFactory.Button("Again", panel, Loc.T("play_again"), UITheme.Gold, new Vector2(320f, 80f), () => GameFlow.Instance.RestartRun(), 30f);
        again.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-170f, 36f), new Vector2(320f, 80f));

        UIButton menu = UIFactory.Button("Menu", panel, Loc.T("main_menu"), UITheme.Neutral, new Vector2(320f, 80f), () => GameFlow.Instance.ReturnToMenu(), 28f);
        menu.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(170f, 36f), new Vector2(320f, 80f));
    }

    private void OnEnable() {
        if (LeaderboardService.Instance != null) {
            LeaderboardService.Instance.OnSubmitStateChanged += RefreshRank;
        }
    }

    private void OnDisable() {
        if (LeaderboardService.Instance != null) {
            LeaderboardService.Instance.OnSubmitStateChanged -= RefreshRank;
        }
    }

    private void RefreshRank() {
        LeaderboardService service = LeaderboardService.Instance;
        if (service == null) {
            rankText.text = "";
            return;
        }

        switch (service.SubmitState) {
            case LeaderboardSubmitState.Pending:
                rankText.text = Loc.T("rank_pending");
                break;
            case LeaderboardSubmitState.Online:
                rankText.text = service.LastRank > 0 ? Loc.Format("rank_world", service.LastRank) : "";
                break;
            case LeaderboardSubmitState.Local:
                rankText.text = service.LastRank > 0 ? Loc.Format("rank_local", service.LastRank) : "";
                break;
            default:
                rankText.text = "";
                break;
        }
    }

    protected override void OnShow() {
        RefreshRank();
        RunStats stats = GameFlow.Instance.Stats;
        int score = LevelManager.Instance.Score;

        recordRoot.gameObject.SetActive(stats.isNewBestScore || stats.isNewBestWave);

        int minutes = Mathf.FloorToInt(stats.Duration / 60f);
        int seconds = Mathf.FloorToInt(stats.Duration % 60f);

        statValues[0].text = stats.waveReached.ToString();
        statValues[1].text = SaveSystem.Data.bestScore.ToString();
        statValues[2].text = stats.kills.ToString();
        statValues[3].text = stats.parries.ToString();
        statValues[4].text = "<color=" + UITheme.Hex(UITheme.Gold) + ">+" + stats.goldEarned + "</color>";
        statValues[5].text = RunProgression.Instance.Level.ToString();
        statValues[6].text = stats.bossesDefeated.ToString();
        statValues[7].text = minutes + ":" + seconds.ToString("00");

        UITweens.PopIn(panel, 0.85f, 0.45f);

        // Skor sayarak yükselir
        scoreValue.text = "0";
        DOVirtual.Int(0, score, Mathf.Clamp(score * 0.01f, 0.4f, 1.4f), v => scoreValue.text = v.ToString())
            .SetDelay(0.25f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject);

        if (recordRoot.gameObject.activeSelf) {
            recordRoot.DOKill();
            recordRoot.localScale = Vector3.one;
            recordRoot.DOScale(1.08f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true).SetLink(recordRoot.gameObject);
        }
    }
}
