using UnityEngine;

public enum EnemyType {

    Regular,
    Ranged,
    Dasher,
}
public class EnemyController : MonoBehaviour {

    [SerializeField] EnemySO enemySO;

    StatsManager statsManager;
    Rigidbody2D rb2d;
    Animator animator;
    Coroutine coroutine;
    PlayerController playerController;

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
    }

    private void Update() {
        CalculateDistanceToPlayer();
    }
    void FixedUpdate() {
        EnemyMoveBehavior(enemySO.enemyType);
    }
    void CalculateDistanceToPlayer() {
        rotationToPlayer = (transform.position - playerController.transform.position).normalized;
        distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
    }

    void EnemyMoveBehavior(EnemyType enemyType) {
        switch (enemyType) {
            case EnemyType.Regular:
                MeleeEnemyMovement();
                break;
            case EnemyType.Ranged:
                RangedEnemyMovement();
                break;
            case EnemyType.Dasher:
                break;

        }

    }

    void ApplyStatsBasedOnType(EnemyType enemyType) {
        switch (enemyType) {
            case EnemyType.Regular:

                break;
            case EnemyType.Ranged:

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
        if (distanceToPlayer <= statsManager.baseRange) {
            Vector2 toTarget = (Vector2)playerController.gameObject.transform.position - rb2d.position;
            Vector2 directionNormalized = toTarget.normalized;
            rb2d.MovePosition(rb2d.position - directionNormalized * statsManager.moveSpeed * Time.fixedDeltaTime);
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
