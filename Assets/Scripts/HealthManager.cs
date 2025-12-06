using System.Collections;
using System.ComponentModel;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour {
    public Coroutine knockbackCoroutine;
    Coroutine coroutine;
    StatsManager statsManager;
    PlayerController playerController;
    Rigidbody2D rb2d;
    EnemyController enemyController;
    CustomTime customTime;
    EnemySpawner enemySpawner;
    Weapon weapon;

    [SerializeField] float deathSpin = 10f;
    [SerializeField] float deathKick = 10f;

    [SerializeField] SpriteRenderer bodySprite;

    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI healthText;

    DifficulityManager difficulityManager;
    //[SerializeField] Image healthFill;

    AudioSource audioSource;
    [SerializeField] AudioClip[] takeDamageSounds;
    [SerializeField] AudioClip[] deathSounds;

     void OnEnable() {
        statsManager = GetComponent<StatsManager>();
        weapon = GetComponentInChildren<Weapon>();
        audioSource = GetComponent<AudioSource>();
        if (!statsManager.isPlayer) {
            enemyController = GetComponent<EnemyController>();
            rb2d = GetComponent<Rigidbody2D>();
            customTime = GetComponent<CustomTime>();
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
            difficulityManager = FindFirstObjectByType<DifficulityManager>();
            statsManager.maxHealth = statsManager.maxHealth * difficulityManager.enemyHealthMultiplier;
        }
        statsManager.currentHealth = statsManager.maxHealth;
    }

    void Start() {
        statsManager = GetComponent<StatsManager>();
        UpdateMaxHp();
        // statsManager.currentHealth = statsManager.maxHealth;

        if (statsManager.isPlayer) {
            PlayerHealthBarUpdate();
            playerController = GetComponent<PlayerController>();
            rb2d = GetComponent<Rigidbody2D>();
            customTime = GetComponent<CustomTime>();
        }
    }
     void Update() {

        if (statsManager.isPlayer) {
            PlayerHealthBarUpdate();
        }
    }

    public void CalculateIncomingDamage(float rawDamage) {
        float calculatedDamage;
        //TODO: Add resistance calculations
        calculatedDamage = rawDamage;
        TakeFinalDamage(calculatedDamage);
    }

    void CalculateEvasion() {

    }

    void TakeFinalDamage(float damage) {
        if (!statsManager.canBeDamaged) {
            Debug.Log(this.name + " can't be Damaged");
            return;
        }
        if (damage > statsManager.currentHealth) {
            DeathSequence();
        }
        else {

            if (statsManager.triggersHitStop && HitStopManager.Instance != null) {
                float hitStopDuration = HitStopManager.Instance.CalculateFreezeDuration(damage);
                HitStopManager.Instance.TriggerHitStop(damage);

            }
            if (enemyController != null) enemyController.BreakCharge();

            StartCoroutine(TakeDamageEffects());
            statsManager.currentHealth -= damage;
            //Debug.Log(this.name + " took " + damage + " damage! \nRemaining hp: " + statsManager.currentHp);

            if (statsManager.isPlayer) {
                PlayerHealthBarUpdate();
            }
        }
    }

    void PlayerHealthBarUpdate() {
        healthText.text = statsManager.currentHealth + " : " + statsManager.maxHealth;
        healthSlider.value = statsManager.currentHealth;
    }

    public void UpdateMaxHp() {
        statsManager.maxHealth = statsManager.baseMaxHealth + statsManager.extraHealth;
    }
    IEnumerator TakeDamageEffects() {
        //Debug.Log(this.name + "damageEffects");
        if (takeDamageSounds.Length > 0 && audioSource != null) {
            audioSource.PlayOneShot(takeDamageSounds[UnityEngine.Random.Range(0, takeDamageSounds.Length)]);
        }
        Color previousColor = bodySprite.color;
        bodySprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        bodySprite.color = Color.white;
    }

    public void ApplyKnockback(float amount, Transform source) {
        //Debug.Log(this.name + ("Got knockbacked by the amount: ") + amount);
        if (rb2d == null) {
            Debug.LogError(this.name + "Rigidbody2D is null!" + " Health: " + statsManager.currentHealth);
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
            playerController.StopAllCoroutines();
        }
        else {
            enemyController.StopAllCoroutines();
        }

        rb2d.linearVelocity = Vector2.zero;

        Vector2 knockbackDirection = (rb2d.position - (Vector2)source.position).normalized;

        float knockbackStrCompare = amount - statsManager.knockBack;

        if (statsManager.triggersHitStop && HitStopManager.Instance != null) {
            //Debug.Log(this.name + "hitstop knockback");
            customTime.ApplyKnockbackOnUnfreeze(knockbackStrCompare, knockbackDirection);
        }
        else {
            //Debug.Log(this.name + "Non hitstop knockback");
            StartCoroutine(KnockbackPause());

            // Use the difference (amount - resistance) as the baseValue for the logarithmic scale
            float clampedKnockback = Mathf.Clamp(knockbackStrCompare, 0f, 10f);
            float scaledExtraKnockback = GeneralCalculations.LogarithmicScale(clampedKnockback, 10f);

            // Base knockback + logarithmically scaled bonus
            rb2d.AddForce(
                knockbackDirection * (scaledExtraKnockback + statsManager.baseAppliedKnockback),
                ForceMode2D.Impulse
            );
        }

        //Debug.Log($"{gameObject.name}: {knockbackStrCompare} is sent to next Method");
        //rb2d.AddForce(knockbackDirection * GeneralCalculations...Compare, 50) * statsManager.baseKnockback, ForceMode2D.Impulse);

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
        if (statsManager != null) {
            if (!statsManager.isPlayer) { enemySpawner.enemyCount--; }
            else { playerController.enabled = false; }
            //Debug.Log(this.name + "isDead!");
            if (weapon != null) weapon.gameObject.SetActive(false);
            
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in colliders) {
                col.gameObject.SetActive(false); col.enabled = false;
            }
            statsManager.canMove = false;
            statsManager.canAttack = false;
            
            bodySprite.color = Color.black;
        }
        else {
            Debug.LogWarning(this.name + "StatsManager Couldn't Found");
        }

        GetComponent<LootDropOnDeath>().IfDestroy();
        Destroy(gameObject, 0.5f);

    }

    IEnumerator HitStop() {
        yield return new WaitForSecondsRealtime(0.1f);
    }
}
