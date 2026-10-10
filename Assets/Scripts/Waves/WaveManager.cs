using System;
using System.Collections;
using UnityEngine;

// Dalga akışı: normal dalgalar düşman kotası dolunca biter,
// her N. dalgada bir boss gelir. Boss listesi bitince daha güçlü bir döngüyle baştan başlar.
public class WaveManager : MonoBehaviour {

    public static WaveManager Instance { get; private set; }

    public event Action<int, bool> OnWaveStarted;   // dalga, boss dalgası mı
    public event Action<int> OnWaveCleared;
    public event Action<int> OnWaveAnnounced;       // dalga başlamadan önceki duyuru
    public event Action<Boss> OnBossSpawned;

    private const float AnnounceDuration = 1.8f;
    private const float BossSpawnZ = 16f;
    // Kamera açısı yüzünden daha geride duran bir boss ekranın üstünden taşar
    private const float BossHomeZ = 5.6f;

    public int CurrentWave { get; private set; }
    public bool IsWaveActive { get; private set; }
    public bool IsBossWave => IsBossWaveNumber(CurrentWave);
    public Boss CurrentBoss { get; private set; }

    private EnemySpawner spawner;
    private GameDatabase db;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla WaveManager var!");
        }
        Instance = this;

        db = GameDatabase.Instance;
    }

    private void Start() {
        spawner = FindFirstObjectByType<EnemySpawner>();
        spawner.OnWaveCleared += Spawner_OnWaveCleared;
        Boss.OnAnyBossDefeated += Boss_OnAnyBossDefeated;
        GameFlow.Instance.OnRunStarted += GameFlow_OnRunStarted;
        GameFlow.Instance.OnRunEnded += GameFlow_OnRunEnded;
    }

    private void GameFlow_OnRunStarted() {
        CurrentWave = 0;
        StartCoroutine(StartNextWave(0.6f));
    }

    private void GameFlow_OnRunEnded() {
        StopAllCoroutines();
        IsWaveActive = false;
        spawner.StopSpawning();
    }

    public bool IsBossWaveNumber(int wave) {
        return wave > 0 && wave % db.balance.bossEveryNWaves == 0;
    }

    public int EnemiesRemaining => IsWaveActive && !IsBossWave ? spawner.RemainingInWave : 0;

    private IEnumerator StartNextWave(float delay) {
        yield return new WaitForSeconds(delay);

        CurrentWave++;
        OnWaveAnnounced?.Invoke(CurrentWave);

        yield return new WaitForSeconds(AnnounceDuration);

        IsWaveActive = true;
        bool isBoss = IsBossWaveNumber(CurrentWave);

        if (isBoss) {
            SpawnBoss();
        }
        else {
            spawner.EnemyHealthMultiplier = db.balance.GetEnemyHealthMultiplier(CurrentWave);
            spawner.ShieldChance = db.balance.GetShieldChance(CurrentWave);
            spawner.StartWave(db.balance.GetEnemyCountForWave(CurrentWave));
        }

        OnWaveStarted?.Invoke(CurrentWave, isBoss);
    }

    private void SpawnBoss() {
        int bossIndex = CurrentWave / db.balance.bossEveryNWaves - 1;
        int cycle = bossIndex / db.bosses.Count;
        BossDefinition definition = db.bosses[bossIndex % db.bosses.Count];

        float healthMultiplier = 1f + db.balance.bossHealthGrowthPerCycle * cycle;
        Vector3 spawnPosition = new Vector3(0f, 0f, BossSpawnZ);
        Vector3 homePosition = new Vector3(0f, 0f, BossHomeZ);

        spawner.EnemyHealthMultiplier = db.balance.GetEnemyHealthMultiplier(CurrentWave);
        spawner.ShieldChance = db.balance.GetShieldChance(CurrentWave);
        CurrentBoss = Boss.Create(definition, healthMultiplier, 1f + cycle * 0.5f, spawner, spawnPosition, homePosition);
        OnBossSpawned?.Invoke(CurrentBoss);
    }

    private void Spawner_OnWaveCleared() {
        if (!IsWaveActive || IsBossWave) return;
        CompleteWave();
    }

    private void Boss_OnAnyBossDefeated(Boss boss) {
        if (boss != CurrentBoss) return;

        CurrentBoss = null;
        spawner.KillAllEnemies(); // bossun çağırdığı yardımcılar da ölür
        CompleteWave();
    }

    private void CompleteWave() {
        if (GameFlow.Instance.State == GameState.GameOver) return;

        IsWaveActive = false;
        OnWaveCleared?.Invoke(CurrentWave);
        SaveSystem.Save();

        StartCoroutine(StartNextWave(db.balance.timeBetweenWaves));
    }

    // Sadece test/geliştirme için (DevCheats): mevcut dalgayı iptal edip istenen dalgaya atlar
    public void DebugJumpToWave(int wave) {
        StopAllCoroutines();
        IsWaveActive = false;
        spawner.StopSpawning();
        spawner.KillAllEnemies();

        if (CurrentBoss != null) {
            Destroy(CurrentBoss.gameObject);
            CurrentBoss = null;
        }

        CurrentWave = Mathf.Max(0, wave - 1);
        StartCoroutine(StartNextWave(0.2f));
    }

    private void OnDestroy() {
        Boss.OnAnyBossDefeated -= Boss_OnAnyBossDefeated;

        if (spawner != null) {
            spawner.OnWaveCleared -= Spawner_OnWaveCleared;
        }
        if (GameFlow.Instance != null) {
            GameFlow.Instance.OnRunStarted -= GameFlow_OnRunStarted;
            GameFlow.Instance.OnRunEnded -= GameFlow_OnRunEnded;
        }
    }
}
