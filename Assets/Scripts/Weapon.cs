using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public WeaponType weaponType;
    public LayerMask weaponTargetLayer;

    [SerializeField] GameObject ammoPrefab;
    [SerializeField] InputActionReference fireAction;

    [SerializeField] float weaponDamage = 1f;
    [SerializeField] float weaponRange = 1f;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float projectileSpeed = 1f;

    [SerializeField] float armor = 1f;
    [SerializeField] float strength = 1f;
    [SerializeField] float intelligence = 1f;
    [SerializeField] float haste = 1f;

    Coroutine firingLoop;

    private void Awake() {
        controller = GetComponentInParent<PlayerController>();
        statsManager = GetComponentInParent<StatsManager>();
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

    private void OnEnable() {
        controller.weapon = this;
        GiveBaseStats();

        var action = fireAction.action;
        action.Enable();
        action.started += OnFireStarted;   // press down
        action.canceled += OnFireCanceled; // release
    }

    private void OnDisable() {
        TakeBaseStats();

        var a = fireAction.action;
        a.started -= OnFireStarted;
        a.canceled -= OnFireCanceled;
        a.Disable();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        HealthManager enemyHealthManager = collision.GetComponent<HealthManager>();
        if (enemyHealthManager == null) return;

        enemyHealthManager.CalculateIncomingDamage(statsManager.baseDamage);
        enemyHealthManager.GetKnockback(transform, statsManager.strength);
    }
    void OnFireStarted(InputAction.CallbackContext ctx) {
        if (firingLoop == null)
            firingLoop = StartCoroutine(FireCoroutine());
    }

    void OnFireCanceled(InputAction.CallbackContext ctx) {
        if (firingLoop != null) {
            StopCoroutine(firingLoop);
            firingLoop = null;
        }
    }
    IEnumerator FireCoroutine() {
        while (true) {
            particleAttack.ParticleSystemPlay();
            yield return new WaitForSeconds(0.001f);
        }
    }

}
