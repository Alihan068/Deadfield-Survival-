using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HealthManager : MonoBehaviour
{
    StatsManager statsManager;
    PlayerController controller;
    Rigidbody2D rb2d;
    public float maxHealthPoint = 100;
    public float knockbackAmount = 10f;
    private void OnEnable() {
        statsManager = GetComponent<StatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
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

    public void GetKnockback(Transform resource, float amount) {
        Vector2 toTarget = (Vector2)resource.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.AddForce(-directionNormalized * amount * knockbackAmount, ForceMode2D.Impulse);
    }
    void TakeFinalDamage(float damage) {
        if (damage > maxHealthPoint) {
            DeathSequence();
        }

        else {
            maxHealthPoint -= damage;
            Debug.Log(this.name +" took " + damage + "damage! \nRemaining hp: " + maxHealthPoint);
        }
    }

    void DeathSequence() {
        controller.canMove = false;
        Destroy(gameObject, 3);
    }

    
}
