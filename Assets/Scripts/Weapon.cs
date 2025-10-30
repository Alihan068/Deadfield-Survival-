using Unity.VisualScripting;
using UnityEngine;
public enum WeaponType {
    Melee,
    Ranged,
    Mixed,
}
public class Weapon : MonoBehaviour {


    StatsManager playerStatsManager;
    PlayerController playerController;

    public WeaponType weaponType;
    public LayerMask weaponTargetLayer;

    [SerializeField] float weaponDamage = 0f;
    [SerializeField] float weaponRange = 0f;
    [SerializeField] float baseAttackSpeed = 0f;
    [SerializeField] float baseBounce = 0f;

    [SerializeField] float baseArmor = 0f;
    [SerializeField] float baseStrenght = 0f;

    [SerializeField] float baseIntelligence = 0f;


    private void Awake() {
        playerController = GetComponentInParent<PlayerController>();
        playerStatsManager = GetComponentInParent<StatsManager>();

    }

    void GiveBaseStats() {
        Debug.Log(this.name + " active");
        playerStatsManager.baseDamage += weaponDamage;
        playerStatsManager.baseRange += weaponRange;
        playerStatsManager.meleeSpeed += baseAttackSpeed;
        playerStatsManager.rangedSpeed += baseAttackSpeed;
        playerStatsManager.projectileBounce += baseBounce;
        playerStatsManager.armor += baseArmor;
        playerStatsManager.strength += baseStrenght;
        playerStatsManager.intelligence += baseIntelligence;
    }

    void TakeBaseStats() {
        playerStatsManager.baseDamage -= weaponDamage;
        playerStatsManager.baseRange -= weaponRange;
        playerStatsManager.meleeSpeed -= baseAttackSpeed;
        playerStatsManager.rangedSpeed -= baseAttackSpeed;
        playerStatsManager.projectileBounce -= baseBounce;
        playerStatsManager.armor -= baseArmor;
        playerStatsManager.strength -= baseStrenght;
        playerStatsManager.intelligence -= baseIntelligence;

    }

    private void OnEnable() {
        playerController.weapon = this;
        GiveBaseStats();
    }

    private void OnDisable() {
        TakeBaseStats();
    }
}
