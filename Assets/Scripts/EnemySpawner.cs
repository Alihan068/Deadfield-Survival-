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
    [SerializeField] Collider2D mapBounds;          // Haritayı kapsayan Box/Composite collider (isTrigger olabilir)
    [SerializeField] LayerMask wallMask;            // Katı duvar layer’ı
    [SerializeField] float wallCheckRadius = 1f;    // Enemy boyuna göre ayarla
    [SerializeField] int maxSpawnTries = 15;        // Sonsuz döngüye girmesin

    [Header("UI")]
    [SerializeField] TextMeshProUGUI enemyCountText;

    Transform target;
    public float enemyCount = 0f;
    public bool pauseEnemySpawn = false;

    void Start() {
        target = FindFirstObjectByType<PlayerController>().transform;
        StartCoroutine(SpawnEnemyAtRandomPos());
        if (enemyCountText != null)
            enemyCountText.text = ": " + enemyCount;
    }

    void FixedUpdate() {
        if (enemyCountText != null)
            enemyCountText.text = ": " + enemyCount;
    }

    IEnumerator SpawnEnemyAtRandomPos() {
        while (!pauseEnemySpawn && enemyCount < maxEnemyCount) {

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

            yield return new WaitForSeconds(spawnDelay);
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
        if (mapBounds == null) return true;  // atanmadıysa kontrol etme
        return mapBounds.OverlapPoint(pos);
    }

    bool IsInsideWall(Vector2 pos) {
        return Physics2D.OverlapCircle(pos, wallCheckRadius, wallMask) != null;
    }
}
