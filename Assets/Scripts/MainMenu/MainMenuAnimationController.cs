using UnityEngine;
using DG.Tweening;

public class MainMenuAnimationController : MonoBehaviour {
    public enum SpawnFrequency { Rare, LessRare, Normal, Frequent }

    [Header("Referanslar")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Ayarlar")]
    [SerializeField] private SpawnFrequency spawnFrequency = SpawnFrequency.Normal;
    [SerializeField] private float moveDuration = 4f;
    [SerializeField] private float targetZ = 6f;

    private float[] spawnTimers;

    private void Start() {
        spawnTimers = new float[spawnPoints.Length];

        for (int i = 0; i < spawnTimers.Length; i++) {
            spawnTimers[i] = GetRandomSpawnTime();
        }
    }

    private void Update() {
        for (int i = 0; i < spawnTimers.Length; i++) {
            spawnTimers[i] -= Time.deltaTime;

            if (spawnTimers[i] <= 0f) {
                SpawnAndMoveEnemy(spawnPoints[i]);
                spawnTimers[i] = GetRandomSpawnTime();
            }
        }
    }

    private float GetRandomSpawnTime() {
        switch (spawnFrequency) {
            case SpawnFrequency.Rare: return Random.Range(3f, 5f);
            case SpawnFrequency.LessRare: return Random.Range(2f, 6f);
            case SpawnFrequency.Normal: return Random.Range(1.5f, 3f);
            case SpawnFrequency.Frequent: return Random.Range(0.2f, 1.5f);
            default: return 2f;
        }
    }

    private void SpawnAndMoveEnemy(Transform spawnPoint) {
        if (enemyPrefabs.Length == 0) return;

        GameObject selectedPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject spawnedEnemy = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);

        spawnedEnemy.transform.DOMoveZ(targetZ, moveDuration)
            .SetEase(Ease.Linear)
            .SetLink(spawnedEnemy)
            .OnComplete(() => {
                Destroy(spawnedEnemy);
            });
    }

    private void OnDestroy() {
        transform.DOKill();
    }
}