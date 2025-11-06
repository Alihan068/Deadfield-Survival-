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
    extraHealth,
    dashCooldown,
    jumpCooldown,
    strength,
    intelligence,
    moveSpeed,
    vitality,
    armor,
    playerSize,
    haste,
    armorPen,
    dodgeChance,
    explosionSize,
    explosionDamage,
    weaponDamage,
    damageReduction,
    stunResistance,
    debufResistance,
    meleeDamage,
    meleeSpeed,
    parryCooldown,
    weaponSize,
    weaponRange,
    rangedSpeed,
    rangedDamage,
    projectileAmount,
    weaponBurst,
    projectileBounce,
    projectileSize,
    spread,
    burnDamage,
    poisonDamage,
    diseaseDamage,
}



[CreateAssetMenu(fileName = "New Item", menuName = "Create ItemSO")]
public class CollectibleItemSO : ScriptableObject
{
    //[SerializeField] SpriteRenderer spriteRenderer;

    public ItemRarity itemRarity;
    [Serializable]   
    public class ItemEffect {
        public TargetStat targetStat;   
        public float effectValue;
        public bool ifIncrease = true;
        public bool ifPercentage = false;
    }
  
    public List<ItemEffect> itemEffects = new List<ItemEffect>();
    StatsManager playerStatsManager;

    
    
}

