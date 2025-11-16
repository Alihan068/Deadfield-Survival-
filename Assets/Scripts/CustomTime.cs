using UnityEngine;
using System.Collections;

public class CustomTime : MonoBehaviour {
    [Range(0f, 1f)]
    public float timeScale = 1f;

    // Custom deltaTime that respects this object's timeScale
    public float DeltaTime => Time.deltaTime * timeScale;

    // For physics-based movement
    public float FixedDeltaTime => Time.fixedDeltaTime * timeScale;

    private Rigidbody2D rb2d;
    private Animator[] animators;
    private Vector2 savedVelocity;
    private float savedAngularVelocity;
    private RigidbodyConstraints2D savedConstraints;
    private bool wasFrozen = false;


    // Store pending knockback to apply after unfreeze
    private float pendingKnockback = 0;
    Vector2 pendingDirection = Vector2.zero;
    private bool hasPendingKnockback = false;

    // Reference to enemy controller if this is an enemy
    private EnemyController enemyController;
    HealthManager healthManager;
    StatsManager statsManager;

    [SerializeField] float knockbackDuration = 0.2f; // How long knockback lasts

    void Awake() {
        statsManager = GetComponent<StatsManager>();

        if (statsManager.isPlayer) {
            rb2d = GetComponent<Rigidbody2D>();
            animators = GetComponentsInChildren<Animator>();
            healthManager = GetComponent<HealthManager>();
        }

    }
    private void OnEnable() {
        statsManager = GetComponent<StatsManager>();

        if (!statsManager.isPlayer) {
            healthManager = GetComponent<HealthManager>();
            rb2d = GetComponent<Rigidbody2D>();
            animators = GetComponentsInChildren<Animator>();
           
        }
    }

    // Freeze this object completely
    public void Freeze() {
        timeScale = 0f;
        wasFrozen = true;

        if (rb2d != null) {
            // Save current physics state
            savedVelocity = rb2d.linearVelocity;
            savedAngularVelocity = rb2d.angularVelocity;
            savedConstraints = rb2d.constraints;

            // Freeze the rigidbody completely
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        foreach (var anim in animators) {
            if (anim != null)
                anim.speed = 0f;
        }
    }

    // Unfreeze this object
    public void Unfreeze() {
        timeScale = 1f;

        if (rb2d != null && wasFrozen && gameObject.activeInHierarchy) {
            // Restore physics constraints
            rb2d.constraints = savedConstraints;

            // Apply pending knockback if any
            if (hasPendingKnockback) {
                // Don't restore old velocity - set to zero so knockback is clean
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;

                // Same logarithmic handling as in HealthManager
                float clampedPendingKnockback = Mathf.Clamp(pendingKnockback, 0f, 10f);
                float scaledExtraPendingKnockback = GeneralCalculations.LogarithmicScale(clampedPendingKnockback, 10f);

                rb2d.AddForce(
                    pendingDirection * (scaledExtraPendingKnockback + statsManager.baseAppliedKnockback),
                    ForceMode2D.Impulse
                );

                //rb2d.linearVelocity = pendingKnockback; // Set velocity directly

                if (healthManager.knockbackCoroutine != null) {
                    StopCoroutine(healthManager.knockbackCoroutine);
                }
                healthManager.knockbackCoroutine = healthManager.StartCoroutine(healthManager.KnockbackPause());

                // Clear pending knockback
                pendingKnockback = 0;
                pendingDirection = Vector2.zero;
                hasPendingKnockback = false;
            }
            else {
                // Restore saved velocity if there is no pending knockback
                rb2d.linearVelocity = savedVelocity;
                rb2d.angularVelocity = savedAngularVelocity;
            }
        }

        foreach (var anim in animators) {
            if (anim != null)
                anim.speed = 1f;
        }

        wasFrozen = false;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;

        //Debug.Log($"{rb2d.linearVelocity}  {rb2d.angularVelocity} {rb2d.constraints}");
    }
    public void ApplyKnockbackOnUnfreeze(float knockbackForce, Vector2 knockbackDirection) {
        if (knockbackForce > 0f) {
            pendingKnockback = knockbackForce;
        }
        else {
            pendingKnockback = 0;
        }
        pendingDirection = knockbackDirection;
        hasPendingKnockback = true;
        //Debug.Log($"{gameObject.name} knockback scheduled: {knockbackForce}");
    }

}