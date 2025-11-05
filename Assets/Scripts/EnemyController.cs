using System.Collections;
using UnityEngine;

public enum EnemyType {
    Melee,
    Ranged,
    Charger,
}

public class EnemyController : MonoBehaviour {

    [SerializeField] EnemyType enemyType;
    [SerializeField] float attackZoneValue;

    float minRange;
    [SerializeField] float chargeSpeed = 2f;
    [SerializeField] float chargeCooldown = 2f;
    [SerializeField] float chargeTime = 2f;

    StatsManager statsManager;
    Rigidbody2D rb2d;
    CustomTime customTime;
    Coroutine chargeCoroutine;
    PlayerController playerController;
    HealthManager healthManager;
    RangedParticleAttack rangedParticleAttack;
    [SerializeField] SpriteRenderer characterSprite;
    Weapon weaponScript;

    [SerializeField] Animator weaponAnimator;
    Animator bodyAnimator;

    Vector3 rotationToPlayer;
    float distanceToPlayer;

    // Knockback state
    public bool isInKnockback = false;

    void Awake() {
        statsManager = GetComponent<StatsManager>();
        customTime = GetComponent<CustomTime>();
    }

    void OnEnable() {
        statsManager = GetComponent<StatsManager>();
        healthManager = GetComponent<HealthManager>();
        rb2d = GetComponent<Rigidbody2D>();
        bodyAnimator = GetComponent<Animator>();

        weaponScript = GetComponentInChildren<Weapon>();
        rangedParticleAttack = GetComponentInChildren<RangedParticleAttack>();

        playerController = FindAnyObjectByType<PlayerController>();
        EnemyBaseStatImplementation(enemyType);
    }

    void Update() {
        // Don't move if frozen, can't move, or in knockback
        if (!statsManager.canMove || customTime.timeScale <= 0 || isInKnockback) return;

        CalculateDistanceToPlayer();
        EnemyMoveBehavior(enemyType);
    }

    void CalculateDistanceToPlayer() {
        rotationToPlayer = (transform.position - playerController.transform.position).normalized;
        distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
    }

    void FlipEnemyFacing() {
        if (playerController == null || rb2d == null || characterSprite == null) return;

        if (rb2d.linearVelocity.sqrMagnitude <= Mathf.Epsilon) {
            float dx = playerController.transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) > Mathf.Epsilon) {
                characterSprite.flipX = dx < 0f;
            }
        }

        float vx = rb2d.linearVelocity.x;
        if (vx > Mathf.Epsilon) {
            characterSprite.flipX = false;
        }
        else if (vx < -Mathf.Epsilon) {
            characterSprite.flipX = true;
        }
    }

    void EnemyMoveBehavior(EnemyType enemyType) {
        switch (enemyType) {
            case EnemyType.Melee:
                MeleeEnemyMovement();
                break;
            case EnemyType.Ranged:
                RangedEnemyMovement();
                break;
            case EnemyType.Charger:
                ChargerMovement();
                break;
        }
        FlipEnemyFacing();
    }

    void MeleeEnemyMovement() {
        if (distanceToPlayer <= statsManager.baseRange) {
            rb2d.linearVelocity = Vector2.zero;
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger("isAttacking");
        }
        else if (distanceToPlayer > statsManager.baseRange) {
            Vector2 toTarget = (Vector2)playerController.gameObject.transform.position - rb2d.position;
            Vector2 directionNormalized = toTarget.normalized;

            // Only apply velocity if not frozen
            if (customTime.timeScale > 0) {
                rb2d.linearVelocity = directionNormalized * statsManager.moveSpeed;
            }
        }
    }

    void RangedEnemyMovement() {
        if (distanceToPlayer <= minRange) {
            if (rangedParticleAttack != null)
                rangedParticleAttack.ParticleSystemToggle(false);
            RunFromTarget(playerController.transform);
            return;
        }
        else if (distanceToPlayer >= statsManager.baseRange) {
            if (rangedParticleAttack != null)
                rangedParticleAttack.ParticleSystemToggle(false);
            ChaseTarget(playerController.transform);
            return;
        }
        else {
            if (rangedParticleAttack != null)
                rangedParticleAttack.ParticleSystemToggle(true);
            rb2d.linearVelocity = Vector2.zero;
        }
    }

    void AnimationHandler() {

        if (rb2d.linearVelocity != Vector2.zero) {
            bodyAnimator.SetBool("isWalking", true);
        }
        else {
            bodyAnimator.SetBool("isWalking", false);
        }
    }

    void ChargerMovement() {
        if (distanceToPlayer <= statsManager.baseRange) {
            if (chargeCoroutine == null) {
                chargeCoroutine = StartCoroutine(ChargeTarget(playerController.transform));
            }
            return;
        }
        else if (distanceToPlayer > statsManager.baseRange) {
            if (chargeCoroutine == null) {
                ChaseTarget(playerController.transform);
                return;
            }
        }
    }

    IEnumerator ChargeTarget(Transform target) {
        rb2d.linearVelocity = Vector2.zero;
        Debug.Log("Wait");
        Vector2 direction = ((Vector2)target.position - rb2d.position).normalized;

        // Use real time for wait so it's not affected by freezes
        yield return new WaitForSecondsRealtime(1);

        healthManager.isUnstoppable = true;
        Debug.Log("Charge!");

        if (customTime.timeScale > 0) {
            rb2d.linearVelocity = (direction.normalized * statsManager.moveSpeed * chargeSpeed);
        }

        yield return new WaitForSecondsRealtime(chargeTime);

        healthManager.isUnstoppable = false;
        rb2d.linearVelocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(chargeCooldown);
        chargeCoroutine = null;
    }

    void ChaseTarget(Transform target) {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;

        // Only apply velocity if not frozen
        if (customTime.timeScale > 0) {
            rb2d.linearVelocity = directionNormalized * statsManager.moveSpeed;
        }
    }

    void RunFromTarget(Transform target) {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;

        // Only apply velocity if not frozen
        if (customTime.timeScale > 0) {
            rb2d.linearVelocity = -directionNormalized * statsManager.moveSpeed;
        }
    }

    void EnemyBaseStatImplementation(EnemyType enemyType) {
        switch (enemyType) {
            case EnemyType.Melee:
                statsManager.strength += 5;
                break;
            case EnemyType.Ranged:
                minRange = statsManager.baseRange;
                statsManager.baseRange += attackZoneValue;
                break;
            case EnemyType.Charger:
                healthManager.baseKnockback = 50;
                rb2d.mass += 10;
                statsManager.strength += 5;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision != null && collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponentInChildren<HealthManager>().CalculateIncomingDamage(statsManager.baseDamage);
            collision.gameObject.GetComponentInChildren<HealthManager>().GetKnockback(transform, statsManager.strength);
        }
    }

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(this.transform.position, statsManager.baseRange);
    //}
}
