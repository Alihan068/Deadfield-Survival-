using System;
using System.Drawing;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class StatsManager : MonoBehaviour {

    DifficulityManager difficultyManager;

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
    public float baseSpeed = 1f;
    public float dashCooldown = 1f;

    [Header("Base Stats")]
    public float baseMaxHealth = 100f;
    [HideInInspector] public float currentHealth = 100f;
    public float extraHealth = 0f;
    [HideInInspector] public float maxHealth;
    public float baseRange = 1f;
    [Tooltip("Effects Knockback-KnockbackResistance")]
    public float playerSize = 1f;
    public float haste = 1f;
    public float xpMultiplier = 1f;

    [Header("General Offensive")]
    public float attackSpeed = 1f;
    public float slowestAttackSPeedPerSecond = 5f;
    public float evasion = 1f;
    public float size = 1f;
    public float baseDamage = 1f;

    [Header("Knockback Attributes")]
    public float knockBack = 5f;
    public float baseAppliedKnockback = 10f;
    public float knockbackStagger = 0.15f;

    [Header("Defensive")]
    public float armor = 1f;
    public float damageReduction = 1f;
    public float stunResistance = 1f;
    public float debufResistance = 1f;
    public float parryCooldown = 1f;

    [Header("Ranged Attributes")]
    public float projectileAmount = 1f;
    public float projectileSpeed = 1f;
    public float spread = 1f;

    [Header("DOT Attributes")]
    public float burnDamage = 1f;
    public float poisonDamage = 1f;
    public float diseaseDamage = 1f;

    void Awake() {
        difficultyManager = FindFirstObjectByType<DifficulityManager>();
    }

    private float AdjustStat(float currentValue, float value, bool isPercentage, bool ifIncrease) {
        float modifier = ifIncrease ? 1f : -1f;

        if (isPercentage) {
            // Apply increase/decrease as a percentage of the current value
            return currentValue * (1f + modifier * (value / 100f));
        }
        else {
            // Apply a flat increase/decrease
            return currentValue + (modifier * value);
        }
    }

    public void ApplyEffect(CollectibleItemSO.ItemEffect effect) {

        bool touchedHealth = false;

        switch (effect.targetStat) {
            // Base Stats
            case TargetStat.currentHealth:
                currentHealth = AdjustStat(currentHealth, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                if (maxHealth > 0f) {
                    currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
                }
                else {
                    currentHealth = Mathf.Max(0f, currentHealth);
                }
                touchedHealth = true;
                break;

            case TargetStat.extraHealth:
                extraHealth = AdjustStat(extraHealth, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                extraHealth = Mathf.Max(0f, extraHealth);
                touchedHealth = true;
                break;

            case TargetStat.maxHealth:
                maxHealth = AdjustStat(maxHealth, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                maxHealth = Mathf.Max(1f, maxHealth);
                touchedHealth = true;
                break;

            case TargetStat.baseRange:
                baseRange = AdjustStat(baseRange, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                baseRange = Mathf.Max(0f, baseRange);
                break;

            case TargetStat.strength:
                knockBack = AdjustStat(knockBack, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                knockBack = Mathf.Max(0f, knockBack);
                break;

            case TargetStat.armor:
                armor = AdjustStat(armor, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                armor = Mathf.Max(0f, armor);
                break;

            case TargetStat.playerSize:
                playerSize = AdjustStat(playerSize, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                playerSize = Mathf.Max(0.1f, playerSize);
                break;

            case TargetStat.haste:
                haste = AdjustStat(haste, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                haste = Mathf.Max(0f, haste);
                break;

            case TargetStat.evasion:
                evasion = AdjustStat(evasion, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                evasion = Mathf.Max(0f, evasion);
                break;

            case TargetStat.xpMultiplier:
                xpMultiplier = AdjustStat(xpMultiplier, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                xpMultiplier = Mathf.Max(0f, xpMultiplier);
                break;

            case TargetStat.baseDamage:
                baseDamage = AdjustStat(baseDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                baseDamage = Mathf.Max(0f, baseDamage);
                break;

            // Knockback Attributes
            case TargetStat.baseAppliedKnockback:
                baseAppliedKnockback = AdjustStat(baseAppliedKnockback, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                baseAppliedKnockback = Mathf.Max(0f, baseAppliedKnockback);
                break;

            case TargetStat.knockbackStagger:
                knockbackStagger = AdjustStat(knockbackStagger, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                knockbackStagger = Mathf.Max(0f, knockbackStagger);
                break;

            // General Defensive
            case TargetStat.damageReduction:
                damageReduction = AdjustStat(damageReduction, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                damageReduction = Mathf.Max(0f, damageReduction);
                break;

            case TargetStat.stunResistance:
                stunResistance = AdjustStat(stunResistance, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                stunResistance = Mathf.Max(0f, stunResistance);
                break;

            case TargetStat.debufResistance:
                debufResistance = AdjustStat(debufResistance, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                debufResistance = Mathf.Max(0f, debufResistance);
                break;

            case TargetStat.attackSpeed:
                attackSpeed = AdjustStat(attackSpeed, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                // Attacks per second must not be 0 or negative
                attackSpeed = Mathf.Max(0.01f, attackSpeed);
                break;

            case TargetStat.parryCooldown:
                parryCooldown = AdjustStat(parryCooldown, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                parryCooldown = Mathf.Max(0f, parryCooldown);
                break;

            case TargetStat.slowestAttackSPeedPerSecond:
                slowestAttackSPeedPerSecond = AdjustStat(slowestAttackSPeedPerSecond, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                slowestAttackSPeedPerSecond = Mathf.Max(0f, slowestAttackSPeedPerSecond);
                break;

            case TargetStat.projectileAmount:
                projectileAmount = AdjustStat(projectileAmount, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                projectileAmount = Mathf.Max(0f, projectileAmount);
                break;

            case TargetStat.projectileSpeed:
                projectileSpeed = AdjustStat(projectileSpeed, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                projectileSpeed = Mathf.Max(0f, projectileSpeed);
                break;

            case TargetStat.spread:
                spread = AdjustStat(spread, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                spread = Mathf.Max(0f, spread);
                break;

            // DOT Attributes
            case TargetStat.burnDamage:
                burnDamage = AdjustStat(burnDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                burnDamage = Mathf.Max(0f, burnDamage);
                break;

            case TargetStat.poisonDamage:
                poisonDamage = AdjustStat(poisonDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                poisonDamage = Mathf.Max(0f, poisonDamage);
                break;

            case TargetStat.diseaseDamage:
                diseaseDamage = AdjustStat(diseaseDamage, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                diseaseDamage = Mathf.Max(0f, diseaseDamage);
                break;

            case TargetStat.enemyDamageMultiplier:
                if (difficultyManager != null) {
                    difficultyManager.enemyDamageMultiplier =
                        AdjustStat(difficultyManager.enemyDamageMultiplier, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                    difficultyManager.enemyDamageMultiplier = Mathf.Max(0f, difficultyManager.enemyDamageMultiplier);
                }
                break;

            case TargetStat.enemyHealthMultiplier:
                if (difficultyManager != null) {
                    difficultyManager.enemyHealthMultiplier =
                        AdjustStat(difficultyManager.enemyHealthMultiplier, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                    difficultyManager.enemyHealthMultiplier = Mathf.Max(0f, difficultyManager.enemyHealthMultiplier);
                }
                break;

            case TargetStat.enemySpeedMultiplier:
                if (difficultyManager != null) {
                    difficultyManager.enemySpeedMultiplier =
                        AdjustStat(difficultyManager.enemySpeedMultiplier, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                    difficultyManager.enemySpeedMultiplier = Mathf.Max(0f, difficultyManager.enemySpeedMultiplier);
                }
                break;

            case TargetStat.luck:
                if (difficultyManager != null) {
                    difficultyManager.playerLuck =
                        AdjustStat(difficultyManager.playerLuck, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                    difficultyManager.playerLuck = Mathf.Max(0f, difficultyManager.playerLuck);
                }
                break;
        }

        if (touchedHealth) {
            var healthManager = GetComponent<HealthManager>();
            if (healthManager != null) {
                healthManager.UpdateMaxHp();
                // Ensure currentHealth does not exceed maxHealth after max updates
                if (maxHealth > 0f) {
                    currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
                }
                else {
                    currentHealth = Mathf.Max(0f, currentHealth);
                }
            }
        }
    }
}
