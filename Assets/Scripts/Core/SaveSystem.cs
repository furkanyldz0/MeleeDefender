using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeLevel {
    public string id;
    public int level;
}

[Serializable]
public class LocalScore {
    public string name;
    public int score;
    public int wave;
}

[Serializable]
public class SaveData {
    // Ekonomi
    public int gold;
    public string equippedWeapon = "sword";
    public List<string> unlockedWeapons = new List<string> { "sword" };
    public List<UpgradeLevel> upgrades = new List<UpgradeLevel>();

    // Rekorlar ve istatistikler
    public int bestScore;
    public int bestWave;
    public int totalRuns;
    public int totalKills;
    public int totalParries;
    public int bossesDefeated;

    // Ayarlar
    public float masterVolume = 0.8f;
    public float sfxVolume = 1f;
    public bool screenShake = true;
    public bool damageNumbers = true;
    public int language = -1; // -1: sistem diline göre seç

    // Bir kerelik ipuçları
    public bool seenShieldTip;

    // Skor tablosu
    public string playerName = "";
    public string onlinePlayerIdentifier = ""; // LootLocker misafir oyuncusu; aynı oyuncu olarak kalmak için saklanır
    public List<LocalScore> localScores = new List<LocalScore>();
}

// Kayıt PlayerPrefs içinde JSON olarak tutulur, böylece WebGL'de de çalışır.
public static class SaveSystem {

    private const string SaveKey = "MeleeDefender.Save.v1";
    private const string LegacyBestScoreKey = "BestScore";

    private static SaveData data;

    public static SaveData Data {
        get {
            if (data == null) {
                Load();
            }
            return data;
        }
    }

    public static void Load() {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);

        if (!string.IsNullOrEmpty(json)) {
            try {
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e) {
                Debug.LogWarning("Kayıt okunamadı, sıfırdan başlanıyor: " + e.Message);
                data = null;
            }
        }

        if (data == null) {
            data = new SaveData();
            // Eski sürümdeki en iyi skoru kaybetme
            data.bestScore = PlayerPrefs.GetInt(LegacyBestScoreKey, 0);
        }

        if (data.unlockedWeapons == null || data.unlockedWeapons.Count == 0) {
            data.unlockedWeapons = new List<string> { "sword" };
        }
        if (data.upgrades == null) {
            data.upgrades = new List<UpgradeLevel>();
        }
        if (data.localScores == null) {
            data.localScores = new List<LocalScore>();
        }
        if (!data.unlockedWeapons.Contains(data.equippedWeapon)) {
            data.equippedWeapon = data.unlockedWeapons[0];
        }
    }

    public static void Save() {
        if (data == null) return;

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.SetInt(LegacyBestScoreKey, data.bestScore);
        PlayerPrefs.Save();
    }

    public static void ResetProgress() {
        // Ayarlar korunur, ilerleme silinir
        SaveData old = Data;
        data = new SaveData {
            playerName = old.playerName,
            onlinePlayerIdentifier = old.onlinePlayerIdentifier,
            masterVolume = old.masterVolume,
            sfxVolume = old.sfxVolume,
            screenShake = old.screenShake,
            damageNumbers = old.damageNumbers,
            language = old.language
        };
        Save();
    }

    public static int GetUpgradeLevel(string id) {
        foreach (UpgradeLevel u in Data.upgrades) {
            if (u.id == id) return u.level;
        }
        return 0;
    }

    public static void SetUpgradeLevel(string id, int level) {
        foreach (UpgradeLevel u in Data.upgrades) {
            if (u.id == id) {
                u.level = level;
                return;
            }
        }
        Data.upgrades.Add(new UpgradeLevel { id = id, level = level });
    }

    public static bool IsWeaponUnlocked(string id) {
        return Data.unlockedWeapons.Contains(id);
    }

    public static bool TrySpendGold(int amount) {
        if (Data.gold < amount) return false;

        Data.gold -= amount;
        Save();
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        data = null;
    }
}
