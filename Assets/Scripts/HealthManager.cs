using System.Collections;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class HealthManager : MonoBehaviour {
    Coroutine knockbackCoroutine;
    Coroutine coroutine;
    StatsManager statsManager;
    PlayerController playerController;
    Rigidbody2D rb2d;
    EnemyController enemyController;
    CustomTime customTime;

    [SerializeField] float deathSpin = 10f;
    [SerializeField] float deathKick = 10f;

    [SerializeField] SpriteRenderer bodySprite;

    private void OnEnable() {
        statsManager = GetComponent<StatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
    }
    void Start() {
        statsManager = GetComponent<StatsManager>();
        customTime = GetComponent<CustomTime>();

        if (statsManager.isPlayer) {
            playerController = GetComponent<PlayerController>();
        }
        else {
            enemyController = GetComponent<EnemyController>();
        }

    }

    public void CalculateIncomingDamage(float rawDamage) {
        float calculatedDamage;
        //TODO: Add resistance calculations
        calculatedDamage = rawDamage;
        TakeFinalDamage(calculatedDamage);
    }

    void TakeFinalDamage(float damage) {

        if (!statsManager.canBeDamaged) {
            Debug.Log(this.name + " can't be Damaged");
            return;
        }
        if (damage > statsManager.maxHealthPoint) {
            DeathSequence();
        }
        else {
            if (statsManager.triggersHitStop && HitStopManager.Instance != null) {
                float hitStopDuration = HitStopManager.Instance.CalculateFreezeDuration(damage);
                HitStopManager.Instance.TriggerHitStop(damage);
                //TODO: Separate knockback later
            }
            StartCoroutine(TakeDamageEffects());
            statsManager.maxHealthPoint -= damage;
            Debug.Log(this.name + " took " + damage + " damage! \nRemaining hp: " + statsManager.maxHealthPoint);
        }
    }


    IEnumerator TakeDamageEffects() {
        Debug.Log(this.name + "damageEffects");
        bodySprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        bodySprite.color = Color.white;
    }

    void DeathSequence() {

        DeathEffects();
        statsManager.canMove = false;
        Destroy(gameObject, 1f);
    }

    void DeathEffects() {
        if (statsManager.isPlayer) {
            //Stop Camera on death area
            FindAnyObjectByType<CinemachineCamera>().enabled = false;
        }

        //RedColorBlink
        bodySprite.color = Color.red;
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

  
    IEnumerator HitStop() {
        yield return new WaitForSecondsRealtime(0.1f);
    }
}
