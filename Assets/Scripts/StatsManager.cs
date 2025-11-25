using System;
using System.Drawing;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.ParticleSystem;

public class StatsManager : MonoBehaviour
{
    DifficulityManager difficultyManager;

    public bool isPlayer;
    public bool canCollectItems = false;
    public bool canMove = true;
    public bool canBeDamaged = true;
    public bool canAttack = true;
    public bool isUnstoppable = false;
    public bool isKnocked;
    public bool triggersHitStop = false;

    [Header("Movement")]
    [Tooltip("Base movement speed before any item/buff bonuses.")]
    public float baseSpeed = 1f;

    public float dashCooldown = 1f;

    [Tooltip("Flat bonus applied on top of baseSpeed.")]
    public float moveSpeedFlatBonus = 0f;

    [Tooltip("Percentage bonus for movement speed. 0.2 = +20%.")]
    public float moveSpeedPercentBonus = 0f;

    public float EffectiveMoveSpeed
    {
        get
        {
            float flat = Mathf.Max(0f, baseSpeed + moveSpeedFlatBonus);
            float result = flat * (1f + moveSpeedPercentBonus);
            return Mathf.Max(0f, result);
        }
    }

    [Header("Base Stats")]
    [Tooltip("Base max health before any flat or percentage bonuses.")]
    public float baseMaxHealth = 100f;

    [HideInInspector] public float currentHealth = 100f;

    [Tooltip("Flat max health bonus from items and effects.")]
    public float extraHealth = 0f;

    [HideInInspector] public float maxHealth;

    [Tooltip("Base attack/interaction range before bonuses.")]
    public float baseRange = 1f;

    [Tooltip("Flat bonus applied on top of baseRange.")]
    public float rangeFlatBonus = 0f;

    [Tooltip("Percentage bonus for range. 0.25 = +25%.")]
    public float rangePercentBonus = 0f;

    public float EffectiveRange
    {
        get
        {
            float flat = Mathf.Max(0f, baseRange + rangeFlatBonus);
            float result = flat * (1f + rangePercentBonus);
            return Mathf.Max(0f, result);
        }
    }

    [Tooltip("Affects knockback-related calculations.")]
    public float playerSize = 1f;

    [Tooltip("General action speed multiplier used by various systems.")]
    public float haste = 1f;

    [Tooltip("Experience multiplier. 1 = normal, 2 = +100% XP.")]
    public float xpMultiplier = 1f;

    [Header("General Offensive")]
    [Tooltip("Base attack speed in attacks per second, before bonuses.")]
    public float attackSpeed = 1f;

    [Tooltip("Minimum attacks per second limit.")]
    public float minAttackSpeed = 0.1f;

    [Tooltip("Maximum attacks per second limit.")]
    public float maxAttackSpeed = 5f;

    [Tooltip("Flat bonus applied on top of attackSpeed (attacks per second).")]
    public float attackSpeedFlatBonus = 0f;

    [Tooltip("Percentage bonus for attack speed. 0.5 = +50%.")]
    public float attackSpeedPercentBonus = 0f;

    public float EffectiveAttackSpeed
    {
        get
        {
            float baseValue = Mathf.Max(0.01f, attackSpeed + attackSpeedFlatBonus);
            float result = baseValue * (1f + attackSpeedPercentBonus);

            float minAPS = Mathf.Max(0.01f, minAttackSpeed);
            float maxAPS = maxAttackSpeed > 0f ? Mathf.Max(minAPS, maxAttackSpeed) : minAPS;

            result = Mathf.Clamp(result, minAPS, maxAPS);
            return result;
        }
    }

    [Tooltip("Chance to avoid attacks. Implementation is not yet applied.")]
    public float evasion = 0f;

    [FormerlySerializedAs("size")]
    [Tooltip("Global/effect size multiplier for weapons, projectiles, AoE, etc.")]
    public float effectSize = 1f;

    [Tooltip("Base damage before any flat or percentage bonuses.")]
    public float baseDamage = 1f;

    [Tooltip("Flat bonus applied on top of baseDamage.")]
    public float damageFlatBonus = 0f;

    [Tooltip("Percentage bonus for damage. 0.3 = +30%.")]
    public float damagePercentBonus = 0f;

    public float EffectiveDamage
    {
        get
        {
            float flat = Mathf.Max(0f, baseDamage + damageFlatBonus);
            float result = flat * (1f + damagePercentBonus);
            return Mathf.Max(0f, result);
        }
    }

    [Header("Knockback Attributes")]
    [Tooltip("Knockback power applied to targets.")]
    public float knockBack = 5f;

    [Tooltip("Base knockback applied from core systems (e.g. charge attacks).")]
    public float baseAppliedKnockback = 10f;

    [Tooltip("How long knockback keeps the target staggered.")]
    public float knockbackStagger = 0.15f;

    [Header("Defensive")]
    [Tooltip("Base armor value before bonuses.")]
    public float armor = 5f;

    [Tooltip("Flat armor bonus.")]
    public float armorFlatBonus = 0f;

    [Tooltip("Percentage armor bonus. 0.4 = +40%.")]
    public float armorPercentBonus = 0f;

    public float EffectiveArmor
    {
        get
        {
            float flat = Mathf.Max(0f, armor + armorFlatBonus);
            float result = flat * (1f + armorPercentBonus);
            return Mathf.Max(0f, result);
        }
    }

    [Tooltip("Base damage reduction in percent. 0 = none, 50 = 50% less damage, -20 = 20% more damage.")]
    public float damageReduction = 0f;

    [Tooltip("Flat bonus (percent) for damageReduction.")]
    public float damageReductionFlatBonus = 0f;

    [Tooltip("Percentage bonus (multiplier) for damageReduction. 0.5 = +50% more of the combined value.")]
    public float damageReductionPercentBonus = 0f;

    public float EffectiveDamageReduction
    {
        get
        {
            float percent = damageReduction + damageReductionFlatBonus;
            percent *= (1f + damageReductionPercentBonus);
            return Mathf.Clamp(percent, -100f, 95f);
        }
    }

    public float stunResistance = 0f;
    public float debufResistance = 0f;
    public float parryCooldown = 3f;

    [Header("Ranged Attributes")]
    public float projectileAmount = 1f;

    [Tooltip("Base projectile speed before any bonuses.")]
    public float projectileSpeed = 1f;

    [Tooltip("Flat bonus for projectileSpeed.")]
    public float projectileSpeedFlatBonus = 0f;

    [Tooltip("Percentage bonus for projectileSpeed.")]
    public float projectileSpeedPercentBonus = 0f;

    public float EffectiveProjectileSpeed
    {
        get
        {
            float flat = Mathf.Max(0f, projectileSpeed + projectileSpeedFlatBonus);
            float result = flat * (1f + projectileSpeedPercentBonus);
            return Mathf.Max(0f, result);
        }
    }

    [Tooltip("Spread angle or randomness, currently used as raw value (0-100).")]
    public float spread = 0f;

    [Header("DOT Attributes")]
    public float burnDamage = 1f;
    public float poisonDamage = 1f;
    public float diseaseDamage = 1f;

    [Header("Meta / Utility")]
    [Tooltip("Luck value that can be used by loot, crit or other chance-based systems.")]
    public float luck = 0f;

    void Awake()
    {
        difficultyManager = FindFirstObjectByType<DifficulityManager>();
    }

    float AdjustStat(float currentValue, float value, bool isPercentage, bool ifIncrease)
    {
        float modifier = ifIncrease ? 1f : -1f;

        if (isPercentage)
        {
            return currentValue * (1f + modifier * (value / 100f));
        }
        else
        {
            return currentValue + (modifier * value);
        }
    }

    public void ApplyEffect(CollectibleItemSO.ItemEffect effect)
    {
        bool touchedHealth = false;

        switch (effect.targetStat)
        {
            case TargetStat.currentHealth:
                currentHealth = AdjustStat(currentHealth, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                if (maxHealth > 0f)
                {
                    currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
                }
                else
                {
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

            case TargetStat.size:
                effectSize = AdjustStat(effectSize, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                effectSize = Mathf.Max(0f, effectSize);
                break;

            case TargetStat.baseAppliedKnockback:
                baseAppliedKnockback = AdjustStat(baseAppliedKnockback, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                baseAppliedKnockback = Mathf.Max(0f, baseAppliedKnockback);
                break;

            case TargetStat.knockbackStagger:
                knockbackStagger = AdjustStat(knockbackStagger, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                knockbackStagger = Mathf.Max(0f, knockbackStagger);
                break;

            case TargetStat.damageReduction:
                damageReduction = AdjustStat(damageReduction, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
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
                attackSpeed = Mathf.Max(0.01f, attackSpeed);
                break;

            case TargetStat.parryCooldown:
                parryCooldown = AdjustStat(parryCooldown, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                parryCooldown = Mathf.Max(0f, parryCooldown);
                break;

            case TargetStat.slowestAttackSPeedPerSecond:
                maxAttackSpeed = AdjustStat(maxAttackSpeed, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                maxAttackSpeed = Mathf.Max(0f, maxAttackSpeed);
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
                if (difficultyManager != null)
                {
                    difficultyManager.enemyDamageMultiplier =
                        AdjustStat(difficultyManager.enemyDamageMultiplier, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                    difficultyManager.enemyDamageMultiplier = Mathf.Max(0f, difficultyManager.enemyDamageMultiplier);
                }
                break;

            case TargetStat.enemyHealthMultiplier:
                if (difficultyManager != null)
                {
                    difficultyManager.enemyHealthMultiplier =
                        AdjustStat(difficultyManager.enemyHealthMultiplier, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                    difficultyManager.enemyHealthMultiplier = Mathf.Max(0f, difficultyManager.enemyHealthMultiplier);
                }
                break;

            case TargetStat.enemySpeedMultiplier:
                if (difficultyManager != null)
                {
                    difficultyManager.enemySpeedMultiplier =
                        AdjustStat(difficultyManager.enemySpeedMultiplier, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                    difficultyManager.enemySpeedMultiplier = Mathf.Max(0f, difficultyManager.enemySpeedMultiplier);
                }
                break;

            case TargetStat.luck:
                luck = AdjustStat(luck, effect.effectValue, effect.ifPercentage, effect.ifIncrease);
                luck = Mathf.Max(0f, luck);
                if (difficultyManager != null)
                {
                    difficultyManager.playerLuck = luck;
                }
                break;
        }

        if (touchedHealth)
        {
        }
    }
}
