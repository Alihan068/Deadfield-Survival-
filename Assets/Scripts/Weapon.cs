using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType {
    Melee,
    Ranged,
    Mixed,
}

public class Weapon : MonoBehaviour {
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
    [SerializeField] float intelligence = 1f;
    [SerializeField] float haste = 1f;

    int animIndex = 1;

    AudioSource audioSource;
    [SerializeField] AudioClip[] attackSounds;
    [SerializeField] AudioClip[] equipSounds;

    void OnEnable() {
        statsManager = GetComponentInParent<StatsManager>();
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        isAttackingRanged = false;
        isCoroutineRuning = false;
        if (coroutine != null) {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (weaponType == WeaponType.Ranged) {
            projectilesParticleSystem = GetComponentInChildren<ParticleSystem>();
            particleAttack = GetComponentInChildren<RangedParticleAttack>();

            if (projectilesParticleSystem != null && projectilesParticleSystem.isPlaying) {
                projectilesParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var em = projectilesParticleSystem.emission;
                em.enabled = false;
            }
        }

        if (statsManager.isPlayer) {
            controller = GetComponentInParent<PlayerController>();
            controller.weapon = this;

            if (equipSounds.Length > 0 && audioSource != null) {
                audioSource.PlayOneShot(equipSounds[Random.Range(0, equipSounds.Length)]);
            }

            GiveBaseStats();

            if (projectilesParticleSystem != null) {
                var main = projectilesParticleSystem.main;
                main.loop = true;
                main.startDelay = 0f;

                if (projectilesParticleSystem.isPlaying) {
                    projectilesParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                var em = projectilesParticleSystem.emission;
                em.enabled = false;
            }
        }

        else {
            enemyController = GetComponentInParent<EnemyController>();
        }
    }

    public void MultipleAttackAnimation() {
        if (animIndex == 1) {
            animator.SetTrigger("UpsideDownAttack");
            PlayWeaponAttackSound();
            animIndex++;
            //Debug.Log("UpToDown! next index: " + animIndex);
        }
        else if (animIndex == 2) {
            animator.SetTrigger("DownSideUpAttack");
            PlayWeaponAttackSound();
            animIndex--;
            //Debug.Log("DownToUp! next index: " + animIndex);
        }
        else {
            //Debug.Log("BrokenAnimIndex");
        }
    }

    public void RegularAttackAnimation() {
        animator.SetTrigger("isAttacking");
        PlayWeaponAttackSound();
    }

    IEnumerator ToggleRangedAttackAudioLoop() {
        isCoroutineRuning = true;

        while (isAttackingRanged) {
            PlayWeaponAttackSound();
            yield return new WaitForSeconds(1f / statsManager.rangedSpeed);
        }

        isCoroutineRuning = false;
    }

    void GiveBaseStats() {
        //Debug.Log(this.name + " active");
        statsManager.baseDamage += weaponDamage;
        statsManager.baseRange += weaponRange;
        statsManager.meleeAttackSpeed += attackSpeed;
        statsManager.rangedSpeed += attackSpeed;
        statsManager.projectileSpeed += projectileSpeed;
        statsManager.armor += armor;
        statsManager.strength += strength;
        statsManager.intelligence += intelligence;
        statsManager.haste += haste;
    }

    void TakeBaseStats() {
        statsManager.baseDamage -= weaponDamage;
        statsManager.baseRange -= weaponRange;
        statsManager.meleeAttackSpeed -= attackSpeed;
        statsManager.rangedSpeed -= attackSpeed;
        statsManager.projectileSpeed -= projectileSpeed;
        statsManager.armor -= armor;
        statsManager.strength -= strength;
        statsManager.intelligence -= intelligence;
    }

    void PlayWeaponAttackSound() {
        if (attackSounds.Length > 0 && audioSource != null) {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            if (clip != null) {
                audioSource.PlayOneShot(clip);
            }
        }
        else {
            if (attackSounds.Length == 0) {
                Debug.LogWarning($"No attack sounds assigned to {gameObject.name}!");
            }
            if (audioSource == null) {
                Debug.LogWarning($"AudioSource is NULL on {gameObject.name}!");
            }
        }
    }


    public void SetFiring(bool pressed) {
        if (pressed) {
            if (particleAttack != null) {
                particleAttack.ParticleSystemUpdateStats();
            }

            if (!projectilesParticleSystem.isPlaying)
                projectilesParticleSystem.Play();

            var em = projectilesParticleSystem.emission;
            em.enabled = true;

            isAttackingRanged = true;
            if (!isCoroutineRuning) {
                coroutine = StartCoroutine(ToggleRangedAttackAudioLoop());
            }
        }
        else {
            var em = projectilesParticleSystem.emission;
            em.enabled = false;

            isAttackingRanged = false;
            if (isCoroutineRuning && coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
                isCoroutineRuning = false;
            }
        }
    }

    private void OnDisable() {
        if (isCoroutineRuning && coroutine != null) {
            StopCoroutine(coroutine);
            coroutine = null;
            isCoroutineRuning = false;
        }
        isAttackingRanged = false;

        if (projectilesParticleSystem != null && projectilesParticleSystem.isPlaying) {
            var em = projectilesParticleSystem.emission;
            em.enabled = false;
            projectilesParticleSystem.Stop();
        }

        if (statsManager.isPlayer) {
            TakeBaseStats();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Weapon")) return;

        HealthManager enemyHealthManager = collision.GetComponent<HealthManager>();
        CustomTime enemyCustomTime = collision.GetComponent<CustomTime>();

        if (enemyHealthManager != null) {
            enemyHealthManager.CalculateIncomingDamage(statsManager.baseDamage + statsManager.meleeDamage);
            enemyHealthManager.ApplyKnockback(statsManager.strength, transform);
        }


    }
}