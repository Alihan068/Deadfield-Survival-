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

        if (weaponType == WeaponType.Ranged) {
            projectilesParticleSystem = GetComponent<ParticleSystem>();
            particleAttack = GetComponent<RangedParticleAttack>();
            StartCoroutine(ToggleRangedAttackAudioLoop());
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

                var em = projectilesParticleSystem.emission;
                em.enabled = false;

                if (!projectilesParticleSystem.isPlaying)
                    projectilesParticleSystem.Play();
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
        if (!isCoroutineRuning) {
            isCoroutineRuning = true;
            while (isAttackingRanged) {
                if (weaponType == WeaponType.Ranged) {
                    PlayWeaponAttackSound();
                    yield return new WaitForSeconds(1f / statsManager.rangedSpeed);
                }
            }
            yield return null;
            isCoroutineRuning = false;
        }
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
            audioSource.PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length)]);
        }
    }


    public void SetFiring(bool pressed) {
        if (projectilesParticleSystem == null) return;
        if (weaponType != WeaponType.Ranged && weaponType != WeaponType.Mixed) return;

        if (pressed && particleAttack != null)
            particleAttack.ParticleSystemUpdateStats();

        var em = projectilesParticleSystem.emission;
        em.enabled = pressed;
        isAttackingRanged = pressed;
    }

    private void OnDisable() {
        if (isCoroutineRuning && coroutine != null) {
            StopCoroutine(coroutine);
            coroutine = null;
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
            enemyHealthManager.CalculateIncomingDamage(statsManager.baseDamage);
            enemyHealthManager.ApplyKnockback(statsManager.strength, transform);
        }


    }
}