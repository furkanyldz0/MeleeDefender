using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<DifficultyTier> OnDifficultyChanged;

    public DifficultyTier CurrentDifficultyTier { get; private set; }
    public int Score => score;

    [SerializeField] private DifficultyConfigSO difficultyConfigSO;
    [SerializeField] private Base baseObject;

    private int currentTierIndex = 0;
    private int score = 0;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla LevelManager var!");
        }
        Instance = this;

        // Seviye etkileri (ilk tier) Start'tan önce hazır olsun ki diğer sistemler okuyabilsin
        CurrentDifficultyTier = difficultyConfigSO.tiers[currentTierIndex];

        SetupGameSystems();
    }

    // Yeni sistemler sahneye elle eklenmek zorunda kalmasın diye burada oluşturuluyor.
    // Sahnede zaten varsa (ör. Inspector'dan eklendiyse) tekrar eklenmez.
    private void SetupGameSystems() {
        Renderer baseRenderer = baseObject.GetComponentInChildren<MeshRenderer>();
        VisualFactory.Initialize(baseRenderer != null ? baseRenderer.sharedMaterial : null);

        EnsureComponent<GameFlow>();
        EnsureComponent<PlayerStats>();
        EnsureComponent<WaveManager>();
        EnsureComponent<RunProgression>();
        EnsureComponent<FeedbackFX>();
        EnsureComponent<AudioManager>();
        EnsureComponent<LeaderboardService>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        EnsureComponent<DevCheats>();
#endif

        if (baseObject.GetComponent<BaseVisual>() == null) {
            baseObject.gameObject.AddComponent<BaseVisual>();
        }
    }

    private void EnsureComponent<T>() where T : Component {
        if (FindFirstObjectByType<T>() == null) {
            gameObject.AddComponent<T>();
        }
    }

    private void Start() {
        ApplyDifficulty(CurrentDifficultyTier);

        baseObject.OnDied += BaseObject_OnDied;

        if (Player.Instance.GetComponent<PlayerVisual>() == null) {
            Player.Instance.gameObject.AddComponent<PlayerVisual>();
        }
    }

    private void BaseObject_OnDied() {
        Player.Instance.gameObject.SetActive(false);
        GameFlow.Instance.EndRun();
    }

    private void Enemy_OnAnyEnemyDied(object sender, Enemy.OnAnyEnemyDiedEventArgs e) {
        AddScore(e.scoreToKill);
    }

    private void Boss_OnAnyBossDefeated(Boss boss) {
        // Boss, normal bir düşmanın 10 katı puan verir
        AddScore(10 * (1 + WaveManager.Instance.CurrentWave / 5));
    }

    private void AddScore(int amount) {
        if (GameFlow.Instance.State == GameState.GameOver) return;

        score += amount;
        OnScoreChanged?.Invoke(score);
        CheckDifficultyUpgrade();
    }

    private void CheckDifficultyUpgrade() {
        while (currentTierIndex + 1 < difficultyConfigSO.tiers.Count &&
            score >= difficultyConfigSO.tiers[currentTierIndex + 1].requiredScore) {
            currentTierIndex++;
            CurrentDifficultyTier = difficultyConfigSO.tiers[currentTierIndex];
            ApplyDifficulty(CurrentDifficultyTier);
        }
    }

    private void ApplyDifficulty(DifficultyTier difficultyTier) {
        OnDifficultyChanged?.Invoke(difficultyTier);
    }

    private void OnDestroy() {
        baseObject.OnDied -= BaseObject_OnDied;
    }

    private void OnEnable() {
        Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
        Boss.OnAnyBossDefeated += Boss_OnAnyBossDefeated;
    }

    private void OnDisable() {
        Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;
        Boss.OnAnyBossDefeated -= Boss_OnAnyBossDefeated;
    }

}
