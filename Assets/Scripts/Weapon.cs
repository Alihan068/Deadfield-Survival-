using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    [Header("Player attacking animation related")]
    private readonly string topToBottomAttackAnim = "TopToBottomAttack";
    private readonly string bottomToTopAttackAnim = "BottomToTopAttack";
    [SerializeField] float meleeCrossFade = 0.05f;
    [SerializeField] float meleeCooldown = 0.25f;

    int meleeHashA1;
    int meleeHashA2;
    bool meleeHeld;
    bool meleeLoopRunning;
    int meleeIndex; // 0 -> A1, 1 -> A2
    Coroutine meleeLoop;


    void Awake() {
        meleeHashA1 = Animator.StringToHash(topToBottomAttackAnim);
        meleeHashA2 = Animator.StringToHash(bottomToTopAttackAnim);

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

    public void AttackAnimation() {
        animator.SetTrigger("isAttacking");
    }

    void GiveBaseStats() {
        Debug.Log(this.name + " active");
        statsManager.baseDamage += weaponDamage;
        statsManager.baseRange += weaponRange;
        statsManager.meleeSpeed += attackSpeed;
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
        statsManager.meleeSpeed -= attackSpeed;
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
        if (projectilesParticleSystem == null ) return;
        if (weaponType != WeaponType.Ranged && weaponType != WeaponType.Mixed) return;

        if (pressed && particleAttack != null)
            particleAttack.ParticleSystemUpdateStats();

        var em = projectilesParticleSystem.emission;
        em.enabled = pressed;
    }

    public void SetMeleeHold(bool pressed)
    {
        if (weaponType != WeaponType.Melee) return;
        meleeHeld = pressed;

        if (pressed)
        {
            if (!meleeLoopRunning)
            {
                meleeLoop = StartCoroutine(MeleeLoop());
            }
        }
    }

    IEnumerator MeleeLoop() {
        meleeLoopRunning = true;

        while (true)
        {
            int targetHash;
            if (meleeIndex == 0) {
                targetHash = meleeHashA1;
            }
            else {
                targetHash = meleeHashA2;
            }

            int layer = 0;
            animator.CrossFade(targetHash, meleeCrossFade, layer, 0f);

            // ensure the state actually became current
            yield return null;
            while (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != targetHash)
            {
                yield return null;
            }

            // wait for the clip to finish
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }

            // cooldown gap between swings
            float t = 0f;
            while (t < meleeCooldown)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (!meleeHeld) break;

            // alternate A1 <-> A2
            if (meleeIndex == 0) meleeIndex = 1;
            else meleeIndex = 0;
        }

        meleeLoopRunning = false;
        meleeLoop = null;
    }


    private void OnDisable() {
        if (statsManager.isPlayer) {
            TakeBaseStats();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        HealthManager enemyHealthManager = collision.GetComponent<HealthManager>();
        if (enemyHealthManager == null) return;

        enemyHealthManager.CalculateIncomingDamage(statsManager.baseDamage);
        enemyHealthManager.GetKnockback(transform, statsManager.strength);
    }
}