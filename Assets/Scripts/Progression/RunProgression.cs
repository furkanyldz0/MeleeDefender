using System;
using System.Collections.Generic;
using UnityEngine;

// Koşu içi ilerleme: deneyim, seviye atlama, perk seçimi ve ödüller (altın).
public class RunProgression : MonoBehaviour {

    public static RunProgression Instance { get; private set; }

    public event Action<int, int, int> OnXpChanged;                   // mevcut xp, gereken xp, seviye
    public event Action<int, List<PerkDefinition>> OnPerkChoicesOffered; // yeni seviye, seçenekler
    public event Action OnPerkChosen;

    private const int ChoiceCount = 3;

    public int Level { get; private set; } = 1;
    public int Xp { get; private set; }
    public int XpToNext { get; private set; }

    private int pendingLevelUps;
    private bool isChoosing;
    private List<PerkDefinition> currentChoices = new List<PerkDefinition>();

    private GameDatabase db;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla RunProgression var!");
        }
        Instance = this;

        db = GameDatabase.Instance;
        XpToNext = db.balance.GetXpToNextLevel(Level);
    }

    private void Start() {
        Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
        Boss.OnAnyBossDefeated += Boss_OnAnyBossDefeated;
        Player.Instance.GetMelee().OnParried += Melee_OnParried;
        WaveManager.Instance.OnWaveCleared += WaveManager_OnWaveCleared;

        OnXpChanged?.Invoke(Xp, XpToNext, Level);
    }

    private void Update() {
        // Duraklatma sırasında biriken seviye atlamaları oyuna dönünce sunulur
        if (!isChoosing && pendingLevelUps > 0 && GameFlow.Instance.State == GameState.Playing) {
            TryOfferChoices();
        }

        if (!isChoosing || GameFlow.Instance.State != GameState.LevelUp) return;

        // Klavye ile hızlı seçim
        for (int i = 0; i < currentChoices.Count; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) {
                ChoosePerk(currentChoices[i]);
                return;
            }
        }
    }

    #region Ödüller

    private void Enemy_OnAnyEnemyDied(object sender, Enemy.OnAnyEnemyDiedEventArgs e) {
        AddXp(db.balance.xpPerKillScore * e.scoreToKill);
        AddGold(db.balance.goldPerKillScore * e.scoreToKill, e.position);

        float lifesteal = PlayerStats.Instance.LifestealPerKill;
        if (lifesteal > 0f && Base.Instance != null) {
            Base.Instance.HealPercent(lifesteal);
        }
    }

    private void Boss_OnAnyBossDefeated(Boss boss) {
        AddXp(boss.Definition.xpReward);
        AddGold(Mathf.RoundToInt(boss.Definition.goldReward * boss.RewardMultiplier), boss.transform.position);
    }

    private void Melee_OnParried(object sender, EventArgs e) {
        AddXp(db.balance.xpPerParry);
    }

    private void WaveManager_OnWaveCleared(int wave) {
        Vector3 position = Base.Instance != null ? Base.Instance.transform.position : Vector3.zero;
        AddGold(db.balance.GetWaveClearGold(wave), position, applyMultiplier: false);

        if (Base.Instance != null) {
            Base.Instance.HealPercent(db.balance.waveClearRepair);
        }
    }

    public void AddGold(int amount, Vector3 worldPosition, bool applyMultiplier = true) {
        if (GameFlow.Instance.State == GameState.GameOver) return;

        if (applyMultiplier) {
            amount = Mathf.Max(1, Mathf.RoundToInt(amount * PlayerStats.Instance.GoldMultiplier));
        }

        SaveSystem.Data.gold += amount;
        GameEvents.GoldEarned(worldPosition, amount);
    }

    #endregion

    #region Deneyim ve seviye

    public void AddXp(int amount) {
        if (GameFlow.Instance.State == GameState.GameOver || GameFlow.Instance.State == GameState.MainMenu) return;

        Xp += Mathf.Max(1, Mathf.RoundToInt(amount * PlayerStats.Instance.XpMultiplier));

        while (Xp >= XpToNext) {
            Xp -= XpToNext;
            Level++;
            pendingLevelUps++;
            XpToNext = db.balance.GetXpToNextLevel(Level);
        }

        OnXpChanged?.Invoke(Xp, XpToNext, Level);
        TryOfferChoices();
    }

    private void TryOfferChoices() {
        if (isChoosing || pendingLevelUps <= 0 || GameFlow.Instance.State != GameState.Playing) return;

        currentChoices = PickRandomPerks(ChoiceCount);
        if (currentChoices.Count == 0) {
            pendingLevelUps = 0;
            return;
        }

        isChoosing = true;
        GameFlow.Instance.EnterLevelUp();
        OnPerkChoicesOffered?.Invoke(Level - pendingLevelUps + 1, currentChoices);
    }

    public void ChoosePerk(PerkDefinition perk) {
        if (!isChoosing) return;

        PlayerStats.Instance.AddPerk(perk);
        pendingLevelUps--;
        isChoosing = false;
        OnPerkChosen?.Invoke();

        GameFlow.Instance.ExitLevelUp(); // sıradaki seviye atlaması varsa Update'te sunulur
    }

    private List<PerkDefinition> PickRandomPerks(int count) {
        List<PerkDefinition> pool = new List<PerkDefinition>();
        foreach (PerkDefinition perk in db.perks) {
            if (PlayerStats.Instance.CanTake(perk)) {
                pool.Add(perk);
            }
        }

        List<PerkDefinition> result = new List<PerkDefinition>();
        while (result.Count < count && pool.Count > 0) {
            float totalWeight = 0f;
            foreach (PerkDefinition perk in pool) totalWeight += GetRarityWeight(perk.rarity);

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            for (int i = 0; i < pool.Count; i++) {
                roll -= GetRarityWeight(pool[i].rarity);
                if (roll <= 0f || i == pool.Count - 1) {
                    result.Add(pool[i]);
                    pool.RemoveAt(i);
                    break;
                }
            }
        }

        return result;
    }

    private static float GetRarityWeight(PerkRarity rarity) {
        switch (rarity) {
            case PerkRarity.Rare: return 30f;
            case PerkRarity.Epic: return 10f;
            default: return 60f;
        }
    }

    #endregion

    private void OnDestroy() {
        Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;
        Boss.OnAnyBossDefeated -= Boss_OnAnyBossDefeated;

        if (Player.Instance != null) {
            Player.Instance.GetMelee().OnParried -= Melee_OnParried;
        }
        if (WaveManager.Instance != null) {
            WaveManager.Instance.OnWaveCleared -= WaveManager_OnWaveCleared;
        }
    }
}
