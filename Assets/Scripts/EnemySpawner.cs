using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public event Action OnWaveCleared;

    [SerializeField] private EnemyTypeSO normalEnemyType;

    [SerializeField] private float horizontalSpace = 1.1f;
    [SerializeField] private float verticalPosition = 7f;
    [SerializeField] private Vector2 horizontalPositionRange = new Vector2(-4f, 4f);

    public EnemyTypeSO NormalEnemyType => normalEnemyType;
    public float EnemyHealthMultiplier { get; set; } = 1f;
    public float ShieldChance { get; set; } // 0-1, WaveManager dalgaya göre ayarlar
    public int RemainingInWave => enemySpawnCount + currentEnemyCount;

    // Bu dalgada daha doğacak düşman sayısı (WaveManager belirler)
    private int enemySpawnCount;

    private int maxCurrentEnemyCount = 2;
    private float enemySpawnTimeOffset = 0.5f;
    private float enemyFirstAttackTime = 1f;

    private Vector3[] enemyPositions;
    private Dictionary<Enemy, int> occupiedEnemyPositions = new Dictionary<Enemy, int>();

    private int currentEnemyCount;
    private bool isWaveRunning;

    private List<EnemyQuota> currentQuotas = new List<EnemyQuota>();


    private void Awake() {
        CalculateEnemySpace();
    }

    private void Start() {
        LevelManager.Instance.OnDifficultyChanged += LevelManager_OnDifficultyChanged;
        if (LevelManager.Instance.CurrentDifficultyTier != null) {
            LevelManager_OnDifficultyChanged(LevelManager.Instance.CurrentDifficultyTier);
        }
    }

    public void StartWave(int enemyCount) {
        enemySpawnCount = enemyCount;
        isWaveRunning = true;
        CheckPositions();
    }

    public void StopSpawning() {
        enemySpawnCount = 0;
        isWaveRunning = false;
    }

    // Boss gibi dalga kotası dışındaki çağrılar için (kotaya sayılmaz)
    public int SpawnExtra(int count, int maxAlive) {
        List<int> availableIndexes = GetAvailableIndexes();
        int spawned = 0;

        while (spawned < count && currentEnemyCount < maxAlive && availableIndexes.Count > 0) {
            int randomIndex = UnityEngine.Random.Range(0, availableIndexes.Count);
            SpawnEnemy(availableIndexes[randomIndex], normalEnemyType);
            availableIndexes.RemoveAt(randomIndex);

            currentEnemyCount++;
            spawned++;
        }

        return spawned;
    }

    public void KillAllEnemies() {
        // Sözlük ölüm olaylarında değişeceği için kopyasını dolaş
        List<Enemy> alive = new List<Enemy>(occupiedEnemyPositions.Keys);
        foreach (Enemy enemy in alive) {
            if (enemy != null) {
                enemy.Kill();
            }
        }
    }

    private void LevelManager_OnDifficultyChanged(DifficultyTier difficultyTier) {
        maxCurrentEnemyCount = difficultyTier.totalEnemyCount;
        enemySpawnTimeOffset = difficultyTier.enemySpawnTimeOffset;
        enemyFirstAttackTime = difficultyTier.enemyFirstAttackTime;
        currentQuotas = difficultyTier.specialEnemies;

        int totalQuota = 0;
        foreach (var q in difficultyTier.specialEnemies) totalQuota += q.count;
        if (totalQuota > difficultyTier.totalEnemyCount)
            Debug.LogWarning($"Tier {difficultyTier.requiredScore}: Özel düşman kotaları toplam sayıyı aşıyor!");

        CheckPositions();
    }

    private void Enemy_OnAnyEnemyDied(object sender, Enemy.OnAnyEnemyDiedEventArgs e) {
        if(sender is Enemy enemy) {
            if (occupiedEnemyPositions.TryGetValue(enemy, out int index)) {
                occupiedEnemyPositions.Remove(enemy);
                currentEnemyCount--;
                CheckPositions();

                if (isWaveRunning && enemySpawnCount <= 0 && currentEnemyCount <= 0) {
                    isWaveRunning = false;
                    OnWaveCleared?.Invoke();
                }
            }
        }
    }

    private void CalculateEnemySpace() {
        //düşmanlar yatay uç değerlerde de olabileceği için +1 eklemeden eksik kalıyor
        int totalEnemyCount = (int)((horizontalPositionRange.y - horizontalPositionRange.x) / horizontalSpace) + 1;

        enemyPositions = new Vector3[totalEnemyCount];
        float tempX = horizontalPositionRange.x;
        for (int i = 0; i < totalEnemyCount; i++) {
            enemyPositions[i] = new Vector3(tempX + i * horizontalSpace, 0f, verticalPosition);
        }
    }

    private List<int> GetAvailableIndexes() {
        List<int> availableIndexes = new List<int>();

        for (int i = 0; i < enemyPositions.Length; i++) {
            if (!occupiedEnemyPositions.ContainsValue(i)) {
                availableIndexes.Add(i);
            }
        }

        return availableIndexes;
    }

    private void CheckPositions() {
        if (!isWaveRunning || currentEnemyCount >= maxCurrentEnemyCount || enemySpawnCount <= 0)
            return;

        // 1. Önce SADECE haritadaki tüm boş yerleri bul (Sayaçları burada ellemiyoruz)
        List<int> availableIndexes = GetAvailableIndexes();

        // 2. Kaç tane spawn yapabiliriz onu bul
        while (enemySpawnCount > 0 && currentEnemyCount < maxCurrentEnemyCount && availableIndexes.Count > 0) {

            // 3. Boş yerler listesinden rastgele bir sıra (index) seç
            int randomIndex = UnityEngine.Random.Range(0, availableIndexes.Count);

            // O sıradaki asıl pozisyon numarasını al (örn: 5. pozisyon)
            int selectedPositionIndex = availableIndexes[randomIndex];

            // Düşmanı o pozisyonda yarat
            SpawnEnemy(selectedPositionIndex, PickTypeToSpawn());

            // Seçilen yeri müsaitler listesinden çıkar ki aynı yere iki düşman inmesin.
            availableIndexes.RemoveAt(randomIndex);

            // 4. Sayaçları şimdi güncelle
            enemySpawnCount--;
            currentEnemyCount++;
        }
    }

    private void SpawnEnemy(int positionIndex, EnemyTypeSO type) {
        var enemy = Instantiate(type.prefab, transform.position, Quaternion.identity);
        enemy.TypeData = type;
        enemy.IsSpawning = true;
        enemy.Health *= EnemyHealthMultiplier;
        enemy.gameObject.AddComponent<EnemyVisual>();
        if (UnityEngine.Random.value < ShieldChance) {
            enemy.gameObject.AddComponent<EnemyShield>();
        }

        enemy.transform.DOMove(enemyPositions[positionIndex], 1f)
            .SetDelay(UnityEngine.Random.Range(.5f + enemySpawnTimeOffset, 1.5f + enemySpawnTimeOffset * 3)) //1-4
            .SetEase(Ease.OutCubic)
            .SetLink(enemy.gameObject)
            .OnComplete(() => {
                enemy.Setup(false, enemyFirstAttackTime);
            });

        occupiedEnemyPositions[enemy] = positionIndex;
    }

    private EnemyTypeSO PickTypeToSpawn() {
        EnemyTypeSO best = null;
        int bestDeficit = 0;

        foreach (var quota in currentQuotas) {
            int deficit = quota.count - CountAlive(quota.type);
            if (deficit > bestDeficit) {
                bestDeficit = deficit;
                best = quota.type;
            }
        }

        return best != null ? best : normalEnemyType;
    }

    private int CountAlive(EnemyTypeSO type) {
        int count = 0;
        foreach (var enemy in occupiedEnemyPositions.Keys) {
            if (enemy.TypeData == type) count++;
        }
        return count;
    }

    private void OnEnable() => Enemy.OnAnyEnemyDied += Enemy_OnAnyEnemyDied;
    private void OnDisable() => Enemy.OnAnyEnemyDied -= Enemy_OnAnyEnemyDied;
    private void OnDestroy() {
        // Obje silinirken aboneliği kesinlikle iptal et!
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnDifficultyChanged -= LevelManager_OnDifficultyChanged;
        }
    }

}
