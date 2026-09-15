using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<DifficultyTier> OnDifficultyChanged;

    public DifficultyTier CurrentDifficultyTier { get; private set; }

    [SerializeField] private DifficultyConfigSO difficultyConfigSO;
    
    private int currentTierIndex = 0;

    private int score = 0;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla LevelManager var!");
        }
        Instance = this;
    }

    private void Start() {
        CurrentDifficultyTier = difficultyConfigSO.tiers[currentTierIndex];
        ApplyDifficulty(CurrentDifficultyTier);
    }

    private void Enemy_OnAnyEnemyDied(Enemy enemy) {
        AddScore(1);
    }

    private void AddScore(int amount) {
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

    private void OnEnable() => Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
    private void OnDisable() => Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;

}
