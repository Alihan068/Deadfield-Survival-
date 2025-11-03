using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    WeaponSwitcher weaponSwitcher;
    StatsManager statsManager;
    Weapon weapon;

    bool isPlayer = false;
    
    void Start()
    {
        weapon = GetComponentInChildren<Weapon>();
        statsManager = GetComponent<StatsManager>();
        weaponSwitcher = GetComponentInParent<WeaponSwitcher>();
        isPlayer = statsManager.isPlayer;
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
        animator.SetTrigger("isAttacking");
        yield return new WaitForSeconds(attackDelay);
    }



    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (statsManager != null) {
            Gizmos.DrawWireSphere(weapon.transform.position, statsManager.baseRange);
        }
    }
}
