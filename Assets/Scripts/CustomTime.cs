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
    private Vector2 pendingKnockback = Vector2.zero;
    private bool hasPendingKnockback = false;

    // Reference to enemy controller if this is an enemy
    private EnemyController enemyController;
    HealthManager healthManager;
    StatsManager statsManager;

    [SerializeField] float knockbackDuration = 0.2f; // How long knockback lasts

    void Awake() {
        statsManager = GetComponent<StatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();     
        enemyController = GetComponent<EnemyController>();
        healthManager = GetComponent<HealthManager>();
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

        if (rb2d != null && wasFrozen) {
            // Restore physics constraints
            rb2d.constraints = savedConstraints;

            // Apply pending knockback if any
            if (hasPendingKnockback) {
                // Don't restore old velocity - set to zero so knockback is clean
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;

                //Debug.Log($"{gameObject.name} applying pending knockback: {pendingKnockback}");
                rb2d.linearVelocity = pendingKnockback; // Set velocity directly

                // Disable enemy AI during knockback
                if (enemyController != null) {
                    StartCoroutine(KnockbackPause());
                }

                pendingKnockback = Vector2.zero;
                hasPendingKnockback = false;
            }
            else {
                // No knockback, restore saved velocity
                rb2d.linearVelocity = savedVelocity;
                rb2d.angularVelocity = savedAngularVelocity;
            }
        }

        foreach (var anim in animators) {
            if (anim != null)
                anim.speed = 1f;
        }

        wasFrozen = false;
    }
    public void ApplyKnockbackOnUnfreeze(Vector2 knockbackVelocity) {
        pendingKnockback = knockbackVelocity;
        hasPendingKnockback = true;
        //Debug.Log($"{gameObject.name} knockback scheduled: {knockbackVelocity}");
    }

    public void GetKnockback( float amount, Transform source) {
        Debug.Log(this.name + ("Got knockbacked by the amount: ") + amount);
        if (rb2d == null) {
            Debug.LogError("Rigidbody2D is null!");
            return;
        }
        if (statsManager.isUnstoppable) {
            Debug.Log(this.name + ("isUnstopabble!"));
            return;
        }
        //TODO: convert to  both player and enemy controller to, interrupt method in the controller scripts if any additions will be added.
        if (statsManager.isPlayer) {
          GetComponent<PlayerController>().StopAllCoroutines();
        }
        else {
            enemyController.StopAllCoroutines();
        }
        rb2d.linearVelocity = Vector2.zero;

        Vector2 knockbackDirection = (rb2d.position - (Vector2)source.position).normalized;

        float knockbackStrCompare = amount - statsManager.strength;

        rb2d.AddForce(knockbackDirection * GeneralCalculations.LogarithmicScale(knockbackStrCompare, 50) * statsManager.baseKnockback, ForceMode2D.Impulse);

        if (!statsManager.isKnocked) StartCoroutine(KnockbackPause());
    }
    IEnumerator KnockbackPause() {
        statsManager.isKnocked = true;
        statsManager.canMove = false;
        float knockStun = statsManager.knockbackStagger; /*(statsManager.strength / 100)*/
        yield return new WaitForSeconds(GeneralCalculations.ClampedValue(knockStun));

        statsManager.canMove = true;
        statsManager.isKnocked = false;
    }


}