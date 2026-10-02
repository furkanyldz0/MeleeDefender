using UnityEngine;

[System.Serializable]
public class DifficultyTier
{
    public int requiredScore;
    public int enemySpawnCount;
    public float enemySpawnTimeOffset;
    public float enemyFirstAttackTime;
    public float bulletSpeed;
}
