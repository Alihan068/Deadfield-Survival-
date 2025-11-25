using System.Collections;
using System.ComponentModel;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
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

    void OnEnable()
    {
        statsManager = GetComponent<StatsManager>();
        weapon = GetComponentInChildren<Weapon>();
        audioSource = GetComponent<AudioSource>();

        if (!statsManager.isPlayer)
        {
            enemyController = GetComponent<EnemyController>();
            rb2d = GetComponent<Rigidbody2D>();
            customTime = GetComponent<CustomTime>();
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
            difficulityManager = FindFirstObjectByType<DifficulityManager>();

            // Enemy max health scaled by difficulty
            if (difficulityManager != null)
            {
                statsManager.maxHealth = statsManager.maxHealth * difficulityManager.enemyHealthMultiplier;
            }
        }

        statsManager.currentHealth = statsManager.maxHealth;
    }

    void Start()
    {
        statsManager = GetComponent<StatsManager>();
        UpdateMaxHp();

        if (statsManager.isPlayer)
        {
            PlayerHealthBarUpdate();
            playerController = GetComponent<PlayerController>();
            rb2d = GetComponent<Rigidbody2D>();
            customTime = GetComponent<CustomTime>();
        }
    }

    void Update()
    {
        if (statsManager.isPlayer)
        {
            PlayerHealthBarUpdate();
        }
    }

    public void CalculateIncomingDamage(float rawDamage)
    {
        Debug.Log(this.name + " incoming damage: " + rawDamage);
        float damage = rawDamage;

        // Difficulty scaling:
        if (statsManager.isPlayer && difficulityManager != null)
        {
            damage *= difficulityManager.enemyDamageMultiplier;
        }

        // EffectiveDamageReduction is in percent: -100..95
        float reductionPercent = statsManager.EffectiveDamageReduction;
        damage *= (1f - (reductionPercent / 100f));

        // Prevent negative damage in case of extreme values
        if (damage < 0f)
        {
            damage = 0f;
        }

        TakeFinalDamage(damage);
    }

    //For more complex resistance logic if needed later
    void CalculateResistance(float damage)
    {
    }

    void TakeFinalDamage(float damage)
    {
        Debug.Log(this.name + " takes " + damage + " damage.");
        if (!statsManager.canBeDamaged)
        {
            Debug.Log(this.name + " can't be Damaged");
            return;
        }

        if (damage >= statsManager.currentHealth)
        {
            DeathSequence();
        }
        else
        {
            if (statsManager.triggersHitStop && HitStopManager.Instance != null)
            {
                float hitStopDuration = HitStopManager.Instance.CalculateFreezeDuration(damage);
                HitStopManager.Instance.TriggerHitStop(damage);
            }

            if (enemyController != null)
            {
                enemyController.BreakCharge();
            }

            StartCoroutine(TakeDamageEffects());
            statsManager.currentHealth -= damage;

            if (statsManager.isPlayer)
            {
                PlayerHealthBarUpdate();
            }
        }
    }

    void PlayerHealthBarUpdate()
    {
        if (healthText != null)
        {
            healthText.text = statsManager.currentHealth + " : " + statsManager.maxHealth;
        }

        if (healthSlider != null)
        {
            healthSlider.value = statsManager.currentHealth;
        }
    }

    public void UpdateMaxHp()
    {
        statsManager.maxHealth = statsManager.baseMaxHealth + statsManager.extraHealth;

        statsManager.currentHealth = Mathf.Clamp(statsManager.currentHealth, 0f, statsManager.maxHealth);
    }

    IEnumerator TakeDamageEffects()
    {
        if (takeDamageSounds.Length > 0 && audioSource != null)
        {
            audioSource.PlayOneShot(takeDamageSounds[UnityEngine.Random.Range(0, takeDamageSounds.Length)]);
        }

        Color previousColor = bodySprite.color;
        bodySprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        bodySprite.color = previousColor;
    }

    public void ApplyKnockback(float amount, Transform source)
    {
        if (rb2d == null)
        {
            Debug.LogError(this.name + " Rigidbody2D is null! Health: " + statsManager.currentHealth);
            return;
        }

        if (statsManager.isUnstoppable)
        {
            Debug.Log(this.name + " isUnstopabble!");
            return;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;

            statsManager.canMove = true;
            statsManager.isKnocked = false;
        }

        if (statsManager.isPlayer)
        {
            if (playerController != null)
            {
                playerController.StopAllCoroutines();
            }
        }
        else
        {
            if (enemyController != null)
            {
                enemyController.StopAllCoroutines();
            }
        }

        rb2d.linearVelocity = Vector2.zero;

        Vector2 knockbackDirection = (rb2d.position - (Vector2)source.position).normalized;
        float knockbackStrCompare = amount - statsManager.knockBack;

        if (statsManager.triggersHitStop && HitStopManager.Instance != null)
        {
            customTime.ApplyKnockbackOnUnfreeze(knockbackStrCompare, knockbackDirection);
        }
        else
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(KnockbackPause());

                float clampedKnockback = Mathf.Clamp(knockbackStrCompare, 0f, 10f);
                float scaledExtraKnockback = GeneralCalculations.LogarithmicScale(clampedKnockback, 10f);

                rb2d.AddForce(
                    knockbackDirection * (scaledExtraKnockback + statsManager.baseAppliedKnockback),
                    ForceMode2D.Impulse
                );
            }
        }
    }

    public IEnumerator KnockbackPause()
    {
        statsManager.isKnocked = true;
        statsManager.canMove = false;

        float knockStun = statsManager.knockbackStagger;
        yield return new WaitForSeconds(GeneralCalculations.ClampedValue(knockStun));

        statsManager.canMove = true;
        statsManager.isKnocked = false;
        knockbackCoroutine = null;
    }

    void DeathSequence()
    {
        if (statsManager != null)
        {
            if (!statsManager.isPlayer)
            {
                if (enemySpawner == null)
                {
                    enemySpawner = FindFirstObjectByType<EnemySpawner>();
                }

                if (enemySpawner != null)
                {
                    enemySpawner.enemyCount--;
                }
            }
            else
            {
                if (playerController != null)
                {
                    playerController.enabled = false;
                }
            }

            if (weapon != null)
            {
                weapon.gameObject.SetActive(false);
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.gameObject.SetActive(false);
                col.enabled = false;
            }

            statsManager.canMove = false;
            statsManager.canAttack = false;

            if (bodySprite != null)
            {
                bodySprite.color = Color.black;
            }
        }
        else
        {
            Debug.LogWarning(this.name + " StatsManager couldn't be found");
        }

        var lootDrop = GetComponent<LootDropOnDeath>();
        if (lootDrop != null)
        {
            lootDrop.IfDestroy();
        }

        Destroy(gameObject, 0.5f);
    }

    IEnumerator HitStop()
    {
        yield return new WaitForSecondsRealtime(0.1f);
    }
}
