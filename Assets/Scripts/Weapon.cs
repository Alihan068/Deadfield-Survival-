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

    Coroutine immunityCoroutine;

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

    void Awake() {
        statsManager = GetComponentInParent<StatsManager>();

        if (statsManager.isPlayer) {
            controller = GetComponentInParent<PlayerController>();
        }
        else {
            enemyController = GetComponentInParent<EnemyController>();
        }

        animator = GetComponent<Animator>();
        particleAttack = GetComponent<RangedParticleAttack>();
        projectilesParticleSystem = GetComponent<ParticleSystem>();
    }

    public void MultipleAttackAnimation() {
        if (animIndex == 1) {
            animator.SetTrigger("UpsideDownAttack");

            animIndex++;
            //Debug.Log("UpToDown! next index: " + animIndex);
        }
        else if (animIndex == 2) {
            animator.SetTrigger("DownSideUpAttack");
            animIndex--;
            //Debug.Log("DownToUp! next index: " + animIndex);
        }
        else {
            //Debug.Log("BrokenAnimIndex");
        }
    }

    public void RegularAttackAnimation() {
        animator.SetTrigger("isAttacking");
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

    void OnEnable() {
        if (statsManager.isPlayer) {
            controller.weapon = this;
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
    }

    public void SetFiring(bool pressed) {
        if (projectilesParticleSystem == null) return;
        if (weaponType != WeaponType.Ranged && weaponType != WeaponType.Mixed) return;

        if (pressed && particleAttack != null)
            particleAttack.ParticleSystemUpdateStats();

        var em = projectilesParticleSystem.emission;
        em.enabled = pressed;
    }

    private void OnDisable() {
        if (statsManager.isPlayer) {
            TakeBaseStats();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Weapon")) return;

        HealthManager enemyHealthManager = collision.GetComponent<HealthManager>();
        CustomTime enemyCustomTime = collision.GetComponent<CustomTime>();

        if (enemyHealthManager != null) enemyHealthManager.CalculateIncomingDamage(statsManager.baseDamage);
        if ((this.CompareTag("Player") && enemyCustomTime != null)) {
            enemyCustomTime.ScheduleKnockback(statsManager.strength, transform);
                }

    }
}