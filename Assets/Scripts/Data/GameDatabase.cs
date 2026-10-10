using System;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponStyle {
    Sword,
    Katana,
    Axe,
    Spear,
    Daggers
}

public enum PerkType {
    Damage,
    HitboxSize,
    SwingSpeed,
    Split,
    Pierce,
    RepairBase,
    Fortify,
    MoveSpeed,
    Focus,
    CritChance,
    CritDamage,
    GoldGain,
    Lifesteal
}

public enum PerkRarity {
    Common,
    Rare,
    Epic
}

public enum MetaUpgradeType {
    BaseHealth,
    Damage,
    GoldGain,
    StartingSkill,
    XpGain
}

public enum BossKind {
    IronWarden,
    VortexSpire,
    EmberLord
}

[Serializable]
public class WeaponDefinition {
    public string id;
    public LocText displayName;
    public LocText description;
    public WeaponStyle style;
    public int cost;

    [Header("Savaş")]
    public float swingDuration = 0.1f;
    [Tooltip("x: dikey kalınlık, y: bıçak boyu (menzil), z: savuruş genişliği")]
    public Vector3 hitboxScale = Vector3.one;
    public float damageMultiplier = 1f;
    public float reflectSpeedMultiplier = 1f;
    [Range(0f, 1f)] public float critChance = 0.05f;
    public int pierce;
    public int extraProjectiles;

    [Header("Görsel")]
    public Color bladeColor = Color.white;
    public Color trailColor = Color.cyan;
}

[Serializable]
public class PerkDefinition {
    public PerkType type;
    public LocText displayName;
    public LocText description; // {0} yerine değer yazılır
    public string icon;
    public PerkRarity rarity;
    public float value;
    public int maxStacks = 5;

    public string GetDescription() {
        // 1'den küçük değerler yüzde olarak yazılır (0.15 -> 15)
        string formatted = value < 1f ? (value * 100f).ToString("0.#") : value.ToString("0.#");
        return string.Format(description.Get(), formatted);
    }
}

[Serializable]
public class MetaUpgradeDefinition {
    public string id;
    public MetaUpgradeType type;
    public LocText displayName;
    public LocText description; // {0} yerine seviye başı değer yazılır
    public string icon;
    public int maxLevel = 5;
    public int baseCost = 100;
    public float costGrowth = 1.6f;
    public float valuePerLevel = 0.1f;

    public int GetCost(int currentLevel) {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, currentLevel) / 5f) * 5;
    }

    public string GetDescription() {
        return string.Format(description.Get(), Mathf.RoundToInt(valuePerLevel * 100f));
    }
}

[Serializable]
public class BossDefinition {
    public string id;
    public BossKind kind;
    public LocText displayName;
    public LocText title;
    public float baseHealth = 2500f;
    public Color primaryColor = Color.gray;
    public Color accentColor = Color.red;
    public int goldReward = 100;
    public int xpReward = 25;
}

[Serializable]
public class BalanceSettings {
    [Header("Dalgalar")]
    public int bossEveryNWaves = 5;
    public int baseEnemiesPerWave = 6;
    public float enemiesPerWaveGrowth = 1.5f;
    public float enemyHealthGrowthPerWave = 0.07f;
    public float bossHealthGrowthPerCycle = 0.75f;
    public float timeBetweenWaves = 2f;
    [Tooltip("Her temizlenen dalgadan sonra kalenin onarılan oranı")]
    [Range(0f, 1f)] public float waveClearRepair = 0.05f;

    [Header("Boss mermileri")]
    public float bossBulletDamage = 8f;
    public float bossHeavyOrbDamage = 24f;

    [Header("Kalkanlı düşmanlar")]
    public int shieldFirstWave = 3;
    public float shieldChancePerWave = 0.08f;
    public float shieldMaxChance = 0.45f;
    public int shieldHits = 2;
    public float shieldRegenTime = 6f;

    [Header("Ekonomi")]
    public int goldPerKillScore = 4;
    public int waveClearGoldBase = 8;
    public int waveClearGoldPerWave = 2;

    [Header("Deneyim")]
    public int xpPerParry = 1;
    public int xpPerKillScore = 3;
    public int xpFirstLevel = 10;
    public float xpLinearGrowth = 7f;
    public float xpQuadraticGrowth = 1.5f;

    public int GetEnemyCountForWave(int wave) {
        return baseEnemiesPerWave + Mathf.FloorToInt(enemiesPerWaveGrowth * (wave - 1));
    }

    public float GetShieldChance(int wave) {
        if (wave < shieldFirstWave) return 0f;
        return Mathf.Min(shieldMaxChance, shieldChancePerWave * (wave - shieldFirstWave + 1));
    }

    public float GetEnemyHealthMultiplier(int wave) {
        return 1f + enemyHealthGrowthPerWave * (wave - 1);
    }

    public int GetWaveClearGold(int wave) {
        return waveClearGoldBase + waveClearGoldPerWave * wave;
    }

    public int GetXpToNextLevel(int level) {
        int n = level - 1;
        return Mathf.RoundToInt(xpFirstLevel + xpLinearGrowth * n + xpQuadraticGrowth * n * n);
    }
}

// Çevrimiçi skor tablosu (LootLocker). Anahtarlar boşsa oyun sadece cihazdaki skorları gösterir.
[Serializable]
public class OnlineSettings {
    [Tooltip("LootLocker panelinde Settings > API Keys altındaki Game API Key")]
    public string lootLockerGameKey = "";
    [Tooltip("Opsiyonel: LootLocker'ın verdiği domain key. Boşsa api.lootlocker.io kullanılır")]
    public string lootLockerDomainKey = "";
    [Tooltip("LootLocker panelinde oluşturulan Player tipindeki leaderboard'un key'i")]
    public string leaderboardKey = "";
    [Tooltip("Eski tip (dev_/prod_ ön eki olmayan) anahtarlarda test modunu belirler")]
    public bool developmentMode = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(lootLockerGameKey) && !string.IsNullOrWhiteSpace(leaderboardKey);
}

// Oyunun tüm denge verileri tek yerde.
// Düzenlemek için: Project penceresinde Create > Melee Defender > Game Database ile
// Assets/Resources/GameDatabase.asset oluştur (varsayılan değerlerle dolu gelir).
// Asset yoksa aşağıdaki varsayılanlar kullanılır.
[CreateAssetMenu(fileName = "GameDatabase", menuName = "Melee Defender/Game Database")]
public class GameDatabase : ScriptableObject {

    private const string ResourcePath = "GameDatabase";

    private static GameDatabase instance;

    public static GameDatabase Instance {
        get {
            if (instance == null) {
                instance = Resources.Load<GameDatabase>(ResourcePath);

                if (instance == null) {
                    instance = CreateInstance<GameDatabase>();
                    instance.FillDefaults();
                }
            }
            return instance;
        }
    }

    public BalanceSettings balance = new BalanceSettings();
    public OnlineSettings online = new OnlineSettings();
    public List<WeaponDefinition> weapons = new List<WeaponDefinition>();
    public List<PerkDefinition> perks = new List<PerkDefinition>();
    public List<MetaUpgradeDefinition> upgrades = new List<MetaUpgradeDefinition>();
    public List<BossDefinition> bosses = new List<BossDefinition>();

    public WeaponDefinition GetWeapon(string id) {
        foreach (WeaponDefinition w in weapons) {
            if (w.id == id) return w;
        }
        return weapons[0];
    }

    public PerkDefinition GetPerk(PerkType type) {
        foreach (PerkDefinition p in perks) {
            if (p.type == type) return p;
        }
        return null;
    }

    public MetaUpgradeDefinition GetUpgrade(MetaUpgradeType type) {
        foreach (MetaUpgradeDefinition u in upgrades) {
            if (u.type == type) return u;
        }
        return null;
    }

    // Inspector'daki "Reset" de varsayılanları yükler
    private void Reset() {
        FillDefaults();
    }

    public void FillDefaults() {
        balance = new BalanceSettings();
        online = new OnlineSettings();

        weapons = new List<WeaponDefinition> {
            new WeaponDefinition {
                id = "sword", style = WeaponStyle.Sword, cost = 0,
                displayName = new LocText("Uzun Kılıç", "Longsword"),
                description = new LocText("Dengeli ve güvenilir. Her şövalyenin ilk dostu.", "Balanced and reliable. Every knight's first friend."),
                swingDuration = 0.1f, hitboxScale = new Vector3(1f, 1f, 1f),
                damageMultiplier = 1f, reflectSpeedMultiplier = 1f, critChance = 0.05f,
                bladeColor = new Color(0.86f, 0.9f, 0.95f), trailColor = new Color(0.36f, 0.84f, 1f)
            },
            new WeaponDefinition {
                id = "katana", style = WeaponStyle.Katana, cost = 350,
                displayName = new LocText("Katana", "Katana"),
                description = new LocText("Şimşek hızında savuruş, yüksek kritik şansı.", "Lightning-fast swings and a high critical chance."),
                swingDuration = 0.075f, hitboxScale = new Vector3(0.9f, 1.1f, 0.9f),
                damageMultiplier = 0.95f, reflectSpeedMultiplier = 1.25f, critChance = 0.2f,
                bladeColor = new Color(0.95f, 0.97f, 1f), trailColor = new Color(1f, 0.3f, 0.43f)
            },
            new WeaponDefinition {
                id = "axe", style = WeaponStyle.Axe, cost = 600,
                displayName = new LocText("Savaş Baltası", "War Axe"),
                description = new LocText("Yavaş ama geniş. Yansıyan mermiler çok sert vurur.", "Slow but wide. Reflected shots hit like a truck."),
                swingDuration = 0.16f, hitboxScale = new Vector3(1.3f, 1.15f, 1.6f),
                damageMultiplier = 1.6f, reflectSpeedMultiplier = 0.85f, critChance = 0.05f,
                bladeColor = new Color(0.55f, 0.58f, 0.65f), trailColor = new Color(1f, 0.54f, 0.24f)
            },
            new WeaponDefinition {
                id = "spear", style = WeaponStyle.Spear, cost = 900,
                displayName = new LocText("Mızrak", "Spear"),
                description = new LocText("Uzun menzil. Yansıyan mermiler bir düşmanı deler geçer.", "Long reach. Reflected shots pierce through an enemy."),
                swingDuration = 0.12f, hitboxScale = new Vector3(0.9f, 1.6f, 0.9f),
                damageMultiplier = 1.1f, reflectSpeedMultiplier = 1.1f, critChance = 0.05f, pierce = 1,
                bladeColor = new Color(0.88f, 0.63f, 0.25f), trailColor = new Color(1f, 0.83f, 0.3f)
            },
            new WeaponDefinition {
                id = "daggers", style = WeaponStyle.Daggers, cost = 1300,
                displayName = new LocText("Çifte Hançer", "Twin Daggers"),
                description = new LocText("Kısa menzil, ama her savuşturma fazladan bir mermi yansıtır.", "Short reach, but every parry reflects an extra shot."),
                swingDuration = 0.065f, hitboxScale = new Vector3(1f, 0.8f, 1.2f),
                damageMultiplier = 0.75f, reflectSpeedMultiplier = 1.15f, critChance = 0.1f, extraProjectiles = 1,
                bladeColor = new Color(0.79f, 0.72f, 1f), trailColor = new Color(0.71f, 0.49f, 1f)
            },
        };

        perks = new List<PerkDefinition> {
            new PerkDefinition {
                type = PerkType.Damage, icon = "▲", rarity = PerkRarity.Common, value = 0.15f, maxStacks = 5,
                displayName = new LocText("Keskin Kenar", "Honed Edge"),
                description = new LocText("Yansıyan mermiler %{0} daha fazla hasar verir.", "Reflected shots deal {0}% more damage.")
            },
            new PerkDefinition {
                type = PerkType.HitboxSize, icon = "◄►", rarity = PerkRarity.Common, value = 0.15f, maxStacks = 4,
                displayName = new LocText("Geniş Savuruş", "Wide Arc"),
                description = new LocText("Kılıcın ve savuşturma alanın %{0} uzar.", "Your blade and parry reach grow by {0}%.")
            },
            new PerkDefinition {
                type = PerkType.SwingSpeed, icon = "»", rarity = PerkRarity.Common, value = 0.1f, maxStacks = 4,
                displayName = new LocText("Hızlı El", "Quick Hands"),
                description = new LocText("Savuruşlar %{0} daha hızlı.", "Swings are {0}% faster.")
            },
            new PerkDefinition {
                type = PerkType.Split, icon = "Ψ", rarity = PerkRarity.Epic, value = 1f, maxStacks = 3,
                displayName = new LocText("Çatallanma", "Fork"),
                description = new LocText("Her savuşturma +{0} ek mermi yansıtır.", "Each parry reflects +{0} extra shot.")
            },
            new PerkDefinition {
                type = PerkType.Pierce, icon = "→", rarity = PerkRarity.Rare, value = 1f, maxStacks = 3,
                displayName = new LocText("Delici", "Piercing"),
                description = new LocText("Yansıyan mermiler +{0} düşmanı deler.", "Reflected shots pierce +{0} enemy.")
            },
            new PerkDefinition {
                type = PerkType.RepairBase, icon = "♥", rarity = PerkRarity.Common, value = 0.35f, maxStacks = 99,
                displayName = new LocText("Onarım", "Repair"),
                description = new LocText("Kaleyi anında %{0} onar.", "Instantly repair {0}% of the castle.")
            },
            new PerkDefinition {
                type = PerkType.Fortify, icon = "■", rarity = PerkRarity.Rare, value = 0.2f, maxStacks = 4,
                displayName = new LocText("Sağlam Surlar", "Fortify"),
                description = new LocText("Kalenin azami canı %{0} artar.", "Castle max health +{0}%.")
            },
            new PerkDefinition {
                type = PerkType.MoveSpeed, icon = "►", rarity = PerkRarity.Common, value = 0.12f, maxStacks = 4,
                displayName = new LocText("Çeviklik", "Agility"),
                description = new LocText("Hareket hızı %{0} artar.", "Move speed +{0}%.")
            },
            new PerkDefinition {
                type = PerkType.Focus, icon = "◊", rarity = PerkRarity.Rare, value = 3f, maxStacks = 3,
                displayName = new LocText("Odak", "Focus"),
                description = new LocText("Yetenek {0} savuşturma daha erken dolar.", "Skill charges {0} parries sooner.")
            },
            new PerkDefinition {
                type = PerkType.CritChance, icon = "♦", rarity = PerkRarity.Rare, value = 0.08f, maxStacks = 5,
                displayName = new LocText("Keskin Göz", "Keen Eye"),
                description = new LocText("Kritik vuruş şansı +%{0}.", "Critical chance +{0}%.")
            },
            new PerkDefinition {
                type = PerkType.CritDamage, icon = "×", rarity = PerkRarity.Epic, value = 0.5f, maxStacks = 3,
                displayName = new LocText("Ölümcül", "Lethal"),
                description = new LocText("Kritik vuruşlar %{0} daha fazla hasar verir.", "Critical hits deal {0}% more damage.")
            },
            new PerkDefinition {
                type = PerkType.GoldGain, icon = "$", rarity = PerkRarity.Common, value = 0.2f, maxStacks = 3,
                displayName = new LocText("Altın Avcısı", "Treasure Hunter"),
                description = new LocText("Düşmanlardan %{0} daha fazla altın.", "{0}% more gold from enemies.")
            },
            new PerkDefinition {
                type = PerkType.Lifesteal, icon = "+", rarity = PerkRarity.Epic, value = 0.015f, maxStacks = 3,
                displayName = new LocText("Kan Bağı", "Siphon"),
                description = new LocText("Her öldürme kaleyi azami canın %{0}'i kadar onarır.", "Each kill repairs {0}% of castle max health.")
            },
        };

        upgrades = new List<MetaUpgradeDefinition> {
            new MetaUpgradeDefinition {
                id = "base_hp", type = MetaUpgradeType.BaseHealth, icon = "■", maxLevel = 5, baseCost = 120, costGrowth = 1.6f, valuePerLevel = 0.1f,
                displayName = new LocText("Kale Duvarları", "Castle Walls"),
                description = new LocText("Seviye başına kale canı +%{0}", "+{0}% castle health per level")
            },
            new MetaUpgradeDefinition {
                id = "damage", type = MetaUpgradeType.Damage, icon = "▲", maxLevel = 5, baseCost = 150, costGrowth = 1.6f, valuePerLevel = 0.06f,
                displayName = new LocText("Dövülmüş Çelik", "Tempered Steel"),
                description = new LocText("Seviye başına hasar +%{0}", "+{0}% damage per level")
            },
            new MetaUpgradeDefinition {
                id = "gold", type = MetaUpgradeType.GoldGain, icon = "$", maxLevel = 5, baseCost = 100, costGrowth = 1.7f, valuePerLevel = 0.1f,
                displayName = new LocText("Hazine Sandığı", "Fortune"),
                description = new LocText("Seviye başına altın kazancı +%{0}", "+{0}% gold gain per level")
            },
            new MetaUpgradeDefinition {
                id = "skill", type = MetaUpgradeType.StartingSkill, icon = "◊", maxLevel = 4, baseCost = 140, costGrowth = 1.6f, valuePerLevel = 0.25f,
                displayName = new LocText("Hazır Başla", "Head Start"),
                description = new LocText("Seviye başına yetenek %{0} dolu başlar", "Start with +{0}% skill charge per level")
            },
            new MetaUpgradeDefinition {
                id = "xp", type = MetaUpgradeType.XpGain, icon = "↑", maxLevel = 5, baseCost = 130, costGrowth = 1.6f, valuePerLevel = 0.1f,
                displayName = new LocText("Bilgelik", "Wisdom"),
                description = new LocText("Seviye başına deneyim kazancı +%{0}", "+{0}% experience per level")
            },
        };

        bosses = new List<BossDefinition> {
            new BossDefinition {
                id = "iron_warden", kind = BossKind.IronWarden, baseHealth = 2500f, goldReward = 120, xpReward = 25,
                displayName = new LocText("Demir Muhafız", "Iron Warden"),
                title = new LocText("Kapıların Bekçisi", "Keeper of the Gates"),
                primaryColor = new Color(0.49f, 0.53f, 0.6f), accentColor = new Color(1f, 0.55f, 0.18f)
            },
            new BossDefinition {
                id = "vortex_spire", kind = BossKind.VortexSpire, baseHealth = 3200f, goldReward = 180, xpReward = 35,
                displayName = new LocText("Girdap Kulesi", "Vortex Spire"),
                title = new LocText("Fırtınanın Gözü", "Eye of the Storm"),
                primaryColor = new Color(0.29f, 0.23f, 0.55f), accentColor = new Color(0.22f, 0.88f, 1f)
            },
            new BossDefinition {
                id = "ember_lord", kind = BossKind.EmberLord, baseHealth = 4000f, goldReward = 250, xpReward = 45,
                displayName = new LocText("Kor Lordu", "Ember Lord"),
                title = new LocText("Küllerin Hükümdarı", "Sovereign of Ash"),
                primaryColor = new Color(0.35f, 0.1f, 0.1f), accentColor = new Color(1f, 0.35f, 0.12f)
            },
        };
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        instance = null;
    }
}
