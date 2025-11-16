using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemRarity {
    common,
    uncommon,
    rare,
    epic,
    legendary,
    mythic,
    unique,
}

public enum TargetStat {
    // Base Stats
    currentHealth,
    extraHealth,
    maxHealth,
    baseRange,
    strength,
    armor,
    playerSize,
    haste,
    luck,

    // General Offensive
    evasion,
    xpMultiplier,
    explosionDamage,
    baseDamage,

    // Knockback Attributes
    baseAppliedKnockback,
    knockbackStagger,

    // General Defensive
    damageReduction,
    stunResistance,
    debufResistance,

    // Melee Attributes
    meleeSwipeAngle,
    attackSpeed,
    parryCooldown,
    size,                     // Global / effect size (StatsManager.effectSize)
    slowestAttackSPeedPerSecond,

    // Ranged Attributes
    projectileAmount,
    weaponBurst,
    projectileSpeed,
    spread,

    // DOT Attributes
    burnDamage,
    poisonDamage,
    diseaseDamage,

    // Enemy Difficulty Attributes
    enemyDamageMultiplier,
    enemyHealthMultiplier,
    enemySpeedMultiplier,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Create ItemSO")]
public class CollectibleItemSO : ScriptableObject {
    public ItemRarity itemRarity;

    [Header("Visuals")]
    public Sprite itemIcon;
    [TextArea]
    public string description;
    [Tooltip("Default color for rarity-based visuals (particles, outline, etc.)")]
    public Color rarityColor = Color.white;

    [Header("Loot Defaults")]
    [Tooltip("Base weight for this item when added to loot tables.")]
    public float baseDropWeight = 1f;

    [Tooltip("If false, the ItemPrefabGenerator will skip prefab generation for this item.")]
    public bool generateDefaultPrefab = true;

    [Header("Audio")]
    [Tooltip("Optional pickup SFX played when this item is collected.")]
    public AudioClip pickupSfx;

    [Serializable]
    public class ItemEffect {
        public TargetStat targetStat;
        public float effectValue;
        public bool ifIncrease = true;
        public bool ifPercentage = false;

        [Tooltip("Optional label shown in UI instead of raw TargetStat name. Leave empty to use enum name.")]
        public string customLabel;
    }

    public List<ItemEffect> itemEffects = new List<ItemEffect>();

     void OnValidate() {
        if (baseDropWeight < 0f)
            baseDropWeight = 0f;

        if (itemEffects != null) {
            foreach (var effect in itemEffects) {
                if (effect == null) continue;

                if (effect.ifPercentage) {
                    effect.effectValue = Mathf.Clamp(effect.effectValue, -1000f, 1000f);
                }
            }
        }
    }
}
