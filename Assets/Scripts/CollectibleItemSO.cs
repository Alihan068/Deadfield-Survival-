using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;

public enum ItemRarity {
    common,
    //uncommon,
    rare,
    //epic,
    legendary,
    //mythic,
    unique,
}

public enum TargetStat {
    // Base Stats
    currentHealth,
    extraHealth,
    maxHealth,
    baseRange,
    strength,
    intelligence,
    vitality,
    armor,
    playerSize,
    haste,

    // General Offensive
    armorPen,
    dodgeChance,
    explosionSize,
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
    meleeDamage,
    meleeAttackSpeed,
    parryCooldown,
    weaponSize,
    slowestAttackSPeedPerSecond,

    // Ranged Attributes
    rangedSpeed,
    rangedDamage,
    projectileAmount,
    weaponBurst,
    projectileSpeed,
    projectileSize,
    spread,

    // DOT Attributes
    burnDamage,
    poisonDamage,
    diseaseDamage
}


[CreateAssetMenu(fileName = "New Item", menuName = "Create ItemSO")]
public class CollectibleItemSO : ScriptableObject {
    public ItemRarity itemRarity;

    [Header("Visuals")]
    public Sprite itemIcon;
    public string itemName;

    [Serializable]
    public class ItemEffect {
        public TargetStat targetStat;
        public float effectValue;
        public bool ifIncrease = true;
        public bool ifPercentage = false;
    }

    public List<ItemEffect> itemEffects = new List<ItemEffect>();
}

