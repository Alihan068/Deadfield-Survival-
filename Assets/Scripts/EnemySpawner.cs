using System.Collections;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] GameObject[] EnemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] float minSpawnDistance = 60f;
    [SerializeField] float maxSpawnDistance = 100f;
    [SerializeField] float spawnDelay = 1f;
    [SerializeField] int maxEnemyCount = 50;

    [Header("Map & Collision")]
    [SerializeField] Collider2D mapBounds;
    [SerializeField] LayerMask wallMask;
    [SerializeField] float wallCheckRadius = 1f;
    [SerializeField] int maxSpawnTries = 15;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI enemyCountText;

    Transform target;
    public int enemyCount = 0;
    public bool pauseEnemySpawn = false;

    Coroutine spawnCoroutine;

    void Start() {
        target = FindFirstObjectByType<PlayerController>().transform;

        if (spawnCoroutine == null) {
            spawnCoroutine = StartCoroutine(SpawnEnemyAtRandomPos());
        }

        if (enemyCountText != null)
            enemyCountText.text = ": " + enemyCount;
    }

    void FixedUpdate() {
        if (enemyCountText != null)
            enemyCountText.text = ": " + enemyCount;
    }

    IEnumerator SpawnEnemyAtRandomPos() {
        var wait = new WaitForSeconds(spawnDelay);

        while (true) {
            if (pauseEnemySpawn || enemyCount >= maxEnemyCount || EnemyPrefabs == null || EnemyPrefabs.Length == 0) {
                yield return wait;
                continue;
            }

            Vector2 spawnPos;
            bool found = TryGetValidSpawnPosition(out spawnPos);

            if (found) {
                Instantiate(
                    EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)],
                    spawnPos,
                    Quaternion.identity
                );
                enemyCount++;
            }

            yield return wait;
        }
    }

    bool TryGetValidSpawnPosition(out Vector2 result) {
        result = Vector2.zero;

        for (int i = 0; i < maxSpawnTries; i++) {
            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector2 candidate = (Vector2)target.position + dir * dist;

            if (!IsInsideMap(candidate)) continue;
            if (IsInsideWall(candidate)) continue;

            result = candidate;
            return true;
        }

        return false;
    }

    bool IsInsideMap(Vector2 pos) {
        if (mapBounds == null) return true;
        return mapBounds.OverlapPoint(pos);
    }

    bool IsInsideWall(Vector2 pos) {
        return Physics2D.OverlapCircle(pos, wallCheckRadius, wallMask) != null;
    }
}
