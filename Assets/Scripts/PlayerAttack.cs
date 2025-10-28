using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    WeaponSwitcher weaponSwitcher;
    PlayerStatsManager playerStatsManager;
    Weapon weapon;
    
    void Start()
    {
        weapon = GetComponentInChildren<Weapon>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        weaponSwitcher = GetComponentInParent<WeaponSwitcher>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AttackWithMeleeWeapon(Weapon weapon) {
        Vector2 vector = weapon.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(vector, playerStatsManager.weaponSize, weapon.weaponTargetLayer);
        foreach (Collider2D hit in hits ) {
            Debug.Log("Hit target: " + hit.name);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (playerStatsManager != null) {
            Gizmos.DrawWireSphere(weapon.transform.position, playerStatsManager.playerBaseRange);
        }
    }
}
