using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<DifficultyTier> OnDifficultyChanged;

    public DifficultyTier CurrentDifficultyTier { get; private set; }

    [SerializeField] private DifficultyConfigSO difficultyConfigSO;
    [SerializeField] private Base baseObject;

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

        baseObject.OnDied += BaseObject_OnDied; 
    }

    private void BaseObject_OnDied() {
        Player.Instance.gameObject.SetActive(false);
    }

    private void Enemy_OnAnyEnemyDied(object sender, Enemy.OnAnyEnemyDiedEventArgs e) {
        AddScore(e.scoreToKill);
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

    private void OnDestroy() {
        baseObject.OnDied -= BaseObject_OnDied;
    }

    private void OnEnable() => Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
    private void OnDisable() => Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;

}
