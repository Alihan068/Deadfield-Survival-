using System.Collections;
using UnityEngine;
using static Unity.Cinemachine.CinemachineTargetGroup;
using static UnityEngine.GraphicsBuffer;

public class HealthManager : MonoBehaviour
{

    Coroutine knockbackCoroutine;
    StatsManager statsManager;
    PlayerController controller;
    Rigidbody2D rb2d;
    EnemyController enemyController;

    public float maxHealthPoint = 100;
    public float knockbackAmount = 10f;
    [SerializeField] float knockbackStagger = 0.15f;
    bool isKnocked;

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

    public void CalculateIncomingDamage(float rawDamage) {
        float calculatedDamage;
        calculatedDamage = rawDamage;
        TakeFinalDamage(calculatedDamage);
    }

    public void GetKnockback(Transform source, float amount) {
        if (rb2d == null) {
            Debug.LogError("Rigidbody2D is null!");
            return;
        }

        rb2d.linearVelocity = Vector2.zero;

        Vector2 knockbackDirection = (rb2d.position - (Vector2)source.position).normalized;
        rb2d.AddForce(knockbackDirection * amount * knockbackAmount, ForceMode2D.Impulse);

        if (!isKnocked) StartCoroutine(KnockbackPause());
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
        Destroy(gameObject, 3);
    }
    IEnumerator KnockbackPause() {
        isKnocked = true;
        var enemy = GetComponent<EnemyController>();
        if (enemy != null) enemy.canMove = false;

        yield return new WaitForSeconds(knockbackStagger);

        if (enemy != null) enemy.canMove = true;
        isKnocked = false;
    }

}
