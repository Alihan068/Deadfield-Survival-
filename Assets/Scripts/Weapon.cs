using Unity.VisualScripting;
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

    public WeaponType weaponType;
    public LayerMask weaponTargetLayer;

    [SerializeField] GameObject ammoPrefab;

    [SerializeField] float weaponDamage = 0f;
    [SerializeField] float weaponRange = 0f;
    [SerializeField] float baseAttackSpeed = 0f;
    [SerializeField] float baseBounce = 0f;

    [SerializeField] float baseArmor = 0f;
    [SerializeField] float baseStrenght = 0f;

    [SerializeField] float baseIntelligence = 0f;


    private void Awake() {
        controller = GetComponentInParent<PlayerController>();
        statsManager = GetComponentInParent<StatsManager>();
        animator = GetComponent<Animator>();

    }

    public void AttackAnimation() {
        animator.SetTrigger("isAttacking");
    }

    public void RangedAttack() {
        while (true) {
            Instantiate(ammoPrefab, transform.position, Quaternion.identity);
        break;
        }
    }

    void GiveBaseStats() {
        Debug.Log(this.name + " active");
        statsManager.baseDamage += weaponDamage;
        statsManager.baseRange += weaponRange;
        statsManager.meleeSpeed += baseAttackSpeed;
        statsManager.rangedSpeed += baseAttackSpeed;
        statsManager.projectileBounce += baseBounce;
        statsManager.armor += baseArmor;
        statsManager.strength += baseStrenght;
        statsManager.intelligence += baseIntelligence;
    }

    void TakeBaseStats() {
        statsManager.baseDamage -= weaponDamage;
        statsManager.baseRange -= weaponRange;
        statsManager.meleeSpeed -= baseAttackSpeed;
        statsManager.rangedSpeed -= baseAttackSpeed;
        statsManager.projectileBounce -= baseBounce;
        statsManager.armor -= baseArmor;
        statsManager.strength -= baseStrenght;
        statsManager.intelligence -= baseIntelligence;

    }

    private void OnEnable() {
        controller.weapon = this;
        GiveBaseStats();
    }

    private void OnDisable() {
        TakeBaseStats();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        HealthManager enemyHealthManager = collision.GetComponent<HealthManager>();
        if (enemyHealthManager == null) return;

        enemyHealthManager.CalculateIncomingDamage(statsManager.baseDamage);
        enemyHealthManager.GetKnockback(transform, statsManager.strength);
    }
}
