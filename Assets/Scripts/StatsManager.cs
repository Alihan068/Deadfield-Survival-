using System;
using System.Drawing;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class StatsManager : MonoBehaviour {

    public bool isPlayer;
    //public int playerIndex = 0;
    public bool canCollectItems = false;
    public bool canMove = true;
    public bool canBeDamaged = true;
    public bool canAttack = true;
    public bool isUnstoppable = false;
    public bool isKnocked;
    public bool triggersHitStop = false;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float dashCooldown = 1f;

    [Header("Base Stats")]
    public float baseHealth = 100f;
    public float currentHealth = 100f;
    public float extraHealth = 0f;
    public float maxHealth;
    public float baseRange = 1f;
    [Tooltip("Effects Knockback-KnockbackResistance")]

    public float armor = 1f;
    public float playerSize = 1f;
    public float haste = 1f;

    [Header("General Offensive")]
    public float attackSpeed = 1f;
    public float slowestAttackSPeedPerSecond = 5f;
    public float armorPen = 1f;
    public float evasion = 1f;
    public float explosionSize = 1f;
    public float explosionDamage = 1f;
    public float baseDamage = 1f;

    [Header("Knockback Attributes")]
    public float knockBack = 5f;
    public float baseAppliedKnockback = 10f;
    public float knockbackStagger = 0.15f;

    [Header("General Defensive")]
    public float damageReduction = 1f;
    public float stunResistance = 1f;
    public float debufResistance = 1f;

    [Header("Melee Attributes")]
    public float parryCooldown = 1f;
    public float weaponSize = 1f;

    [Header("Ranged Attributes")]
    public float projectileAmount = 1f;
    public float weaponBurst = 1f;
    public float projectileSpeed = 1f;
    public float projectileSize = 1f;
    public float spread = 1f;

    [Header("DOT Attributes")]
    public float burnDamage = 1f;
    public float poisonDamage = 1f;
    public float diseaseDamage = 1f;

    private float AdjustStat(float currentValue, float value, bool isPercentage, bool ifIncrease) {
        float modifier = ifIncrease ? 1f : -1f;

        if (isPercentage) {
            // Mevcut değerin yüzdesi kadar artış veya azalış uygular
            return currentValue * (1f + modifier * (value / 100f));
        }
        else {
            // Düz ekleme veya çıkarma
            return currentValue + (modifier * value);
        }
    }

    public void ApplyEffect(CollectibleItemSO.ItemEffect effect) {
        switch (effect.targetStat) {
            // Base Stats
            case TargetStat.currentHealth:
                currentHealth = AdjustStat(currentHealth, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.extraHealth:
                extraHealth = AdjustStat(extraHealth, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.maxHealth:
                maxHealth = AdjustStat(maxHealth, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.baseRange:
                baseRange = AdjustStat(baseRange, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.strength:
                knockBack = AdjustStat(knockBack, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.armor:
                armor = AdjustStat(armor, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.playerSize:
                playerSize = AdjustStat(playerSize, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.haste:
                haste = AdjustStat(haste, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            // General Offensive
            case TargetStat.armorPen:
                armorPen = AdjustStat(armorPen, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.dodgeChance:
                evasion = AdjustStat(evasion, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.explosionSize:
                explosionSize = AdjustStat(explosionSize, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.explosionDamage:
                explosionDamage = AdjustStat(explosionDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.baseDamage:
                baseDamage = AdjustStat(baseDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            // Knockback Attributes
            case TargetStat.baseAppliedKnockback:
                baseAppliedKnockback = AdjustStat(baseAppliedKnockback, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.knockbackStagger:
                knockbackStagger = AdjustStat(knockbackStagger, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            // General Defensive
            case TargetStat.damageReduction:
                damageReduction = AdjustStat(damageReduction, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.stunResistance:
                stunResistance = AdjustStat(stunResistance, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.debufResistance:
                debufResistance = AdjustStat(debufResistance, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.meleeAttackSpeed:
                attackSpeed = AdjustStat(attackSpeed, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.parryCooldown:
                parryCooldown = AdjustStat(parryCooldown, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.weaponSize:
                weaponSize = AdjustStat(weaponSize, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.slowestAttackSPeedPerSecond:
                slowestAttackSPeedPerSecond = AdjustStat(slowestAttackSPeedPerSecond, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.projectileAmount:
                projectileAmount = AdjustStat(projectileAmount, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.weaponBurst:
                weaponBurst = AdjustStat(weaponBurst, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.projectileSpeed:
                projectileSpeed = AdjustStat(projectileSpeed, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.projectileSize:
                projectileSize = AdjustStat(projectileSize, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.spread:
                spread = AdjustStat(spread, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            // DOT Attributes
            case TargetStat.burnDamage:
                burnDamage = AdjustStat(burnDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.poisonDamage:
                poisonDamage = AdjustStat(poisonDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;

            case TargetStat.diseaseDamage:
                diseaseDamage = AdjustStat(diseaseDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                break;
        }
    }



}
