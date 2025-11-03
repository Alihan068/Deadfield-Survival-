using System.Collections;
using UnityEngine;

public enum EnemyType {
    Melee,
    Ranged,
    Charger,
}
public class EnemyController : MonoBehaviour {

    //[SerializeField] CollectibleItemSO baseStats;
    [SerializeField] EnemyType enemyType;

    [SerializeField] float attackZoneValue;
    float minRange;
    [SerializeField] float chargeSpeed = 2f;
    [SerializeField] float chargeCooldown = 2f;
    [SerializeField] float chargeTime = 2f;

    StatsManager statsManager;
    Rigidbody2D rb2d;
    Animator animator;
    Coroutine chargeCoroutine;
    PlayerController playerController;
    HealthManager healthManager;
    RangedParticleAttack rangedParticleAttack;

    Vector3 rotationToPlayer;
    float distanceToPlayer;
    void Awake() {
        statsManager = GetComponent<StatsManager>();
    }
    void OnEnable() {
        healthManager = GetComponent<HealthManager>();
        statsManager = GetComponent<StatsManager>();
        rangedParticleAttack = GetComponentInChildren<RangedParticleAttack>();
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerController = FindAnyObjectByType<PlayerController>();
        EnemyBaseStatImplementation(enemyType);
    }

    void Update() {
        if(!statsManager.canMove) return;

        CalculateDistanceToPlayer();
        EnemyMoveBehavior(enemyType);

    }
    void CalculateDistanceToPlayer() {
        rotationToPlayer = (transform.position - playerController.transform.position).normalized;
        distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
    }
    void FlipEnemyFacing() {

        transform.localScale = new Vector2(Mathf.Sign(-rb2d.linearVelocity.x), 1f);
    }


    void EnemyMoveBehavior(EnemyType enemyType) {
        Debug.Log("Move");
        FlipEnemyFacing();
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
        
    }
    void MeleeEnemyMovement() {
        if (distanceToPlayer <= statsManager.baseRange) {
            Debug.Log(this.name + "attack");
        }
        else if (distanceToPlayer > statsManager.baseRange) {
            Vector2 toTarget = (Vector2)playerController.gameObject.transform.position - rb2d.position;
            Vector2 directionNormalized = toTarget.normalized;
            rb2d.MovePosition(rb2d.position + directionNormalized * statsManager.moveSpeed * Time.fixedDeltaTime);  
            return;
        }
        rb2d.linearVelocity = Vector2.zero;
    }

    void RangedEnemyMovement() {
        if (distanceToPlayer <= minRange) {
            rangedParticleAttack.ParticleSystemToggle(false);
            RunFromTarget(playerController.transform);
            return;
            
        } else if ( distanceToPlayer >= statsManager.baseRange) {
            rangedParticleAttack.ParticleSystemToggle(false);
            ChaseTarget(playerController.transform);
            return;
        } else {
            rangedParticleAttack.ParticleSystemToggle(true);
            Vector2 toTarget = (Vector2)playerController.transform.position - rb2d.position;
            Vector2 directionNormalized = toTarget.normalized;
            transform.localScale = new Vector2(Mathf.Sign(directionNormalized.x),1f);
            //Debug.Log("RangedAttack!");

        }

        rb2d.linearVelocity = Vector2.zero;
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
        yield return new WaitForSeconds(1);
        healthManager.isUnstoppable = true;
        Debug.Log("Charge!");
        rb2d.linearVelocity = (direction.normalized * statsManager.moveSpeed * chargeSpeed);
        yield return new WaitForSeconds(chargeTime);
        healthManager.isUnstoppable = false;
        rb2d.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(chargeCooldown);
        chargeCoroutine = null;
    }

    void ChaseTarget(Transform target) {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        //rb2d.MovePosition(rb2d.position + directionNormalized * statsManager.moveSpeed * Time.fixedDeltaTime);
        rb2d.linearVelocity = directionNormalized * statsManager.moveSpeed;
    }

    void RunFromTarget(Transform target) {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        //rb2d.MovePosition(rb2d.position + -directionNormalized * statsManager.moveSpeed * Time.fixedDeltaTime);
        rb2d.linearVelocity = -directionNormalized * statsManager.moveSpeed;
    }

    void EnemyBaseStatImplementation(EnemyType enemyType) {
        switch (enemyType) {
            case EnemyType.Melee:

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
