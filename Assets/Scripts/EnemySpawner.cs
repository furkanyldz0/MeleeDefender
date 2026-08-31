using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnter : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;

    [SerializeField] private float horizontalSpace = 1.1f;
    [SerializeField] private float verticalPosition = 5f;
    [SerializeField] private Vector2 horizontalPositionRange = new Vector2(-4f, 4f);

    [SerializeField] private int enemySpawnCount = 10;
    [SerializeField] private int maxCurrentEnemyCount = 3;

    private Vector3[] enemyPositions;
    private Dictionary<Enemy, int> occupiedEnemyPositions = new Dictionary<Enemy, int>();

    int currentEnemyCount;


    private void Start() {
        //düþmanlar yatay uç deðerlerde de olabileceði için +1 eklemeden eksik kalýyor
        int totalEnemyCount = (int)((horizontalPositionRange.y - horizontalPositionRange.x) / horizontalSpace) + 1;
        Debug.Log(totalEnemyCount);

        enemyPositions = new Vector3[totalEnemyCount];
        float tempX = horizontalPositionRange.x;
        for(int i = 0; i < totalEnemyCount; i++) {
            enemyPositions[i] = new Vector3(tempX + i * horizontalSpace, 0f, verticalPosition);
        }
        CheckPositions();
    }
    private void CheckPositions() {
        if (currentEnemyCount >= maxCurrentEnemyCount || enemySpawnCount <= 0)
            return;

        List<int> availableIndexes = new List<int>();

        // 1. Önce SADECE haritadaki tüm boþ yerleri bul (Sayaçlarý burada ellemiyoruz)
        for (int i = 0; i < enemyPositions.Length; i++) {
            if (!occupiedEnemyPositions.ContainsValue(i)) {
                availableIndexes.Add(i);
            }
        }

        // 2. Kaç tane spawn yapabiliriz onu bul
        int enemyToSpawn = 0;
        while (enemySpawnCount > 0 && currentEnemyCount < maxCurrentEnemyCount && availableIndexes.Count > 0) {

            // 3. Boþ yerler listesinden rastgele bir sýra (index) seç
            int randomIndex = Random.Range(0, availableIndexes.Count);

            // O sýradaki asýl pozisyon numarasýný al (Örn: 5. pozisyon)
            int selectedPositionIndex = availableIndexes[randomIndex];

            // Düþmaný o pozisyonda yarat
            SpawnEnemy(selectedPositionIndex);

            // *** KRÝTÝK NOKTA *** 
            // Seçilen yeri müsaitler listesinden çýkar ki ayný yere iki düþman inmesin.
            // Bu iþlem senin ana enemyPositions dizini ASLA bozmaz, sadece geçici listeden siler.
            availableIndexes.RemoveAt(randomIndex);

            // 4. Sayaçlarý þimdi güncelle
            enemyToSpawn++;
            enemySpawnCount--;
            currentEnemyCount++;
        }
    }

    private void SpawnEnemy(int positionIndex) {
        var enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.IsSpawning = true;

        enemy.transform.DOMove(enemyPositions[positionIndex], 1f)
            .SetDelay(Random.Range(1f, 4f))
            .OnComplete(() => {
                enemy.IsSpawning = false;
            });

        occupiedEnemyPositions[enemy] = positionIndex;
        enemy.OnDied += Enemy_OnDied;
    }

    private void Enemy_OnDied(Enemy enemy) {
        enemy.OnDied -= Enemy_OnDied;

        if (occupiedEnemyPositions.TryGetValue(enemy, out int index)) {
            occupiedEnemyPositions.Remove(enemy);
            currentEnemyCount--;
            CheckPositions();
        }
    }




    //private void CheckPositions() {
    //    if(enemySpawnCount > 0 && currentEnemyCount < maxCurrentEnemyCount) {
    //        for (int i = 0; i < enemyPositions.Length; i++) {
    //            if (currentEnemyCount >= maxCurrentEnemyCount)
    //                return;

    //            if (!occupiedEnemyPositions.ContainsValue(i)) {
    //                SpawnEnemy(i);
    //                enemySpawnCount--;
    //                currentEnemyCount++;
    //            }
    //        }
    //    }
    //}
}
