using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    WeaponSwitcher weaponSwitcher;
    StatsManager statsManager;
    Weapon weapon;
    Coroutine attackCoroutine;

    bool canAttack = true;
    bool isAttacking = false;
    bool pressingAttack;

    void Start()
    {
        weapon = GetComponentInChildren<Weapon>();
        statsManager = GetComponent<StatsManager>();
        weaponSwitcher = GetComponentInParent<WeaponSwitcher>();
    }

    public void AttackCoroutine(bool pressed)
    {
        pressingAttack = pressed;

        if (attackCoroutine == null && pressed)
        {
            attackCoroutine = StartCoroutine(AttackCooldownHandler());
        }
    }

    IEnumerator AttackCooldownHandler()
    {
        while (true)
        {
            if (!pressingAttack)
            {
                attackCoroutine = null;
                isAttacking = false;
                break;
            }

            if (canAttack && !isAttacking)
            {
                isAttacking = true;
                if (weapon != null)
                {
                    weapon.MultipleAttackAnimation();
                }

                // Attack cadence now driven by effective attack speed
                float atkSpeed = Mathf.Max(0.01f, statsManager.EffectiveAttackSpeed);
                yield return new WaitForSeconds(1f / atkSpeed);

                isAttacking = false;
            }

            yield return null;
        }
    }

    void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            pressingAttack = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (statsManager != null && weapon != null)
        {
            // Melee reach uses effective range
            Gizmos.DrawWireSphere(weapon.transform.position, statsManager.EffectiveRange);
        }
    }
}
