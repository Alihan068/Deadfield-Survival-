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
    public float jumpCooldown = 1f;

    [Header("Base Stats")]
    public float currentHp = 100;
    public float extraHealth = 1f;
    public float baseRange = 1f;
    [Tooltip("Effects Knockback-KnockbackResistance")]
    public float strength = 5f;
    public float intelligence = 2f; 
    public float vitality = 5f;
    public float armor = 1f;
    public float playerSize = 1f;
    public float haste = 1f;
    [Header("General Offensive")]
    public float armorPen = 1f;
    public float dodgeChance = 1f;
    public float explosionSize = 1f;
    public float explosionDamage = 1f;
    public float baseDamage = 1f;
    [Header("Knockback Attributes")]
    public float baseAppliedKnockback = 10f;
    public float knockbackStagger = 0.15f;

    [Header("General Defensive")]
    public float damageReduction = 1f;
    public float stunResistance = 1f;
    public float debufResistance = 1f;

    [Header("Melee Attributes")]
    public float meleeSwipeAngle = 1f;
    public float meleeDamage = 1f;
    public float meleeAttackSpeed = 1f;
    public float parryCooldown = 1f;
    public float weaponSize = 1f;
    public float slowestAttackSPeedPerSecond = 5f;
    [Header("Ranged Attributes")]
    public float rangedSpeed = 1f;
    public float rangedDamage = 1f;
    public float projectileAmount = 1f;
    public float weaponBurst = 1f;
    public float projectileSpeed = 1f;
    public float projectileSize = 1f;
    public float spread = 1f;

    [Header("DOT Attributes")]
    public float burnDamage = 1f;
    public float poisonDamage = 1f;
    public float diseaseDamage = 1f;

     float ModifyStatBasedOnvariables(float stat, float value, bool ifIncrease, bool ifPercentage) {
        float calculation;
        if (!ifIncrease) value = -value;

        if (ifPercentage) {
            calculation = stat + (value / stat) * 100;
            return calculation;
        }
        else {
            calculation = stat + value;
            return calculation;
        }
    }

    public void ApplyEffect(CollectibleItemSO.ItemEffect effect) {
        Debug.Log("Apply Effects");
        switch (effect.targetStat) {
            case TargetStat.extraHealth:
                extraHealth = ModifyStatBasedOnvariables(extraHealth, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.dashCooldown:
                dashCooldown = ModifyStatBasedOnvariables(dashCooldown, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.jumpCooldown:
                jumpCooldown = ModifyStatBasedOnvariables(jumpCooldown, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.strength:
                strength = ModifyStatBasedOnvariables(strength, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.intelligence:
                intelligence = ModifyStatBasedOnvariables(intelligence, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.moveSpeed:
                moveSpeed = ModifyStatBasedOnvariables(moveSpeed, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.vitality:
                vitality = ModifyStatBasedOnvariables(vitality, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.armor:
                armor = ModifyStatBasedOnvariables(armor, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.playerSize:
                playerSize = ModifyStatBasedOnvariables(playerSize, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.haste:
                haste = ModifyStatBasedOnvariables(haste, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.armorPen:
                armorPen = ModifyStatBasedOnvariables(armorPen, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.dodgeChance:
                dodgeChance = ModifyStatBasedOnvariables(dodgeChance, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.explosionSize:
                explosionSize = ModifyStatBasedOnvariables(explosionSize, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.explosionDamage:
                explosionDamage = ModifyStatBasedOnvariables(explosionDamage, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.weaponDamage:
                baseDamage = ModifyStatBasedOnvariables(baseDamage, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.damageReduction:
                damageReduction = ModifyStatBasedOnvariables(damageReduction, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.stunResistance:
                stunResistance = ModifyStatBasedOnvariables(stunResistance, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.debufResistance:
                debufResistance = ModifyStatBasedOnvariables(debufResistance, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.meleeDamage:
                meleeDamage = ModifyStatBasedOnvariables(meleeDamage, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.meleeSpeed:
                meleeAttackSpeed = ModifyStatBasedOnvariables(meleeAttackSpeed, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.parryCooldown:
                parryCooldown = ModifyStatBasedOnvariables(parryCooldown, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.weaponSize:
                weaponSize = ModifyStatBasedOnvariables(weaponSize, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.weaponRange:
                baseRange = ModifyStatBasedOnvariables(baseRange, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.rangedSpeed:
                rangedSpeed = ModifyStatBasedOnvariables(rangedSpeed, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.rangedDamage:
                rangedDamage = ModifyStatBasedOnvariables(rangedDamage, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.projectileAmount:
                projectileAmount = ModifyStatBasedOnvariables(projectileAmount, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.weaponBurst:
                weaponBurst = ModifyStatBasedOnvariables(weaponBurst, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.projectileBounce:
                projectileSpeed = ModifyStatBasedOnvariables(projectileSpeed, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.projectileSize:
                projectileSize = ModifyStatBasedOnvariables(projectileSize, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.spread:
                spread = ModifyStatBasedOnvariables(spread, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.burnDamage:
                burnDamage = ModifyStatBasedOnvariables(burnDamage, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.poisonDamage:
                poisonDamage = ModifyStatBasedOnvariables(poisonDamage, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            case TargetStat.diseaseDamage:
                diseaseDamage = ModifyStatBasedOnvariables(diseaseDamage, effect.effectValue, effect.ifIncrease, effect.ifPercentage);
                break;
            default:
                Debug.Log("Something is not right");
                break;
        }
    }



}
