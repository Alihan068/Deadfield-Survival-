using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType {
    Melee,
    Ranged,
    Charger,
}
public class EnemyController : MonoBehaviour {

    //[SerializeField] CollectibleItemSO baseStats;
    [SerializeField] EnemyType enemyType;

    [SerializeField] float rangeBuffer;
    [SerializeField] float attackZoneValue;

    float minRange;
    [SerializeField] float chargeSpeed = 2f;
    [SerializeField] float chargeCooldown = 2f;
    [SerializeField] float chargeTime = 2f;

    StatsManager statsManager;
    Rigidbody2D rb2d;
    Coroutine chargeCoroutine;
    PlayerController playerController;
    HealthManager healthManager;
    RangedParticleAttack rangedParticleAttack;
    CustomTime customTime;
    [SerializeField] SpriteRenderer characterSprite;
    Weapon weaponScript;

    [SerializeField] Animator weaponAnimator;
    Animator bodyAnimator;

    string attackAnimName = "ChargeAttack";
    int attackAnimHash;

    Vector3 rotationToPlayer;
    float distanceToPlayer;

    void Awake() {
        statsManager = GetComponent<StatsManager>();
    }

    void OnEnable() {
        statsManager = GetComponent<StatsManager>();
        healthManager = GetComponent<HealthManager>();
        customTime = GetComponent<CustomTime>();
        rb2d = GetComponent<Rigidbody2D>();
        bodyAnimator = GetComponent<Animator>();

        weaponScript = GetComponentInChildren<Weapon>();
        rangedParticleAttack = GetComponentInChildren<RangedParticleAttack>();

        playerController = FindAnyObjectByType<PlayerController>();
        EnemyBaseStatImplementation(enemyType);

        attackAnimHash = Animator.StringToHash(attackAnimName);
    }

    void FixedUpdate() {
        if (!statsManager.canMove || statsManager.isKnocked || customTime.timeScale <= 0) return;

        CalculateDistanceToPlayer();
        EnemyMoveBehavior(enemyType);

    }
    void CalculateDistanceToPlayer() {
        rotationToPlayer = (transform.position - playerController.transform.position).normalized;
        distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
    }

    void FlipEnemyFacing() {
        if (playerController == null || rb2d == null || characterSprite == null) return;

        float vx = rb2d.linearVelocity.x;
        const float dead = 0.05f;

        if (Mathf.Abs(vx) <= dead && rb2d.linearVelocity.sqrMagnitude <= dead * dead) {
            float dx = playerController.transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) > dead) characterSprite.flipX = dx < 0f;
        }
        else if (vx < -dead) {
            characterSprite.flipX = true;
        }
        else if (vx > dead) {
            characterSprite.flipX = false;
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
    //TODO: Make enemy attack based on meleeAttackSpeed by setting the animation speed.
    void MeleeEnemyMovement() {
        if (distanceToPlayer <= statsManager.baseRange) {
            rb2d.linearVelocity = Vector2.zero;

            float attackSpeed = Mathf.Max(0.01f, statsManager.meleeAttackSpeed);
            weaponAnimator.SetFloat("attackSpeed", attackSpeed);

            weaponAnimator.SetBool("isAttacking", true);
        }
        else if (distanceToPlayer > statsManager.baseRange) {
            weaponAnimator.SetBool("isAttacking", false);
            Vector2 toTarget = (Vector2)playerController.gameObject.transform.position - rb2d.position;
            Vector2 directionNormalized = toTarget.normalized;
            rb2d.linearVelocity = directionNormalized * statsManager.moveSpeed;
        }
    }

    void RangedEnemyMovement() {
        if (distanceToPlayer <= minRange - rangeBuffer) {
            rangedParticleAttack.ParticleSystemToggle(false);
            RunFromTarget(playerController.transform);
            return;
        }
        else if (distanceToPlayer >= statsManager.baseRange + rangeBuffer) {
            rangedParticleAttack.ParticleSystemToggle(false);
            ChaseTarget(playerController.transform);
            return;
        }
        else {
            rangedParticleAttack.ParticleSystemToggle(true);
            rb2d.linearVelocity = Vector2.Lerp(
                rb2d.linearVelocity,
                Vector2.zero,
                0.30f);
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
        rb2d.linearVelocity = Vector2.Lerp(
            rb2d.linearVelocity,
            Vector2.zero,
            0.30f);
        Debug.Log("Wait");
        Vector2 direction = ((Vector2)target.position - rb2d.position).normalized;
        yield return new WaitForSeconds(1);
        statsManager.isUnstoppable = true;
        Debug.Log("Charge!");
        rb2d.linearVelocity = (direction.normalized * statsManager.moveSpeed * chargeSpeed);
        yield return new WaitForSeconds(chargeTime);
        statsManager.isUnstoppable = false;
        rb2d.linearVelocity = Vector2.Lerp(
            rb2d.linearVelocity,
            Vector2.zero,
            0.30f);
        yield return new WaitForSeconds(chargeCooldown);
        chargeCoroutine = null;
    }

    void ChaseTarget(Transform target) {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        //rb2d.MovePosition(rb2d.position + directionNormalized * statsManager.moveSpeed * Time.fixedDeltaTime);
        rb2d.linearVelocity = Vector2.Lerp(
            rb2d.linearVelocity,
            directionNormalized * statsManager.moveSpeed,
            0.10f);
    }

    void RunFromTarget(Transform target) {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        //rb2d.MovePosition(rb2d.position + -directionNormalized * statsManager.moveSpeed * Time.fixedDeltaTime);
        rb2d.linearVelocity = Vector2.Lerp(
            rb2d.linearVelocity,
            -directionNormalized * statsManager.moveSpeed,
            0.10f);
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
                statsManager.baseAppliedKnockback = 50;
                rb2d.mass += 10;
                statsManager.strength += 5;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision != null && collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<HealthManager>().CalculateIncomingDamage(statsManager.baseDamage);
            collision.gameObject.GetComponent<CustomTime>().ScheduleKnockback(statsManager.strength, transform);
        }
    }

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(this.transform.position, statsManager.baseRange);
    //}
}
