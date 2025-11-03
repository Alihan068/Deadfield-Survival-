using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    WeaponSwitcher weaponSwitcher;
    StatsManager statsManager;
    Weapon weapon;


    bool canAttack = true;
    void Start()
    {
        weapon = GetComponentInChildren<Weapon>();
        statsManager = GetComponent<StatsManager>();
        weaponSwitcher = GetComponentInParent<WeaponSwitcher>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AttackWithMeleeWeapon() {
        Vector2 vector = weapon.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(vector, statsManager.weaponSize, weapon.weaponTargetLayer);
        foreach (Collider2D hit in hits ) {
            Debug.Log("Hit target: " + hit.name);
            HealthManager targetHealthManager = hit.GetComponent<HealthManager>();
            targetHealthManager.CalculateIncomingDamage(statsManager.baseDamage);
        }
    }
    public IEnumerator MeleeAnimationAttack(Animator animator,float attackDelay) {
        if (canAttack) {
            canAttack = false;
            animator.SetTrigger("isAttacking");
            yield return new WaitForSeconds(statsManager.meleeSpeed);
            canAttack = true;
        }
    }



    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (statsManager != null) {
            Gizmos.DrawWireSphere(weapon.transform.position, statsManager.baseRange);
        }
    }
}
