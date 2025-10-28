using System;
using System.Drawing;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PlayerStatsManager : MonoBehaviour {

    public int playerIndex = 0;

    [Header("Movement")]
    public float dashCooldown = 1f;
    public float jumpCooldown;

    [Header("Base Stats")]
    public float strength = 5f;
    public float intelligence = 2f;
    public float moveSpeed = 5f;
    public float vitality = 5f;
    public float armor = 1f;
    public float playerSize;
    public float haste;

    [Header("General Offensive")]
    public float armorPen;
    public float dodgeChance;
    public float explosionSize;
    public float explosionDamage;
    public float weaponDamage;
    [Header("General Defensive")]
    public float damageReduction;
    public float stunResistance;
    public float debufResistance;

    [Header("Melee Attributes")]
    public float meleeDamage;
    public float meleeSpeed;
    public float parryCooldown;
    public float weaponSize;

    [Header("Ranged Attributes")]
    public float weaponRange;
    public float rangedSpeed;
    public float rangedDamage;
    public float projectileAmount;
    public float weaponBurst;
    public float projectileBounce;
    public float projectileSize;
    public float spread;

    [Header("DOT Attributes")]
    public float burnDamage;
    public float poisonDamage;
    public float diseaseDamage;
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
            case TargetStat.dashCooldown:
                dashCooldown = ModifyStatBasedOnvariables(dashCooldown, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.jumpCooldown:
                jumpCooldown = ModifyStatBasedOnvariables(jumpCooldown, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.strength:
                strength = ModifyStatBasedOnvariables(strength, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.intelligence:
                intelligence = ModifyStatBasedOnvariables(intelligence, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.moveSpeed:
                moveSpeed = ModifyStatBasedOnvariables(moveSpeed, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.vitality:
                vitality = ModifyStatBasedOnvariables(vitality, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.armor:
                armor = ModifyStatBasedOnvariables(armor, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.playerSize:
                playerSize = ModifyStatBasedOnvariables(playerSize, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.haste:
                haste = ModifyStatBasedOnvariables(haste, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.armorPen:
                armorPen = ModifyStatBasedOnvariables(armorPen, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.dodgeChance:
                dodgeChance = ModifyStatBasedOnvariables(dodgeChance, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.explosionSize:
                explosionSize = ModifyStatBasedOnvariables(explosionSize, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.explosionDamage:
                explosionDamage = ModifyStatBasedOnvariables(explosionDamage, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.weaponDamage:
                weaponDamage = ModifyStatBasedOnvariables(weaponDamage, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.damageReduction:
                damageReduction = ModifyStatBasedOnvariables(damageReduction, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.stunResistance:
                stunResistance = ModifyStatBasedOnvariables(stunResistance, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.debufResistance:
                debufResistance = ModifyStatBasedOnvariables(debufResistance, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.meleeDamage:
                meleeDamage = ModifyStatBasedOnvariables(meleeDamage, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.meleeSpeed:
                meleeSpeed = ModifyStatBasedOnvariables(meleeSpeed, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.parryCooldown:
                parryCooldown = ModifyStatBasedOnvariables(parryCooldown, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.weaponSize:
                weaponSize = ModifyStatBasedOnvariables(weaponSize, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.weaponRange:
                weaponRange = ModifyStatBasedOnvariables(weaponRange, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.rangedSpeed:
                rangedSpeed = ModifyStatBasedOnvariables(rangedSpeed, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.rangedDamage:
                rangedDamage = ModifyStatBasedOnvariables(rangedDamage, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.projectileAmount:
                projectileAmount = ModifyStatBasedOnvariables(projectileAmount, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.weaponBurst:
                weaponBurst = ModifyStatBasedOnvariables(weaponBurst, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.projectileBounce:
                projectileBounce = ModifyStatBasedOnvariables(projectileBounce, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.projectileSize:
                projectileSize = ModifyStatBasedOnvariables(projectileSize, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.spread:
                spread = ModifyStatBasedOnvariables(spread, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.burnDamage:
                burnDamage = ModifyStatBasedOnvariables(burnDamage, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.poisonDamage:
                poisonDamage = ModifyStatBasedOnvariables(poisonDamage, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            case TargetStat.diseaseDamage:
                diseaseDamage = ModifyStatBasedOnvariables(diseaseDamage, effect.effectValue, effect.ifIncrease, effect.isPercentage);
                break;
            default:
                Debug.Log("Something is not right");
                break;
        }
    }



}
