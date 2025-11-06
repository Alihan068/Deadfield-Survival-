using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {
    WeaponSwitcher weaponSwitcher;
    StatsManager statsManager;
    Weapon weapon;
    Coroutine attackCoroutine;

    bool canAttack = true;
    bool isAttacking = false;
    bool pressingAttack;
    void Start() {
        weapon = GetComponentInChildren<Weapon>();
        statsManager = GetComponent<StatsManager>();
        weaponSwitcher = GetComponentInParent<WeaponSwitcher>();
    }

    //// Update is called once per frame
    //void Update() {

    //}

    //void AttackWithMeleeWeapon() {
    //    Vector2 vector = weapon.transform.position;
    //    Collider2D[] hits = Physics2D.OverlapCircleAll(vector, statsManager.weaponSize, weapon.weaponTargetLayer);
    //    foreach (Collider2D hit in hits) {
    //        Debug.Log("Hit target: " + hit.name);
    //        HealthManager targetHealthManager = hit.GetComponent<HealthManager>();
    //        targetHealthManager.CalculateIncomingDamage(statsManager.baseDamage, transform);
    //    }
    //}

    public void AttackCoroutine(bool pressed) {
        pressingAttack = pressed;

        if (attackCoroutine == null && pressed) {
            //Debug.Log("AttackCoroutine started");
            attackCoroutine = StartCoroutine(AttackCooldownHandler());
        }
    }
    IEnumerator AttackCooldownHandler() {
        while (true) {
            if (!pressingAttack) { attackCoroutine = null; isAttacking = false; break; }

            if (canAttack && !isAttacking) {
                isAttacking = true;
                weapon.MultipleAttackAnimation();
                //Debug.Log("Attacked");
                yield return new WaitForSeconds(1f / statsManager.meleeAttackSpeed);
                isAttacking = false;
            }
            yield return null;
        }
    }

    private void OnDisable() {
        if (attackCoroutine != null) {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            pressingAttack = false;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (statsManager != null) {
            Gizmos.DrawWireSphere(weapon.transform.position, statsManager.baseRange);
        }
    }
}
