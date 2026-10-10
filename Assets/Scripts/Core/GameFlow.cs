using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {
    MainMenu,
    Playing,
    LevelUp,
    Paused,
    GameOver
}

// Bir koşu boyunca tutulan istatistikler (oyun sonu ekranı için)
public class RunStats {
    public int kills;
    public int parries;
    public int goldEarned;
    public int bossesDefeated;
    public int waveReached;
    public float startTime;
    public float endTime;
    public bool isNewBestScore;
    public bool isNewBestWave;

    public float Duration => endTime - startTime;
}

// Oyunun ana durum makinesi: Menü -> Oyun -> (Seviye atlama / Duraklatma) -> Oyun sonu
public class GameFlow : MonoBehaviour {

    public static GameFlow Instance { get; private set; }

    public event Action<GameState> OnStateChanged;
    public event Action OnRunStarted;
    public event Action OnRunEnded;

    public GameState State { get; private set; } = GameState.MainMenu;
    public RunStats Stats { get; private set; } = new RunStats();
    public bool IsPlaying => State == GameState.Playing;
    public bool IsAutoStarting { get; private set; }

    // "Tekrar Oyna" sahneyi yeniden yükler ve menüyü atlayıp doğrudan oyunu başlatır
    private static bool startRunOnLoad;

    private readonly object pauseOwner = new object();
    private readonly object levelUpOwner = new object();

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla GameFlow var!");
        }
        Instance = this;

        GameTime.Reset();

        IsAutoStarting = startRunOnLoad;
        startRunOnLoad = false;
    }

    private void Start() {
        Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
        Player.Instance.GetMelee().OnParried += Melee_OnParried;
        Boss.OnAnyBossDefeated += Boss_OnAnyBossDefeated;
        GameEvents.OnGoldEarned += GameEvents_OnGoldEarned;

        if (IsAutoStarting) {
            StartCoroutine(StartRunNextFrame());
        }
        else {
            SetState(GameState.MainMenu);
        }
    }

    // Diğer sistemlerin Start'ta olaylara abone olabilmesi için bir kare bekle
    private IEnumerator StartRunNextFrame() {
        yield return null;
        IsAutoStarting = false;
        StartRun();
    }

    private void Enemy_OnAnyEnemyDied(object sender, Enemy.OnAnyEnemyDiedEventArgs e) {
        Stats.kills++;
    }

    private void Melee_OnParried(object sender, EventArgs e) {
        Stats.parries++;
    }

    private void Boss_OnAnyBossDefeated(Boss boss) {
        Stats.bossesDefeated++;
    }

    private void GameEvents_OnGoldEarned(Vector3 position, int amount) {
        Stats.goldEarned += amount;
    }

    public void StartRun() {
        if (State != GameState.MainMenu) return;

        Stats = new RunStats { startTime = Time.time };
        SaveSystem.Data.totalRuns++;

        SetState(GameState.Playing);
        OnRunStarted?.Invoke();
    }

    public void EndRun() {
        if (State == GameState.GameOver) return;

        GameTime.ReleasePause(pauseOwner);
        GameTime.ReleasePause(levelUpOwner);

        Stats.endTime = Time.time;
        Stats.waveReached = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 0;

        SaveData save = SaveSystem.Data;
        int score = LevelManager.Instance.Score;
        if (score > save.bestScore) {
            save.bestScore = score;
            Stats.isNewBestScore = true;
        }
        if (Stats.waveReached > save.bestWave) {
            save.bestWave = Stats.waveReached;
            Stats.isNewBestWave = true;
        }
        save.totalKills += Stats.kills;
        save.totalParries += Stats.parries;
        save.bossesDefeated += Stats.bossesDefeated;
        SaveSystem.Save();

        if (LeaderboardService.Instance != null) {
            LeaderboardService.Instance.SubmitScore(score, Stats.waveReached);
        }

        SetState(GameState.GameOver);
        OnRunEnded?.Invoke();
    }

    public void TogglePause() {
        if (State == GameState.Playing) {
            GameTime.RequestPause(pauseOwner);
            SetState(GameState.Paused);
        }
        else if (State == GameState.Paused) {
            GameTime.ReleasePause(pauseOwner);
            SetState(GameState.Playing);
        }
    }

    public void EnterLevelUp() {
        if (State != GameState.Playing) return;

        GameTime.RequestPause(levelUpOwner);
        SetState(GameState.LevelUp);
    }

    public void ExitLevelUp() {
        if (State != GameState.LevelUp) return;

        GameTime.ReleasePause(levelUpOwner);
        SetState(GameState.Playing);
    }

    public void RestartRun() {
        startRunOnLoad = true;
        ReloadScene();
    }

    public void ReturnToMenu() {
        startRunOnLoad = false;
        ReloadScene();
    }

    public void QuitGame() {
        SaveSystem.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ReloadScene() {
        SaveSystem.Save();
        GameTime.Reset();
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetState(GameState newState) {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    private void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) SaveSystem.Save();
    }

    private void OnApplicationQuit() {
        SaveSystem.Save();
    }

    private void OnDestroy() {
        Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;
        Boss.OnAnyBossDefeated -= Boss_OnAnyBossDefeated;
        GameEvents.OnGoldEarned -= GameEvents_OnGoldEarned;

        if (Player.Instance != null) {
            Player.Instance.GetMelee().OnParried -= Melee_OnParried;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        startRunOnLoad = false;
    }
}
