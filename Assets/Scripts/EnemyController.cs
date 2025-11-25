using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    MeleeChaser,
    MeleeWeapon,
    Ranged,
    Charger,
}

public class EnemyController : MonoBehaviour
{

    //[SerializeField] CollectibleItemSO baseStats;
    [SerializeField] EnemyType enemyType;

    [SerializeField] float rangeBuffer;

    float minRange;
    [SerializeField] float chargeSpeed = 2f;
    [SerializeField] float chargeCooldown = 2f;
    [SerializeField] float chargeTime = 2f;

    [SerializeField] bool canDamageOnTouch = false;

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
    EnemySpawner enemySpawner;

    AudioSource audioSource;
    AudioClip[] baseSounds;

    string attackAnimName = "ChargeAttack";
    int attackAnimHash;

    bool chargeCoroutineRunning;

    Vector3 rotationToPlayer;
    float distanceToPlayer;

    void Awake()
    {
        statsManager = GetComponent<StatsManager>();
    }

    void OnEnable()
    {
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

        enemySpawner = FindFirstObjectByType<EnemySpawner>();

        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (!statsManager.canMove || statsManager.isKnocked || customTime.timeScale <= 0) return;

        CalculateDistanceToPlayer();
        EnemyMoveBehavior(enemyType);
    }

    void CalculateDistanceToPlayer()
    {
        rotationToPlayer = (transform.position - playerController.transform.position).normalized;
        distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
    }

    void FlipEnemyFacing()
    {
        if (playerController == null || rb2d == null || characterSprite == null) return;

        if (rb2d.linearVelocity.sqrMagnitude <= Mathf.Epsilon)
        {
            float dx = playerController.transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) > Mathf.Epsilon)
            {
                characterSprite.flipX = dx < 0f;
            }
        }

        float vx = rb2d.linearVelocity.x;
        if (vx > Mathf.Epsilon)
        {
            characterSprite.flipX = false;
        }
        else if (vx < -Mathf.Epsilon)
        {
            characterSprite.flipX = true;
        }
    }

    void EnemyMoveBehavior(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.MeleeChaser:
            case EnemyType.MeleeWeapon:
                MeleeEnemyMovement();
                FlipEnemyFacing();
                break;
            case EnemyType.Ranged:
                RangedEnemyMovement();
                break;
            case EnemyType.Charger:
                ChargerMovement();
                FlipEnemyFacing();
                break;
        }
    }

    void ChaserMovement()
    {
        Vector2 toTarget = (Vector2)playerController.gameObject.transform.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.linearVelocity = directionNormalized * statsManager.EffectiveMoveSpeed;
    }

    void MeleeEnemyMovement()
    {
        // Use effective range instead of raw baseRange
        float effectiveRange = statsManager.EffectiveRange;

        if (distanceToPlayer <= effectiveRange)
        {
            rb2d.linearVelocity = Vector2.zero;

            // Use effective attack speed for animation speed
            float attackSpeed = statsManager.EffectiveAttackSpeed;
            weaponAnimator.SetFloat("attackSpeed", attackSpeed);

            weaponAnimator.SetBool("isAttacking", true);
        }
        else if (distanceToPlayer > effectiveRange)
        {
            weaponAnimator.SetBool("isAttacking", false);
            Vector2 toTarget = (Vector2)playerController.gameObject.transform.position - rb2d.position;
            Vector2 directionNormalized = toTarget.normalized;
            rb2d.linearVelocity = directionNormalized * statsManager.EffectiveMoveSpeed;
        }
    }

    void RangedEnemyMovement()
    {
        float effectiveRange = statsManager.EffectiveRange;

        if (distanceToPlayer <= minRange - rangeBuffer)
        {
            weaponScript.SetFiring(false);
            rangedParticleAttack.ParticleSystemToggle(false);
            RunFromTarget(playerController.transform);
            FlipEnemyFacing();
            return;
        }
        else if (distanceToPlayer >= effectiveRange + rangeBuffer)
        {
            weaponScript.SetFiring(false);
            rangedParticleAttack.ParticleSystemToggle(false);
            ChaseTarget(playerController.transform);
            FlipEnemyFacing();
            return;
        }
        else
        {
            weaponScript.SetFiring(true);
            rangedParticleAttack.ParticleSystemToggle(true);
            rb2d.linearVelocity = Vector2.zero;
        }
    }

    void AnimationHandler()
    {
        if (rb2d.linearVelocity != Vector2.zero)
        {
            bodyAnimator.SetBool("isWalking", true);
        }
        else
        {
            bodyAnimator.SetBool("isWalking", false);
        }
    }

    void ChargerMovement()
    {
        float effectiveRange = statsManager.EffectiveRange;

        if (distanceToPlayer <= effectiveRange)
        {
            if (chargeCoroutine == null)
            {
                chargeCoroutine = StartCoroutine(ChargeTarget(playerController.transform));
            }
            return;
        }
        else if (distanceToPlayer > effectiveRange)
        {
            chargeCoroutine = null;
            ChaseTarget(playerController.transform);
            return;
        }
    }

    IEnumerator ChargeTarget(Transform target)
    {
        chargeCoroutineRunning = true;
        rb2d.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(1);
        Vector2 direction = ((Vector2)target.position - rb2d.position).normalized;
        yield return null;

        rb2d.linearVelocity = (direction.normalized * statsManager.EffectiveMoveSpeed * chargeSpeed);
        yield return new WaitForSeconds(chargeTime);
        rb2d.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(chargeCooldown);
        chargeCoroutineRunning = true;
        chargeCoroutine = null;
    }

    public void BreakCharge()
    {
        if (enemyType == EnemyType.Charger && chargeCoroutineRunning && chargeCoroutine != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            StopCoroutine(chargeCoroutine);
            chargeCoroutineRunning = true;
            chargeCoroutine = null;
            statsManager.isUnstoppable = false;
        }
    }

    void ChaseTarget(Transform target)
    {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.linearVelocity = directionNormalized * statsManager.EffectiveMoveSpeed;
    }

    void RunFromTarget(Transform target)
    {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.linearVelocity = -directionNormalized * statsManager.EffectiveMoveSpeed;
    }

    void EnemyBaseStatImplementation(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.MeleeWeapon:
                statsManager.knockBack += 5;
                break;
            case EnemyType.Ranged:
                // Use effective range at initialization
                minRange = statsManager.EffectiveRange;
                break;
            case EnemyType.Charger:
                statsManager.baseAppliedKnockback = 50;
                rb2d.mass += 10;
                statsManager.knockBack += 5;
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.gameObject.CompareTag("Player") && canDamageOnTouch)
        {
            var targetHealth = collision.gameObject.GetComponent<HealthManager>();
            if (targetHealth != null)
            {
                // Use effective damage instead of raw baseDamage
                targetHealth.CalculateIncomingDamage(statsManager.EffectiveDamage);
                targetHealth.ApplyKnockback(statsManager.knockBack, transform);
            }
        }
    }
}
