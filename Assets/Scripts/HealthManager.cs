using UnityEngine;

public class HealthManager : MonoBehaviour
{
    StatsManager statsManager;
    PlayerController controller;
    public float maxHealthPoint = 100;
    private void OnEnable() {
        statsManager = GetComponent<StatsManager>();
    }
    void Start()
    {
        statsManager = GetComponent<StatsManager>();
        if (statsManager.isPlayer) { 
            controller = GetComponent<PlayerController>(); 
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CalculateIncomingDamage(float rawDamage) {
        float calculatedDamage;
        calculatedDamage = rawDamage;
        TakeFinalDamage(calculatedDamage);
    }
    void TakeFinalDamage(float damage) {
        if (damage > maxHealthPoint) {
            DeathSequence();
        }

        else {
            maxHealthPoint -= damage;
            Debug.Log("Player took " + damage + "damage! \nRemaining hp: " + maxHealthPoint);
        }
    }

    void DeathSequence() {
        controller.canMove = false;
    }

    
}
