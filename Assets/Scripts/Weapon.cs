using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum WeaponType {
        Melee,
        Ranged,
        Mixed,
    }

   PlayerStatsManager playerStatsManager;

    [SerializeField] float baseDamage = 0f;
    [SerializeField] float baseRange = 0f;
    [SerializeField] float baseAttackSpeed = 0f;
    [SerializeField] float baseBounce = 0f;

    [SerializeField] float baseArmor = 0f;
    [SerializeField] float baseStrenght = 0f;

    [SerializeField] float baseIntelligence = 0f;

    private void Start() {
        playerStatsManager = GetComponentInParent<PlayerStatsManager>();

    }

    void GiveBaseStats() {

    }

    void TaheBaseStats() { 
    
    }
}
