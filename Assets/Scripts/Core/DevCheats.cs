using UnityEngine;

// Sadece Editor ve Development Build'de çalışan test kısayolları.
// F1: seviye atla   F2: sonraki boss dalgası   F3: +500 altın   F4: kaleyi onar   F5: düşmanları öldür
public class DevCheats : MonoBehaviour {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update() {
        if (GameFlow.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.F3)) {
            SaveSystem.Data.gold += 500;
            SaveSystem.Save();
            Debug.Log("[Hile] +500 altın");
        }

        if (!GameFlow.Instance.IsPlaying) return;

        if (Input.GetKeyDown(KeyCode.F1)) {
            LevelUp();
        }
        if (Input.GetKeyDown(KeyCode.F2)) {
            JumpToNextBoss();
        }
        if (Input.GetKeyDown(KeyCode.F4) && Base.Instance != null) {
            Base.Instance.HealPercent(1f);
        }
        if (Input.GetKeyDown(KeyCode.F5)) {
            KillEverything();
        }
    }
#endif

    public static void LevelUp() {
        RunProgression.Instance.AddXp(RunProgression.Instance.XpToNext - RunProgression.Instance.Xp);
    }

    public static void JumpToNextBoss() {
        int every = GameDatabase.Instance.balance.bossEveryNWaves;
        int nextBoss = (WaveManager.Instance.CurrentWave / every + 1) * every;
        WaveManager.Instance.DebugJumpToWave(nextBoss);
    }

    public static void KillEverything() {
        FindFirstObjectByType<EnemySpawner>().KillAllEnemies();

        Boss boss = WaveManager.Instance.CurrentBoss;
        if (boss != null && boss.IsActive) {
            boss.Damage(boss.Health + 1f);
        }
    }
}
