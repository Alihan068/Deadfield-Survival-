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

    [Header("Config")]
    [SerializeField] EnemySO enemyData;
    [SerializeField] EnemyType enemyType;
    [SerializeField] float rangeBuffer = 1f;
    [SerializeField] float detectionRange = 10f;
    [SerializeField] bool canDamageOnTouch = false;

    float minRange;

    [Header("Charge Settings")]
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
    EnemySpawner enemySpawner;

    AudioSource audioSource;
    AudioClip[] baseSounds;

    string attackAnimName = "ChargeAttack";
    int attackAnimHash;

    bool chargeCoroutineRunning;

    Vector3 rotationToPlayer;
    float distanceToPlayer;

    IEnemyState currentState;
    EnemyStateId currentStateId = EnemyStateId.Idle;

    public float DistanceToPlayer => distanceToPlayer;
    public float DetectionRange => detectionRange;
    public bool IsDead => statsManager != null && statsManager.currentHealth <= 0f;

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
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        audioSource = GetComponent<AudioSource>();

        InitializeFromEnemyData();
        EnemyBaseStatImplementation(enemyType);

        attackAnimHash = Animator.StringToHash(attackAnimName);

        ChangeState(new EnemyIdleState(), EnemyStateId.Idle);
    }

    void FixedUpdate()
    {
        if (statsManager == null || customTime == null) return;
        if (!statsManager.canMove || statsManager.isKnocked || customTime.timeScale <= 0f) return;

        CalculateDistanceToPlayer();

        if (currentState != null)
        {
            currentState.OnUpdate(this);
        }

        AnimationHandler();
    }

    void InitializeFromEnemyData()
    {
        if (enemyData == null || statsManager == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(enemyData.enemyName))
        {
            gameObject.name = enemyData.enemyName;
        }

        enemyType = enemyData.enemyType;

        statsManager.baseMaxHealth = enemyData.baseMaxHealth;
        statsManager.maxHealth = enemyData.baseMaxHealth;
        statsManager.baseSpeed = enemyData.baseSpeed;
        statsManager.baseRange = enemyData.baseRange;
        statsManager.baseDamage = enemyData.baseDamage;
        statsManager.attackSpeed = enemyData.attackSpeed;
        statsManager.knockBack = enemyData.knockBack;
        statsManager.baseAppliedKnockback = enemyData.baseAppliedKnockback;
        statsManager.knockbackStagger = enemyData.knockbackStagger;

        canDamageOnTouch = enemyData.canDamageOnTouch;

        if (enemyData.bodyAnimatorController != null && bodyAnimator != null)
        {
            bodyAnimator.runtimeAnimatorController = enemyData.bodyAnimatorController;
        }

        if (enemyData.weaponAnimatorController != null && weaponAnimator != null)
        {
            weaponAnimator.runtimeAnimatorController = enemyData.weaponAnimatorController;
        }

        if (enemyData.baseSounds != null && enemyData.baseSounds.Length > 0)
        {
            baseSounds = enemyData.baseSounds;
        }

        if (statsManager.baseRange > 0f)
        {
            detectionRange = Mathf.Max(detectionRange, statsManager.baseRange * 2f);
        }
    }

    public void ChangeState(IEnemyState newState, EnemyStateId newStateId)
    {
        if (newState == null) return;

        if (currentState != null)
        {
            currentState.OnExit(this);
        }

        currentState = newState;
        currentStateId = newStateId;
        currentState.OnEnter(this);
    }

    void CalculateDistanceToPlayer()
    {
        if (playerController == null) return;

        rotationToPlayer = (transform.position - playerController.transform.position).normalized;
        distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
    }

    public void UpdateCombatMovementAndAttack()
    {
        EnemyMoveBehavior(enemyType);
    }

    void EnemyMoveBehavior(EnemyType type)
    {
        switch (type)
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
        if (playerController == null || rb2d == null) return;

        Vector2 toTarget = (Vector2)playerController.gameObject.transform.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.linearVelocity = directionNormalized * statsManager.baseSpeed;
    }

    void MeleeEnemyMovement()
    {
        if (playerController == null || rb2d == null || weaponAnimator == null) return;

        if (distanceToPlayer <= statsManager.baseRange)
        {
            rb2d.linearVelocity = Vector2.zero;

            float atkSpeed = Mathf.Max(0.01f, statsManager.attackSpeed);
            weaponAnimator.SetFloat("attackSpeed", atkSpeed);
            weaponAnimator.SetBool("isAttacking", true);
        }
        else
        {
            weaponAnimator.SetBool("isAttacking", false);
            Vector2 toTarget = (Vector2)playerController.gameObject.transform.position - rb2d.position;
            Vector2 directionNormalized = toTarget.normalized;
            rb2d.linearVelocity = directionNormalized * statsManager.baseSpeed;
        }
    }

    void RangedEnemyMovement()
    {
        if (playerController == null || rb2d == null || weaponScript == null || rangedParticleAttack == null) return;

        if (distanceToPlayer <= minRange - rangeBuffer)
        {
            weaponScript.SetFiring(false);
            rangedParticleAttack.ParticleSystemToggle(false);
            RunFromTarget(playerController.transform);
            FlipEnemyFacing();
            return;
        }
        else if (distanceToPlayer >= statsManager.baseRange + rangeBuffer)
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

    void ChargerMovement()
    {
        if (playerController == null) return;

        if (distanceToPlayer <= statsManager.baseRange)
        {
            if (chargeCoroutine == null)
            {
                chargeCoroutine = StartCoroutine(ChargeTarget(playerController.transform));
            }
            return;
        }
        else
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

        yield return new WaitForSeconds(1f);
        Vector2 direction = ((Vector2)target.position - rb2d.position).normalized;
        yield return null;

        rb2d.linearVelocity = direction.normalized * statsManager.baseSpeed * chargeSpeed;
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
        if (rb2d == null) return;

        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.linearVelocity = directionNormalized * statsManager.baseSpeed;
    }

    void RunFromTarget(Transform target)
    {
        if (rb2d == null) return;

        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.linearVelocity = -directionNormalized * statsManager.baseSpeed;
    }

    void EnemyBaseStatImplementation(EnemyType type)
    {
        if (statsManager == null) return;

        switch (type)
        {
            case EnemyType.MeleeWeapon:
                statsManager.knockBack += 5f;
                break;
            case EnemyType.Ranged:
                minRange = statsManager.baseRange;
                break;
            case EnemyType.Charger:
                statsManager.baseAppliedKnockback = 50f;
                if (rb2d != null)
                {
                    rb2d.mass += 10f;
                }
                statsManager.knockBack += 5f;
                break;
        }
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

    void AnimationHandler()
    {
        if (bodyAnimator == null || rb2d == null) return;

        bool isWalking = rb2d.linearVelocity != Vector2.zero;
        bodyAnimator.SetBool("isWalking", isWalking);
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (rb2d == null) return;
        rb2d.linearVelocity = velocity;
    }

    public void SetWalkingAnimation(bool isWalking)
    {
        if (bodyAnimator == null) return;
        bodyAnimator.SetBool("isWalking", isWalking);
    }

    public bool HasValidPlayerTarget()
    {
        return playerController != null;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag("Player") && canDamageOnTouch)
        {
            HealthManager targetHealth = collision.gameObject.GetComponent<HealthManager>();
            if (targetHealth != null)
            {
                targetHealth.CalculateIncomingDamage(statsManager.baseDamage);
                targetHealth.ApplyKnockback(statsManager.knockBack, transform);
            }
        }
    }
}
