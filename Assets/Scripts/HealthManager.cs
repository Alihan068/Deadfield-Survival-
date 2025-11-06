using System.Collections;
using System.ComponentModel;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class HealthManager : MonoBehaviour {
    public Coroutine knockbackCoroutine;
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
                
            }
            if (enemyController != null) enemyController.BreakCharge();
            StartCoroutine(TakeDamageEffects());
            statsManager.maxHealthPoint -= damage;
            Debug.Log(this.name + " took " + damage + " damage! \nRemaining hp: " + statsManager.maxHealthPoint);
        }
    }


    IEnumerator TakeDamageEffects() {
        Debug.Log(this.name + "damageEffects");
        Color previousColor = bodySprite.color;
        bodySprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        bodySprite.color = Color.white;
    }

    public void ApplyKnockback(float amount, Transform source) {
        //Debug.Log(this.name + ("Got knockbacked by the amount: ") + amount);
        if (rb2d == null) {
            Debug.LogError("Rigidbody2D is null!");
            return;
        }
        if (statsManager.isUnstoppable) {
            Debug.Log(this.name + ("isUnstopabble!"));
            return;
        }

        if (knockbackCoroutine != null) {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;

            statsManager.canMove = true;
            statsManager.isKnocked = false;
        }

        if (statsManager.isPlayer) {
            GetComponent<PlayerController>().StopAllCoroutines();
        }
        else {
            enemyController.StopAllCoroutines();
        }
        rb2d.linearVelocity = Vector2.zero;

        Vector2 knockbackDirection = (rb2d.position - (Vector2)source.position).normalized;

        float knockbackStrCompare = amount - statsManager.strength;

        if (statsManager.triggersHitStop && HitStopManager.Instance != null) {
            Debug.Log(this.name + "hitstop knockback");
            customTime.ApplyKnockbackOnUnfreeze(knockbackStrCompare, knockbackDirection);
        } else {
            Debug.Log(this.name + "Non hitstop knockback");
            StartCoroutine(KnockbackPause());
            rb2d.AddForce(knockbackDirection * GeneralCalculations.LogarithmicScale(0, Mathf.Clamp(knockbackStrCompare, 0, 10))
                    + statsManager.baseAppliedKnockback * knockbackDirection, ForceMode2D.Impulse);
        }
        //Debug.Log($"{gameObject.name}: {knockbackStrCompare} is sent to next Method");
        //rb2d.AddForce(knockbackDirection * GeneralCalculations.LogarithmicScale(knockbackStrCompare, 50) * statsManager.baseKnockback, ForceMode2D.Impulse);

        //if (!statsManager.isKnocked) StartCoroutine(KnockbackPause());
    }

    public IEnumerator KnockbackPause() {
        statsManager.isKnocked = true;
        statsManager.canMove = false;
        //Debug.Log(this.name + " Knockback Pause");
        float knockStun = statsManager.knockbackStagger; /*(statsManager.strength / 100)*/
        yield return new WaitForSeconds(GeneralCalculations.ClampedValue(knockStun));

        statsManager.canMove = true;
        statsManager.isKnocked = false;
        knockbackCoroutine = null;  // Clear reference
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
