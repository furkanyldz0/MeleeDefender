using System;
using System.Collections.Generic;
using UnityEngine;

// Silah + koşu içi perkler + kalıcı geliştirmelerden oluşan son değerler.
// Oyun kodu sabit sayılar yerine buradaki değerleri okur.
public class PlayerStats : MonoBehaviour {

    public static PlayerStats Instance { get; private set; }

    public event Action OnStatsChanged;
    public event Action<PerkDefinition> OnPerkAdded;

    private const int BaseSkillPointsRequired = 20;
    private const float BaseCritDamageMultiplier = 2f;

    public WeaponDefinition Weapon { get; private set; }
    public IReadOnlyList<PerkDefinition> AcquiredPerks => acquiredPerks;

    private readonly Dictionary<PerkType, int> perkStacks = new Dictionary<PerkType, int>();
    private readonly List<PerkDefinition> acquiredPerks = new List<PerkDefinition>(); // HUD'da sırayla göstermek için

    private GameDatabase db;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("Sahnede birden fazla PlayerStats var!");
        }
        Instance = this;

        db = GameDatabase.Instance;
        Weapon = db.GetWeapon(SaveSystem.Data.equippedWeapon);
    }

    public void EquipWeapon(string weaponId) {
        Weapon = db.GetWeapon(weaponId);
        SaveSystem.Data.equippedWeapon = Weapon.id;
        SaveSystem.Save();
        OnStatsChanged?.Invoke();
    }

    #region Perkler

    public int GetStacks(PerkType type) {
        return perkStacks.TryGetValue(type, out int stacks) ? stacks : 0;
    }

    public bool CanTake(PerkDefinition perk) {
        return GetStacks(perk.type) < perk.maxStacks;
    }

    public void AddPerk(PerkDefinition perk) {
        perkStacks[perk.type] = GetStacks(perk.type) + 1;

        if (!acquiredPerks.Contains(perk)) {
            acquiredPerks.Add(perk);
        }

        ApplyInstantEffect(perk);

        OnPerkAdded?.Invoke(perk);
        OnStatsChanged?.Invoke();
    }

    private void ApplyInstantEffect(PerkDefinition perk) {
        if (Base.Instance == null) return;

        switch (perk.type) {
            case PerkType.RepairBase:
                Base.Instance.HealPercent(perk.value);
                break;
            case PerkType.Fortify:
                Base.Instance.RefreshMaxHealth(healAddedAmount: true);
                break;
        }
    }

    private float PerkTotal(PerkType type) {
        int stacks = GetStacks(type);
        if (stacks == 0) return 0f;

        PerkDefinition perk = db.GetPerk(type);
        return perk != null ? perk.value * stacks : 0f;
    }

    #endregion

    private float MetaTotal(MetaUpgradeType type) {
        MetaUpgradeDefinition upgrade = db.GetUpgrade(type);
        if (upgrade == null) return 0f;

        return SaveSystem.GetUpgradeLevel(upgrade.id) * upgrade.valuePerLevel;
    }

    #region Hesaplanan değerler

    public float DamageMultiplier => Weapon.damageMultiplier * (1f + PerkTotal(PerkType.Damage)) * (1f + MetaTotal(MetaUpgradeType.Damage));
    public float SwingDuration => Mathf.Max(0.04f, Weapon.swingDuration * (1f - PerkTotal(PerkType.SwingSpeed)));
    public float ReachMultiplier => 1f + PerkTotal(PerkType.HitboxSize);
    public float ReflectSpeedMultiplier => Weapon.reflectSpeedMultiplier;
    public float CritChance => Mathf.Clamp01(Weapon.critChance + PerkTotal(PerkType.CritChance));
    public float CritDamageMultiplier => BaseCritDamageMultiplier + PerkTotal(PerkType.CritDamage);
    public int Pierce => Weapon.pierce + Mathf.RoundToInt(PerkTotal(PerkType.Pierce));
    public int ExtraProjectiles => Weapon.extraProjectiles + Mathf.RoundToInt(PerkTotal(PerkType.Split));
    public float MoveSpeedMultiplier => 1f + PerkTotal(PerkType.MoveSpeed);
    public int SkillPointsRequired => Mathf.Max(8, BaseSkillPointsRequired - Mathf.RoundToInt(PerkTotal(PerkType.Focus)));
    public float GoldMultiplier => (1f + PerkTotal(PerkType.GoldGain)) * (1f + MetaTotal(MetaUpgradeType.GoldGain));
    public float XpMultiplier => 1f + MetaTotal(MetaUpgradeType.XpGain);
    public float BaseHealthMultiplier => (1f + PerkTotal(PerkType.Fortify)) * (1f + MetaTotal(MetaUpgradeType.BaseHealth));
    public float LifestealPerKill => PerkTotal(PerkType.Lifesteal);
    public float StartingSkillCharge => Mathf.Clamp01(MetaTotal(MetaUpgradeType.StartingSkill));

    #endregion
}
