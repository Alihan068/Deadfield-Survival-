using System.Collections;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] GameObject[] EnemyPrefabs;

    [SerializeField] float minSpawnDistance = 60f;
    [SerializeField] float maxSpawnDistance = 100f;
    [SerializeField] float spawnDelay = 1f;
    [SerializeField] int spawnCount = 10;

    [SerializeField] TextMeshProUGUI enemyCountText;

    Transform target;
    [SerializeField] int maxEnemyCount = 50;
    public float enemyCount = 0f;
    public bool pauseEnemySpawn = false;
    void Start() {
        target = FindFirstObjectByType<PlayerController>().transform;
        StartCoroutine(SpawnEnemyAtRandomPos(spawnCount));

        Vector2 OuterCircle = new Vector2(maxSpawnDistance + target.position.x, maxSpawnDistance + target.position.y);
        Vector2 innerCircle = new Vector2(minSpawnDistance + target.position.x, minSpawnDistance + target.position.y);

        transform.position = Random.insideUnitCircle * minSpawnDistance;
        enemyCountText.text = (": " + enemyCount);
    }

    // Update is called once per frame
    void FixedUpdate() {
        enemyCountText.text = (": " + enemyCount);
    }

    IEnumerator SpawnEnemyAtRandomPos(int spawnAmount) {

        //for (int i = 0; i < spawnAmount; i++)
        while(!pauseEnemySpawn && enemyCount < maxEnemyCount) {
            Vector2 pos = target.position;

            Vector2 randomDriection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector2 randomPos = new Vector2(pos.x + randomDriection.x * randomDistance,
                 pos.y + randomDriection.y * randomDistance);

            Instantiate(EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)], randomPos, Quaternion.identity);
            //Debug.Log(randomPos);
            yield return new WaitForSeconds(spawnDelay);
        }     
    }

    Vector2 RandomOutsideUnitCircle(float minRadius, Vector2 directionParent) {
        // minRadius: dairenin dışına ne kadar mesafeden başlasın
        Vector2 dir = directionParent.normalized;
        float dist = Random.Range(minRadius, minRadius + 1f);
        return dir * dist;
    }

}