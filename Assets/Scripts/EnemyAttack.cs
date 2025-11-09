using Unity.VisualScripting;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    StatsManager statsManager;
    EnemyController enemyController;
    HealthManager healthManager;

    private void Start() {
        statsManager = GetComponent<StatsManager>();
        healthManager = GetComponent<HealthManager>();
        enemyController = GetComponent<EnemyController>();
    }

    void AttackTarget(EnemyType enemyType) {
        switch (enemyType) {
            case EnemyType.MeleeWeapon:
                MeleeAttack();
                break;
            case EnemyType.Ranged:
                RangedAttack();
                break;
        }
    }
     
    void MeleeAttack() {

    }

    void RangedAttack() {

    }

}
