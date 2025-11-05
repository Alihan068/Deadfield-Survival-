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

    public float maxHealthPoint = 100;
    public float baseKnockback = 10f;
    [SerializeField] float knockbackStagger = 0.15f;
    public bool isUnstoppable = false;
    public bool isKnocked;

    [SerializeField] float deathSpin = 10f;
    [SerializeField] float deathKick = 10f;

    [SerializeField] SpriteRenderer bodySprite;

    private void OnEnable() {
        statsManager = GetComponent<StatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
    }
    void Start() {
        statsManager = GetComponent<StatsManager>();
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
        if (damage > maxHealthPoint) {
            DeathSequence();
        }
        else {
            StartCoroutine(TakeDamageEffects());
            maxHealthPoint -= damage;
            Debug.Log(this.name + " took " + damage + " damage! \nRemaining hp: " + maxHealthPoint);
        }
    }
    IEnumerator TakeDamageEffects() {
        Debug.Log(this.name + "damageEffects");
        Color previousColor = bodySprite.color;
        bodySprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        bodySprite.color = Color.white;
    }

    public void GetKnockback(Transform source, float amount) {
        Debug.Log(this.name + ("Got knockbacked by the amount: ") + amount);
        if (rb2d == null) {
            Debug.LogError("Rigidbody2D is null!");
            return;
        }
        if (isUnstoppable) {
            Debug.Log(this.name + ("isUnstopabble!"));
            return;
        }
        //TODO: convert to  both player and enemy controller to, interrupt method in the controller scripts if any additions will be added.
        if (statsManager.isPlayer) {
            playerController.StopAllCoroutines();
        }
        else {
            enemyController.StopAllCoroutines();
        }
        rb2d.linearVelocity = Vector2.zero;

        Vector2 knockbackDirection = (rb2d.position - (Vector2)source.position).normalized;

        float knockbackStrCompare = amount - statsManager.strength;

        rb2d.AddForce(knockbackDirection * LogarithmicScale(knockbackStrCompare, 50) * baseKnockback, ForceMode2D.Impulse);

        if (!isKnocked) StartCoroutine(KnockbackPause());
    }
    IEnumerator KnockbackPause() {
        isKnocked = true;
        statsManager.canMove = false;
        float knockStun = knockbackStagger; /*(statsManager.strength / 100)*/
        yield return new WaitForSeconds(ClampedValue(knockStun));

        statsManager.canMove = true;
        isKnocked = false;
    }



    void DeathSequence() {

        DeathEffects();
        statsManager.canMove = false;
        Destroy(gameObject, 3f);
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

    float ClampedValue(float value) {
        value = Mathf.Clamp(value, 0, float.MaxValue);
        return value;
    }

    float ToPercent(float value, float max) {
        if (max <= Mathf.Epsilon)
            return 0f;

        float ratio = value / max;
        ratio = Mathf.Clamp01(ratio);
        return ratio * 100f;
    }
    public float LogarithmicScale(float baseValue, float maxLimit) {
        if (baseValue <= 0f)
            return 0f;
        if (maxLimit <= 0f)
            return baseValue;

        float scaled = maxLimit * (1f - Mathf.Exp(-baseValue / maxLimit));
        Debug.Log(1 + ((scaled) / 100));
        return 1 + ((scaled) / 100);
    }

    IEnumerator HitStop() {
        yield return new WaitForSecondsRealtime(0.1f);
    }
}
