using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Skor tablosu: oyuncu adı + ilk 10. Çevrimiçi ayarlı değilse cihazdaki skorları gösterir.
public class LeaderboardView : UIScreen {

    private const int RowCount = 10;
    private const float RowHeight = 48f;
    private const float TableWidth = 820f;

    private class Row {
        public Image background;
        public TextMeshProUGUI rank;
        public TextMeshProUGUI name;
        public TextMeshProUGUI score;
    }

    private RectTransform table;
    private TMP_InputField nameInput;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI emptyText;
    private readonly List<Row> rows = new List<Row>();
    private int requestVersion;

    protected override void Build() {
        RectTransform content = CreateOverlay(new Color(0.03f, 0.04f, 0.08f, 0.94f));

        TextMeshProUGUI title = UIFactory.Text("Title", content, Loc.T("leaderboard"), 64f, UITheme.Text);
        title.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(1000f, 80f));
        title.characterSpacing = 12f;
        UIFactory.AddShadow(title);

        UIButton back = UIFactory.Button("Back", content, "‹  " + Loc.T("back"), UITheme.Neutral, new Vector2(200f, 60f), () => Manager.CloseLeaderboard(), 26f);
        back.GetComponent<RectTransform>().Place(new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(200f, 60f));

        BuildNameRow(content);

        statusText = UIFactory.Text("Status", content, "", 20f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
        statusText.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -214f), new Vector2(1200f, 30f));

        BuildTable(content);
    }

    private void BuildNameRow(RectTransform content) {
        TextMeshProUGUI label = UIFactory.Text("NameLabel", content, Loc.T("your_name"), 26f, UITheme.TextMuted, TextAlignmentOptions.Right, false);
        label.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), new Vector2(-200f, -160f), new Vector2(200f, 40f));

        nameInput = UIFactory.InputField("NameInput", content, new Vector2(360f, 56f), Loc.T("your_name"), LeaderboardService.MaxNameLength);
        nameInput.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(360f, 56f));
        nameInput.onSubmit.AddListener(_ => SaveName());

        UIButton save = UIFactory.Button("SaveName", content, Loc.T("save"), UITheme.Gold, new Vector2(180f, 56f), SaveName, 24f);
        save.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1f), new Vector2(0f, 0.5f), new Vector2(200f, -160f), new Vector2(180f, 56f));
    }

    private void BuildTable(RectTransform content) {
        Image panel = UIFactory.BorderedPanel("Table", content, UITheme.Panel, UITheme.PanelBorder);
        table = panel.rectTransform;
        float height = 70f + RowCount * RowHeight + 20f;
        table.Center(new Vector2(0f, -60f), new Vector2(TableWidth, height));

        TextMeshProUGUI rankHeader = UIFactory.Text("RankHeader", table, "#", 20f, UITheme.TextMuted);
        rankHeader.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(70f, -36f), new Vector2(80f, 30f));
        TextMeshProUGUI nameHeader = UIFactory.Text("NameHeader", table, Loc.T("col_name"), 20f, UITheme.TextMuted, TextAlignmentOptions.Left);
        nameHeader.rectTransform.Place(new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(140f, -36f), new Vector2(400f, 30f));
        nameHeader.characterSpacing = 6f;
        TextMeshProUGUI scoreHeader = UIFactory.Text("ScoreHeader", table, Loc.T("col_score"), 20f, UITheme.TextMuted, TextAlignmentOptions.Right);
        scoreHeader.rectTransform.Place(new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-40f, -36f), new Vector2(200f, 30f));
        scoreHeader.characterSpacing = 6f;

        for (int i = 0; i < RowCount; i++) {
            float y = -70f - i * RowHeight;

            Row row = new Row();
            // Lineer renk uzayında düşük alfa beklenenden parlak görünür; şerit çok hafif tutuldu
            row.background = UIFactory.Panel("Row", table, new Color(0.7f, 0.8f, 1f, i % 2 == 0 ? 0.012f : 0f), 2f);
            row.background.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(TableWidth - 40f, RowHeight - 4f));

            row.rank = UIFactory.Text("Rank", row.background.transform, "", 24f, UITheme.TextMuted);
            row.rank.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50f, 0f), new Vector2(80f, 36f));
            row.name = UIFactory.Text("Name", row.background.transform, "", 24f, UITheme.Text, TextAlignmentOptions.Left, false);
            row.name.rectTransform.Place(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 0f), new Vector2(460f, 36f));
            row.score = UIFactory.Text("Score", row.background.transform, "", 26f, UITheme.Text, TextAlignmentOptions.Right);
            row.score.rectTransform.Place(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(200f, 36f));

            rows.Add(row);
        }

        emptyText = UIFactory.Text("Empty", table, Loc.T("lb_empty"), 24f, UITheme.TextMuted, TextAlignmentOptions.Center, false);
        emptyText.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(700f, 40f));
    }

    protected override void OnShow() {
        nameInput.text = LeaderboardService.PlayerName;
        UITweens.PopIn(table, 0.96f, 0.25f);
        Refresh();
    }

    private void SaveName() {
        LeaderboardService service = LeaderboardService.Instance;
        if (service == null) return;

        if (service.TrySetPlayerName(nameInput.text, success => {
                if (this != null && IsVisible) Refresh();
            })) {
            nameInput.text = LeaderboardService.PlayerName;
            Manager.ShowToast(Loc.T("name_saved"));
        }
        else {
            AudioManager.Instance.Play(Sfx.Error);
            Manager.ShowToast(Loc.T("name_invalid"));
        }
    }

    private void Refresh() {
        LeaderboardService service = LeaderboardService.Instance;
        if (service == null) return;

        int version = ++requestVersion;

        if (!service.IsOnlineConfigured) {
            statusText.text = Loc.T("lb_not_configured");
            Fill(service.GetLocalEntries());
            return;
        }

        statusText.text = Loc.T("loading");
        Fill(new List<LeaderboardEntry>(), showEmpty: false);

        service.FetchTop(RowCount, (online, entries) => {
            // Ekran kapandıysa ya da daha yeni bir istek varsa sonucu yok say
            if (this == null || version != requestVersion) return;

            statusText.text = online ? Loc.T("lb_online") : Loc.T("lb_offline");
            Fill(entries);
        });
    }

    private void Fill(List<LeaderboardEntry> entries, bool showEmpty = true) {
        for (int i = 0; i < rows.Count; i++) {
            Row row = rows[i];
            bool hasEntry = i < entries.Count;
            row.rank.gameObject.SetActive(hasEntry);
            row.name.gameObject.SetActive(hasEntry);
            row.score.gameObject.SetActive(hasEntry);
            if (!hasEntry) continue;

            LeaderboardEntry entry = entries[i];
            Color rankColor = entry.rank == 1 ? UITheme.Gold : entry.rank <= 3 ? UITheme.Text : UITheme.TextMuted;
            row.rank.text = entry.rank.ToString();
            row.rank.color = rankColor;
            row.name.text = entry.name;
            row.name.color = entry.isLocalPlayer && LeaderboardService.Instance.IsOnlineConfigured ? UITheme.Gold : UITheme.Text;
            row.score.text = entry.score.ToString();
        }

        emptyText.gameObject.SetActive(showEmpty && entries.Count == 0);
    }
}
