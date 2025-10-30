using UnityEngine;

public enum EnemyType {
    Melee,
    Ranged,
    Dasher,
}
public class EnemyController : MonoBehaviour {

    //[SerializeField] CollectibleItemSO baseStats;
    [SerializeField] EnemyType enemyType;

    [SerializeField] float kiteRange;
    float minRange;

    StatsManager statsManager;
    Rigidbody2D rb2d;
    Animator animator;
    Coroutine coroutine;
    PlayerController playerController;

    public bool canMove = true;

    Vector3 rotationToPlayer;
    float distanceToPlayer;
    void Awake() {
        statsManager = GetComponent<StatsManager>();
    }
    void OnEnable() {
        statsManager = GetComponent<StatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerController = FindAnyObjectByType<PlayerController>();
        EnemyBaseStatImplementation(enemyType);
    }

    private void Update() {
        if (!canMove) return;
        CalculateDistanceToPlayer();
    }
    void FixedUpdate() {
        if (!canMove) return;
        EnemyMoveBehavior(enemyType);
    }
    void CalculateDistanceToPlayer() {
        rotationToPlayer = (transform.position - playerController.transform.position).normalized;
        distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
    }

    void EnemyMoveBehavior(EnemyType enemyType) {
        
        switch (enemyType) {
            case EnemyType.Melee:
                MeleeEnemyMovement();
                break;
            case EnemyType.Ranged:
                RangedEnemyMovement();
                break;
            case EnemyType.Dasher:
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
        }
    }

    void RangedEnemyMovement() {
        if (distanceToPlayer <= minRange) {
            RunFromTarget(playerController.transform);
        } else if ( distanceToPlayer >= statsManager.baseRange) {
            ChaseTarget(playerController.transform);
        } else {     
            Debug.Log("RangedAttack!");
        }
    }

    void ChaseTarget(Transform target) {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.MovePosition(rb2d.position + directionNormalized * statsManager.moveSpeed * Time.fixedDeltaTime);
    }

    void RunFromTarget(Transform target) {
        Vector2 toTarget = (Vector2)target.position - rb2d.position;
        Vector2 directionNormalized = toTarget.normalized;
        rb2d.MovePosition(rb2d.position - directionNormalized * statsManager.moveSpeed * Time.fixedDeltaTime);
    }

    void EnemyBaseStatImplementation(EnemyType enemyType) {
        switch (enemyType) {
            case EnemyType.Melee:

                break;
            case EnemyType.Ranged:
                minRange = statsManager.baseRange - kiteRange;
                break;
            case EnemyType.Dasher:
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision != null && collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponentInChildren<HealthManager>().CalculateIncomingDamage(statsManager.baseDamage);
        }
    }

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(this.transform.position, statsManager.baseRange);
    //}
}
