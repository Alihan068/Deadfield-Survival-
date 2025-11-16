using UnityEngine;

public class SpawnChestOnDeath : MonoBehaviour {
    [Header("Chest Settings")]
    public GameObject chestPrefab;

    [Tooltip("Random offset radius for where the chest spawns around the enemy.")]
    public float chestSpawnRadius = 0.3f;

    [Tooltip("Use random offset when spawning the chest.")]
    public bool useRandomOffset = true;

    [Tooltip("If true and chest has Rigidbody2D, add an impulse to make it pop out.")]
    public bool usePhysicsImpulse = true;
    public float impulseForce = 2f;

    [Header("Chest Drop Chance")]
    [Range(0f, 1f)]
    [Tooltip("Base chance to spawn a chest when this enemy dies.")]
    public float baseChestDropChance = 0.3f;

    [Tooltip("If true, chest drop chance is modified by luck.")]
    public bool useLuckModifier = true;

    [Tooltip("How much each point of luck scales drop chance. 0.01 = +1% per luck.")]
    public float luckChanceFactor = 0.01f;

    [Tooltip("Current luck value. Set this from your StatsManager or another system.")]
    public float currentLuck = 0f;

    // Call this from your Health/Death logic when the enemy dies.
    public void IfDestroy() {
        if (!ShouldSpawnChest())
            return;

        SpawnChest();
    }

    private bool ShouldSpawnChest() {
        float chance = Mathf.Clamp01(baseChestDropChance);

        if (useLuckModifier) {
            // Example: chance *= 1 + (luck * 0.01)
            chance *= 1f + currentLuck * luckChanceFactor;
        }

        chance = Mathf.Clamp01(chance);

        return Random.value <= chance;
    }

    private void SpawnChest() {
        if (chestPrefab == null)
            return;

        Vector3 spawnPos = transform.position;

        if (useRandomOffset && chestSpawnRadius > 0f) {
            Vector2 offset = Random.insideUnitCircle * chestSpawnRadius;
            spawnPos += (Vector3)offset;
        }

        GameObject chestInstance = Instantiate(chestPrefab, spawnPos, Quaternion.identity);

        if (usePhysicsImpulse && chestInstance != null) {
            Rigidbody2D rb = chestInstance.GetComponent<Rigidbody2D>();
            if (rb != null) {
                Vector2 dir = Random.insideUnitCircle.normalized;
                if (dir == Vector2.zero)
                    dir = Vector2.up;

                rb.AddForce(dir * impulseForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnValidate() {
        baseChestDropChance = Mathf.Clamp01(baseChestDropChance);
        if (luckChanceFactor < 0f)
            luckChanceFactor = 0f;
    }
}
