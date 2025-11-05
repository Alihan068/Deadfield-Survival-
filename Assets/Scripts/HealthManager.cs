using UnityEngine;
using System.Collections;

public class HealthManager : MonoBehaviour {
    [Header("Health Settings")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth;

    [Header("Knockback Settings")]
    public float baseKnockback = 10f;
    [SerializeField] float knockbackResistance = 1f;

    [Header("Status")]
    public bool isUnstoppable = false;

    [Header("Hit Stop Settings")]
    [SerializeField] bool triggersHitStop = true;

    private Rigidbody2D rb2d;
    private StatsManager statsManager;
    private CustomTime customTime;
    private bool isDying = false;

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        statsManager = GetComponent<StatsManager>();
        customTime = GetComponent<CustomTime>();
        currentHealth = maxHealth;
    }

    public void CalculateIncomingDamage(float incomingDamage) {
        if (statsManager != null && !statsManager.canBeDamaged)
            return;

        if (isDying) return; // Already dying, ignore further damage

        // Apply damage reduction/armor
        float finalDamage = incomingDamage;
        if (statsManager != null) {
            finalDamage = incomingDamage / statsManager.armor;
            finalDamage *= (1f - (statsManager.damageReduction / 100f));
        }

        // Apply damage
        currentHealth -= finalDamage;

        //Debug.Log($"{gameObject.name} took {finalDamage} damage. Health: {currentHealth}/{maxHealth}");

        // Check if should die
        bool shouldDie = currentHealth <= 0;

        // Trigger hit stop effect AFTER applying damage
        if (triggersHitStop && HitStopManager.Instance != null) {
            float hitStopDuration = HitStopManager.Instance.CalculateFreezeDuration(finalDamage);
            HitStopManager.Instance.TriggerHitStop(finalDamage);

            // If should die, delay death until after hit stop
            if (shouldDie) {
                isDying = true;
                StartCoroutine(DelayedDeath(hitStopDuration + 0.05f));
            }
        }
        else if (shouldDie) {
            // No hit stop, die immediately
            Die();
        }
    }

    IEnumerator DelayedDeath(float delay) {
        // Wait for hit stop to complete (plus small buffer)
        yield return new WaitForSecondsRealtime(delay + 0.05f);
        Die();
    }

    public void GetKnockback(Transform attacker, float attackerStrength) {
        if (isUnstoppable || rb2d == null)
            return;

        // Calculate knockback direction
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;

        // Calculate knockback force
        float knockbackForce = baseKnockback + attackerStrength;
        if (statsManager != null) {
            knockbackForce /= statsManager.strength; // Use strength as knockback resistance
        }

        // Apply knockback
        rb2d.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Calculate and schedule knockback to be applied after unfreeze
    /// </summary>
    public void ScheduleKnockback(Transform attacker, float attackerStrength) {
        if (isUnstoppable || rb2d == null || customTime == null)
            return;

        // Calculate knockback direction
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;

        // Calculate knockback velocity (much stronger than before!)
        // Base knockback speed + attacker strength, reduced by defender strength
        float knockbackSpeed = baseKnockback + (attackerStrength * 2f);

        if (statsManager != null && statsManager.strength > 1f) {
            // Reduce by defender strength (but not below 50% of base)
            knockbackSpeed = Mathf.Max(knockbackSpeed * 0.5f, knockbackSpeed / Mathf.Sqrt(statsManager.strength));
        }

        // Calculate final knockback velocity vector
        Vector2 knockbackVelocity = knockbackDirection * knockbackSpeed;

        // Schedule it to be applied when unfrozen
        customTime.ApplyKnockbackOnUnfreeze(knockbackVelocity);

        //Debug.Log($"{gameObject.name} knockback scheduled: direction={knockbackDirection}, speed={knockbackSpeed}, velocity={knockbackVelocity}");
    }

    void Die() {
        Debug.Log($"{gameObject.name} died!");
        // Add death logic here (animations, loot drops, etc.)
        Destroy(gameObject);
    }

    public void Heal(float amount) {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetHealthPercentage() {
        return currentHealth / maxHealth;
    }
}
