using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LeaderboardEntry {
    public int rank;
    public string name;
    public int score;
    public bool isLocalPlayer;
}

public enum LeaderboardSubmitState {
    None,
    Pending,
    Online,  // dünya sıralaması alındı
    Local    // çevrimdışı / ayarlanmamış: cihazdaki sıralama
}

// Skor tablosu: LootLocker (WebGL'de tarayıcıdan doğrudan çalışır) + cihazdaki yedek liste.
// Kurulum için README'ye bakın: GameDatabase > online alanına Game API Key ve leaderboard key girilir.
public class LeaderboardService : MonoBehaviour {

    public static LeaderboardService Instance { get; private set; }

    public event Action OnSubmitStateChanged;

    public const int MinNameLength = 2;
    public const int MaxNameLength = 16;
    private const int LocalScoreLimit = 10;
    private const int RequestTimeout = 10;
    private const string ApiHost = "api.lootlocker.io";

    #region LootLocker cevapları (alan adları API ile birebir aynı olmalı)

    [Serializable] private class SessionResponse { public string session_token; public int player_id; public string player_identifier; }
    [Serializable] private class PlayerInfo { public int id; public string name; }
    [Serializable] private class ListItem { public string member_id; public int rank; public int score; public PlayerInfo player; }
    [Serializable] private class ListResponse { public ListItem[] items; }
    [Serializable] private class SubmitResponse { public string member_id; public int rank; public int score; }

    #endregion

    // Sahne yeniden yüklense de aynı oturum kullanılsın
    private static string sessionToken;
    private static int sessionPlayerId;

    public LeaderboardSubmitState SubmitState { get; private set; } = LeaderboardSubmitState.None;
    public int LastRank { get; private set; }

    private static OnlineSettings Settings => GameDatabase.Instance.online;
    public bool IsOnlineConfigured => Settings != null && Settings.IsConfigured;

    private static string BaseUrl {
        get {
            string domainKey = Settings.lootLockerDomainKey;
            return string.IsNullOrWhiteSpace(domainKey) ? "https://" + ApiHost : "https://" + domainKey.Trim() + "." + ApiHost;
        }
    }

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla LeaderboardService var!");
        }
        Instance = this;
    }

    #region Oyuncu adı

    public static string PlayerName {
        get {
            SaveData save = SaveSystem.Data;
            if (string.IsNullOrWhiteSpace(save.playerName)) {
                string prefix = Loc.Current == Language.Turkish ? "Şövalye" : "Knight";
                save.playerName = prefix + UnityEngine.Random.Range(1000, 10000);
                SaveSystem.Save();
            }
            return save.playerName;
        }
    }

    public static string SanitizeName(string raw) {
        if (raw == null) return string.Empty;

        StringBuilder builder = new StringBuilder();
        foreach (char c in raw.Trim()) {
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '_' || c == '-') {
                builder.Append(c);
            }
            if (builder.Length >= MaxNameLength) break;
        }
        return builder.ToString().Trim();
    }

    // Geçerliyse kaydeder ve (ayarlıysa) çevrimiçi adı da günceller
    public bool TrySetPlayerName(string raw, Action<bool> onComplete = null) {
        string name = SanitizeName(raw);
        if (name.Length < MinNameLength) return false;

        SaveSystem.Data.playerName = name;
        foreach (LocalScore entry in SaveSystem.Data.localScores) {
            entry.name = name; // cihazdaki skorların hepsi bu oyuncuya ait
        }
        SaveSystem.Save();

        if (IsOnlineConfigured) {
            StartCoroutine(SetNameRoutine(name, onComplete));
        }
        else {
            onComplete?.Invoke(true);
        }
        return true;
    }

    private IEnumerator SetNameRoutine(string name, Action<bool> onComplete) {
        yield return EnsureSession();
        if (sessionToken == null) {
            onComplete?.Invoke(false);
            yield break;
        }

        using (UnityWebRequest request = CreateRequest("PATCH", "/game/player/name", "{\"name\":" + JsonString(name) + "}")) {
            yield return request.SendWebRequest();
            onComplete?.Invoke(request.result == UnityWebRequest.Result.Success);
        }
    }

    #endregion

    #region Skor gönderme

    public void SubmitScore(int score, int wave) {
        if (score <= 0) return;

        int localRank = AddLocalScore(score, wave);

        if (!IsOnlineConfigured) {
            SetSubmitState(LeaderboardSubmitState.Local, localRank);
            return;
        }

        SetSubmitState(LeaderboardSubmitState.Pending, 0);
        StartCoroutine(SubmitRoutine(score, localRank));
    }

    private IEnumerator SubmitRoutine(int score, int localRank) {
        for (int attempt = 0; attempt < 2; attempt++) {
            yield return EnsureSession();
            if (sessionToken == null) break;

            string path = "/game/leaderboards/" + UnityWebRequest.EscapeURL(Settings.leaderboardKey.Trim()) + "/submit";
            using (UnityWebRequest request = CreateRequest("POST", path, "{\"score\":" + score + "}")) {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success) {
                    SubmitResponse response = Parse<SubmitResponse>(request.downloadHandler.text);
                    SetSubmitState(LeaderboardSubmitState.Online, response != null ? response.rank : 0);
                    yield break;
                }

                // Oturum süresi dolmuşsa bir kez yeniden bağlan
                if (request.responseCode == 401) {
                    sessionToken = null;
                    continue;
                }

                Debug.LogWarning("Skor gönderilemedi: " + request.responseCode + " " + request.error);
                break;
            }
        }

        SetSubmitState(LeaderboardSubmitState.Local, localRank);
    }

    private void SetSubmitState(LeaderboardSubmitState state, int rank) {
        SubmitState = state;
        LastRank = rank;
        OnSubmitStateChanged?.Invoke();
    }

    #endregion

    #region Liste

    // Sonuç: çevrimiçi mi, liste. Çevrimiçi alınamazsa cihazdaki liste döner.
    public void FetchTop(int count, Action<bool, List<LeaderboardEntry>> onComplete) {
        if (!IsOnlineConfigured) {
            onComplete?.Invoke(false, GetLocalEntries());
            return;
        }
        StartCoroutine(FetchRoutine(count, onComplete));
    }

    private IEnumerator FetchRoutine(int count, Action<bool, List<LeaderboardEntry>> onComplete) {
        yield return EnsureSession();
        if (sessionToken == null) {
            onComplete?.Invoke(false, GetLocalEntries());
            yield break;
        }

        string path = "/game/leaderboards/" + UnityWebRequest.EscapeURL(Settings.leaderboardKey.Trim()) + "/list?count=" + count;
        using (UnityWebRequest request = CreateRequest("GET", path, null)) {
            yield return request.SendWebRequest();

            ListResponse response = request.result == UnityWebRequest.Result.Success ? Parse<ListResponse>(request.downloadHandler.text) : null;
            if (response == null || response.items == null) {
                Debug.LogWarning("Skor tablosu alınamadı: " + request.responseCode + " " + request.error);
                onComplete?.Invoke(false, GetLocalEntries());
                yield break;
            }

            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
            foreach (ListItem item in response.items) {
                bool isLocal = item.player != null && item.player.id == sessionPlayerId;
                string name = item.player != null && !string.IsNullOrEmpty(item.player.name) ? item.player.name : "#" + item.member_id;
                entries.Add(new LeaderboardEntry { rank = item.rank, name = name, score = item.score, isLocalPlayer = isLocal });
            }
            onComplete?.Invoke(true, entries);
        }
    }

    #endregion

    #region Cihazdaki skorlar

    private int AddLocalScore(int score, int wave) {
        List<LocalScore> scores = SaveSystem.Data.localScores;
        LocalScore entry = new LocalScore { name = PlayerName, score = score, wave = wave };
        scores.Add(entry);
        scores.Sort((a, b) => b.score.CompareTo(a.score));
        if (scores.Count > LocalScoreLimit) {
            scores.RemoveRange(LocalScoreLimit, scores.Count - LocalScoreLimit);
        }
        SaveSystem.Save();

        int index = scores.IndexOf(entry);
        return index >= 0 ? index + 1 : 0;
    }

    public List<LeaderboardEntry> GetLocalEntries() {
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        List<LocalScore> scores = SaveSystem.Data.localScores;
        for (int i = 0; i < scores.Count; i++) {
            entries.Add(new LeaderboardEntry { rank = i + 1, name = scores[i].name, score = scores[i].score, isLocalPlayer = true });
        }
        return entries;
    }

    #endregion

    #region LootLocker oturumu

    private IEnumerator EnsureSession() {
        if (sessionToken != null) yield break;

        OnlineSettings settings = Settings;
        string gameKey = settings.lootLockerGameKey.Trim();
        StringBuilder body = new StringBuilder();
        body.Append("{\"game_key\":").Append(JsonString(gameKey));
        body.Append(",\"game_version\":").Append(JsonString(Application.version));

        // Yeni tip anahtarlar (dev_ / prod_) ortamı kendisi belirtir
        if (!gameKey.StartsWith("dev_") && !gameKey.StartsWith("prod_")) {
            body.Append(",\"development_mode\":").Append(settings.developmentMode ? "true" : "false");
        }

        string identifier = SaveSystem.Data.onlinePlayerIdentifier;
        if (!string.IsNullOrEmpty(identifier)) {
            body.Append(",\"player_identifier\":").Append(JsonString(identifier));
        }
        body.Append("}");

        using (UnityWebRequest request = CreateRequest("POST", "/game/v2/session/guest", body.ToString(), includeSession: false)) {
            yield return request.SendWebRequest();

            SessionResponse response = request.result == UnityWebRequest.Result.Success ? Parse<SessionResponse>(request.downloadHandler.text) : null;
            if (response == null || string.IsNullOrEmpty(response.session_token)) {
                Debug.LogWarning("LootLocker oturumu açılamadı: " + request.responseCode + " " + request.error);
                yield break;
            }

            sessionToken = response.session_token;
            sessionPlayerId = response.player_id;

            bool isNewPlayer = SaveSystem.Data.onlinePlayerIdentifier != response.player_identifier;
            SaveSystem.Data.onlinePlayerIdentifier = response.player_identifier;
            SaveSystem.Save();

            // Yeni misafir oyuncuya kayıtlı adı ver (skor tablosunda görünsün)
            if (isNewPlayer) {
                using (UnityWebRequest nameRequest = CreateRequest("PATCH", "/game/player/name", "{\"name\":" + JsonString(PlayerName) + "}")) {
                    yield return nameRequest.SendWebRequest();
                }
            }
        }
    }

    private static UnityWebRequest CreateRequest(string method, string path, string jsonBody, bool includeSession = true) {
        UnityWebRequest request = new UnityWebRequest(BaseUrl + path, method) {
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = RequestTimeout
        };

        if (jsonBody != null) {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        }
        request.SetRequestHeader("Content-Type", "application/json");
        if (includeSession && sessionToken != null) {
            request.SetRequestHeader("x-session-token", sessionToken);
        }
        return request;
    }

    private static T Parse<T>(string json) where T : class {
        try {
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception) {
            return null;
        }
    }

    private static string JsonString(string value) {
        StringBuilder builder = new StringBuilder("\"");
        foreach (char c in value ?? string.Empty) {
            switch (c) {
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                default:
                    if (c < 0x20) builder.Append("\\u").Append(((int)c).ToString("x4"));
                    else builder.Append(c);
                    break;
            }
        }
        return builder.Append('"').ToString();
    }

    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        sessionToken = null;
        sessionPlayerId = 0;
    }
}
