using System.Collections;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Cinemachine.CinemachineTargetGroup;
using static UnityEngine.GraphicsBuffer;

public class HealthManager : MonoBehaviour
{

    Coroutine knockbackCoroutine;
    Coroutine coroutine;
    StatsManager statsManager;
    PlayerController playerController;
    Rigidbody2D rb2d;
    EnemyController enemyController;

    public float maxHealthPoint = 100;
    public float knockbackMultiplier = 10f;
    [SerializeField] float knockbackStagger = 0.15f;
    public bool isUnstoppable = false;
    bool isKnocked;

    [SerializeField] float deathSpin = 10f;
    [SerializeField] float deathKick = 10f;

    private void OnEnable() {
        statsManager = GetComponent<StatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        statsManager = GetComponent<StatsManager>();
        if (statsManager.isPlayer) { 
            playerController = GetComponent<PlayerController>(); 
        } else {
            enemyController = GetComponent<EnemyController>();
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
        if (isUnstoppable) {
            Debug.Log(this.name + ("isUnstopabble!"));
            return;
        }
        //TODO: convert to interrupt method in the controller scripts if any additions will be added.
        if (statsManager.isPlayer) {
            playerController.StopAllCoroutines();
        }
        else {
            enemyController.StopAllCoroutines();
        }
        rb2d.linearVelocity = Vector2.zero;

        Vector2 knockbackDirection = (rb2d.position - (Vector2)source.position).normalized;
        rb2d.AddForce(knockbackDirection * (amount - statsManager.strength) * knockbackMultiplier, ForceMode2D.Impulse);

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

    IEnumerator KnockbackPause() {
        
        isKnocked = true;
        var enemy = GetComponent<EnemyController>();
        if (enemy != null) enemy.canMove = false;
        float knockStun = knockbackStagger - (statsManager.strength / 100);
        yield return new WaitForSeconds(Mathf.Clamp(knockStun,0,float.MaxValue));

        if (enemy != null) enemy.canMove = true;
        isKnocked = false;
    }

    void DeathSequence() {

        DeathEffects();
        Destroy(gameObject, 3f);
    }

    void DeathEffects() {
        if (statsManager.isPlayer) {
            //Stop Camera on death area
            FindAnyObjectByType<CinemachineCamera>().enabled = false;
        }

        //RedColorBlink
        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke(nameof(ResetSpriteColor), 0.2f);
        //Disable Colliders
        Collider2D[] collider2Ds = GetComponents<Collider2D>();
        foreach (Collider2D col in collider2Ds) {
            col.enabled = false;
        }
        //DeathSpin
        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        rb2d.linearVelocity = Vector2.up * deathKick;
        rb2d.freezeRotation = false;
        rb2d.AddTorque(deathSpin, ForceMode2D.Impulse);

        Invoke(nameof(StopSpin), 2f);

        Invoke(nameof(DestroyPlayer), 5f);
    }
    void ResetSpriteColor() {
        GetComponent<SpriteRenderer>().color = Color.white;
    }
    void StopSpin() {
        rb2d.angularVelocity = 0f;
    }
    void DestroyPlayer() {
        Destroy(gameObject);
    }
}
