using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    PlayerStatsManager playerStatsManager;
    PlayerController playerController;
    public float playerHp = 100;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
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
        if (damage > playerHp) {
            DeathSequence();
        }

        else {
            playerHp -= damage;
            Debug.Log("Player took " + damage + "damage! \nRemaining hp: " + playerHp);
        }
    }

    void DeathSequence() {
        playerController.canMove = false;
    }

    
}
