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

    // Update is called once per frame
    void Update() {

    }

    void AttackWithMeleeWeapon() {
        Vector2 vector = weapon.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(vector, statsManager.weaponSize, weapon.weaponTargetLayer);
        foreach (Collider2D hit in hits) {
            Debug.Log("Hit target: " + hit.name);
            HealthManager targetHealthManager = hit.GetComponent<HealthManager>();
            targetHealthManager.CalculateIncomingDamage(statsManager.baseDamage);
        }
    }

    public void AttackCoroutine(bool pressed) {
        pressingAttack = pressed;

        if (attackCoroutine == null && pressed) {
            Debug.Log("AttackCoroutine started");
            attackCoroutine = StartCoroutine(AttackCooldownHandler());
        }
    }
    IEnumerator AttackCooldownHandler() {
        while (true) {
            if (!pressingAttack) { attackCoroutine = null; isAttacking = false; break; }

            if (canAttack && !isAttacking) {
                isAttacking = true;
                weapon.MultipleAttackAnimation();
                Debug.Log("Attacked");
                yield return new WaitForSeconds(1f / statsManager.meleeAttackSpeed);
                isAttacking = false;
            }
            yield return null;
        }
    }

    void CalculateAttackSpeed(float attackSpeed) { 
    
    }
    




    private void OnDisable() {
        if (attackCoroutine != null) {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            pressingAttack = false;
        }
    }

  


    //    void MeleeSwitch() {
    //        while (t < meleeCooldown) {
    //            t += Time.deltaTime;
    //            yield return null;
    //        }

    //        if (!meleeHeld) break;

    //        // alternate A1 <-> A2
    //        if (meleeIndex == 0) meleeIndex = 1;
    //        else meleeIndex = 0;
    //    }
    //}



    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (statsManager != null) {
            Gizmos.DrawWireSphere(weapon.transform.position, statsManager.baseRange);
        }
    }
}
