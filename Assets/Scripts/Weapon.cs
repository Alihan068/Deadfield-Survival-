using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum WeaponType
{
    Melee,
    Ranged,
    Mixed,
}

public class Weapon : MonoBehaviour
{
    StatsManager statsManager;
    PlayerController controller;
    Animator animator;
    RangedParticleAttack particleAttack;
    ParticleSystem projectilesParticleSystem;
    EnemyController enemyController;

    Coroutine coroutine;
    bool isCoroutineRuning = false;
    bool isAttackingRanged = true;

    public WeaponType weaponType;
    public LayerMask weaponTargetLayer;

    [SerializeField] GameObject ammoPrefab;

    [SerializeField] float weaponDamage = 1f;
    [SerializeField] float weaponRange = 1f;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float projectileSpeed = 1f;

    [SerializeField] float armor = 1f;
    [SerializeField] float strength = 1f;
    //[SerializeField] float intelligence = 1f;
    [SerializeField] float haste = 1f;

    int animIndex = 1;

    AudioSource audioSource;
    [SerializeField] AudioClip[] attackSounds;
    [SerializeField] AudioClip[] equipSounds;

    void OnEnable()
    {
        statsManager = GetComponentInParent<StatsManager>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        isAttackingRanged = false;
        isCoroutineRuning = false;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (weaponType == WeaponType.Ranged)
        {
            projectilesParticleSystem = GetComponentInChildren<ParticleSystem>();
            particleAttack = GetComponentInChildren<RangedParticleAttack>();

            if (projectilesParticleSystem != null && projectilesParticleSystem.isPlaying)
            {
                projectilesParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var em = projectilesParticleSystem.emission;
                em.enabled = false;
            }
        }

        if (statsManager.isPlayer)
        {
            controller = GetComponentInParent<PlayerController>();
            controller.weapon = this;

            if (equipSounds.Length > 0 && audioSource != null)
            {
                audioSource.PlayOneShot(equipSounds[Random.Range(0, equipSounds.Length)]);
            }

            GiveBaseStats();

            if (projectilesParticleSystem != null)
            {
                var main = projectilesParticleSystem.main;
                main.loop = true;
                main.startDelay = 0f;

                if (projectilesParticleSystem.isPlaying)
                {
                    projectilesParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                var em = projectilesParticleSystem.emission;
                em.enabled = false;
            }
        }
        else
        {
            enemyController = GetComponentInParent<EnemyController>();
        }
    }

    public void MultipleAttackAnimation()
    {
        if (animIndex == 1)
        {
            animator.SetTrigger("UpsideDownAttack");
            PlayWeaponAttackSound();
            animIndex++;
        }
        else if (animIndex == 2)
        {
            animator.SetTrigger("DownSideUpAttack");
            PlayWeaponAttackSound();
            animIndex--;
        }
    }

    public void RegularAttackAnimation()
    {
        animator.SetTrigger("isAttacking");
        PlayWeaponAttackSound();
    }

    IEnumerator ToggleRangedAttackAudioLoop()
    {
        isCoroutineRuning = true;

        while (isAttackingRanged)
        {
            PlayWeaponAttackSound();

            float atkSpeed = Mathf.Max(0.01f, statsManager.EffectiveAttackSpeed);
            yield return new WaitForSeconds(1f / atkSpeed);
        }

        isCoroutineRuning = false;
    }

    void GiveBaseStats()
    {
        // Weapon contributes as flat bonuses into layered stats
        statsManager.damageFlatBonus += weaponDamage;
        statsManager.rangeFlatBonus += weaponRange;
        statsManager.attackSpeedFlatBonus += attackSpeed;
        statsManager.projectileSpeedFlatBonus += projectileSpeed;
        statsManager.armorFlatBonus += armor;

        // Knockback and haste currently applied as direct modifiers
        statsManager.knockBack += strength;
        statsManager.haste += haste;
    }

    void TakeBaseStats()
    {
        statsManager.damageFlatBonus -= weaponDamage;
        statsManager.rangeFlatBonus -= weaponRange;
        statsManager.attackSpeedFlatBonus -= attackSpeed;
        statsManager.projectileSpeedFlatBonus -= projectileSpeed;
        statsManager.armorFlatBonus -= armor;
        statsManager.knockBack -= strength;
        // Haste is left as additive; reverse it as well
        statsManager.haste -= haste;
    }

    void PlayWeaponAttackSound()
    {
        if (attackSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            if (clip != null && audioSource.enabled)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    public void SetFiring(bool pressed)
    {
        if (projectilesParticleSystem == null)
        {
            return;
        }

        if (pressed)
        {
            if (particleAttack != null)
            {
                particleAttack.ParticleSystemUpdateStats();
            }

            if (!projectilesParticleSystem.isPlaying)
            {
                projectilesParticleSystem.Play();
            }

            var em = projectilesParticleSystem.emission;
            em.enabled = true;

            isAttackingRanged = true;
            if (!isCoroutineRuning)
            {
                coroutine = StartCoroutine(ToggleRangedAttackAudioLoop());
            }
        }
        else
        {
            var em = projectilesParticleSystem.emission;
            em.enabled = false;

            isAttackingRanged = false;
            if (isCoroutineRuning && coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
                isCoroutineRuning = false;
            }
        }
    }

    void OnDisable()
    {
        if (isCoroutineRuning && coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
            isCoroutineRuning = false;
        }

        isAttackingRanged = false;

        if (projectilesParticleSystem != null && projectilesParticleSystem.isPlaying)
        {
            var em = projectilesParticleSystem.emission;
            em.enabled = false;
            projectilesParticleSystem.Stop();
        }

        if (statsManager != null && statsManager.isPlayer)
        {
            TakeBaseStats();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Weapon")) return;

        HealthManager enemyHealthManager = collision.GetComponent<HealthManager>();
        CustomTime enemyCustomTime = collision.GetComponent<CustomTime>();

        if (enemyHealthManager != null)
        {
            float damage = statsManager.EffectiveDamage;
            enemyHealthManager.CalculateIncomingDamage(damage);
            enemyHealthManager.ApplyKnockback(statsManager.knockBack, transform);
        }
    }
}
