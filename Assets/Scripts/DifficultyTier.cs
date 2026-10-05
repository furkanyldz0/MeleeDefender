using System.Collections.Generic;

[System.Serializable]
public class DifficultyTier
{
    public int requiredScore;
    public int totalEnemyCount;
    public float enemySpawnTimeOffset;
    public float enemyFirstAttackTime;
    public float bulletSpeed;

    public List<EnemyQuota> specialEnemies = new List<EnemyQuota>();
}

[System.Serializable]
public class EnemyQuota {
    public EnemyTypeSO type;
    public int count;
}
