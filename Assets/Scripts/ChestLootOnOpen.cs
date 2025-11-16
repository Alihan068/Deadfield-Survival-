using System.Collections.Generic;
using UnityEngine;

public class ChestLootOnOpen : MonoBehaviour {
    [Header("Loot Source")]
    public LootTableSO lootTable;

    [Header("Local Loot Table (used if LootTable is null)")]
    [Range(0f, 1f)]
    public float frequency = 1f;                // Base chance that the chest drops anything at all.
    public List<WeightedObject> weightedObjectList = new List<WeightedObject>();

    [Header("Drop Amount")]
    public int minDrops = 1;
    public int maxDrops = 1;

    [Header("Drop Positioning")]
    public float dropRadius = 0.5f;
    public bool useRandomSpread = true;

    [Header("Physics")]
    public bool usePhysicsImpulse = true;
    public float impulseForce = 3f;

    [Header("Weight Scaling (Rarity / Balancing)")]
    public bool useRarityWeightMultipliers = false;
    public List<RarityWeightMultiplier> rarityWeightMultipliers = new List<RarityWeightMultiplier>();

    [Header("External Modifiers")]
    [Tooltip("Additive modifier to the loot frequency. 0.25 = +25%, -0.5 = -50%.")]
    public float externalDropRateModifier = 0f;

    [Header("Luck")]
    [Tooltip("If true, luck will increase the chance that the chest drops loot.")]
    public bool useLuckFrequencyModifier = true;

    [Tooltip("Current luck value. Set this from your StatsManager or another system.")]
    public float currentLuck = 0f;

    [Tooltip("How much each point of luck scales loot drop chance. 0.01 = +1% per luck.")]
    public float luckFrequencyFactor = 0.01f;

    private bool isOpened = false;

    /// <summary>
    /// Call this to open the chest (e.g., player presses E or animation event).
    /// </summary>
    public void OpenChest() {
        if (isOpened)
            return;

        isOpened = true;

        TryDropLoot();

        // Optionally destroy chest or change state here:
        // Destroy(gameObject);
        // or play open animation, disable collider, etc.
    }

    private void TryDropLoot() {
        List<WeightedObject> activeList = GetActiveWeightedList();
        if (activeList == null || activeList.Count == 0)
            return;

        float effectiveFrequency = GetEffectiveFrequency();
        if (effectiveFrequency <= 0f)
            return;

        // Roll once: does this chest drop anything?
        if (Random.value > effectiveFrequency)
            return;

        if (maxDrops < minDrops)
            maxDrops = minDrops;

        int dropsToSpawn = Mathf.Clamp(Random.Range(minDrops, maxDrops + 1), 0, 1000);
        if (dropsToSpawn <= 0)
            return;

        for (int i = 0; i < dropsToSpawn; i++) {
            float totalWeight = CalculateTotalWeight(activeList);
            if (totalWeight <= 0f)
                return;

            if (!TryGetRandomWeightedObject(activeList, totalWeight, out WeightedObject selectedEntry))
                return;

            GameObject prefab = selectedEntry.GetRandomPrefab();
            if (prefab == null)
                continue;

            Vector3 spawnPos = GetSpawnPosition();
            Quaternion spawnRot = Quaternion.identity;

            GameObject instance = SpawnLootObject(prefab, spawnPos, spawnRot);
            if (usePhysicsImpulse) {
                ApplyImpulse(instance);
            }
        }
    }

    private List<WeightedObject> GetActiveWeightedList() {
        if (lootTable != null && lootTable.entries != null && lootTable.entries.Count > 0)
            return lootTable.entries;

        return weightedObjectList;
    }

    private float GetEffectiveFrequency() {
        float baseFreq = lootTable != null ? lootTable.frequency : frequency;
        baseFreq = Mathf.Clamp01(baseFreq);

        float modified = baseFreq * (1f + externalDropRateModifier);

        if (useLuckFrequencyModifier) {
            // Example: modified *= 1 + (luck * 0.01)
            modified *= 1f + currentLuck * luckFrequencyFactor;
        }

        return Mathf.Clamp01(modified);
    }

    private float CalculateTotalWeight(List<WeightedObject> list) {
        float sum = 0f;

        for (int i = 0; i < list.Count; i++) {
            WeightedObject entry = list[i];

            if (entry.collectibleItem == null || entry.collectibleItem.Length == 0)
                continue;

            float w = Mathf.Max(0f, entry.weight);

            if (useRarityWeightMultipliers) {
                w *= GetRarityMultiplier(entry.Rarity);
            }

            sum += w;
        }

        return sum;
    }

    private bool TryGetRandomWeightedObject(List<WeightedObject> list, float totalWeight, out WeightedObject selected) {
        float pick = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < list.Count; i++) {
            WeightedObject entry = list[i];

            if (entry.collectibleItem == null || entry.collectibleItem.Length == 0)
                continue;

            float w = Mathf.Max(0f, entry.weight);
            if (useRarityWeightMultipliers) {
                w *= GetRarityMultiplier(entry.Rarity);
            }

            if (w <= 0f)
                continue;

            cumulative += w;
            if (pick <= cumulative) {
                selected = entry;
                return true;
            }
        }

        selected = default;
        return false;
    }

    private float GetRarityMultiplier(ItemRarity rarity) {
        if (!useRarityWeightMultipliers || rarityWeightMultipliers == null || rarityWeightMultipliers.Count == 0)
            return 1f;

        for (int i = 0; i < rarityWeightMultipliers.Count; i++) {
            if (rarityWeightMultipliers[i].rarity == rarity) {
                float m = rarityWeightMultipliers[i].multiplier;
                return m < 0f ? 0f : m;
            }
        }

        return 1f;
    }

    private Vector3 GetSpawnPosition() {
        if (!useRandomSpread || dropRadius <= 0f)
            return transform.position;

        Vector2 offset = Random.insideUnitCircle * dropRadius;
        return transform.position + (Vector3)offset;
    }

    private GameObject SpawnLootObject(GameObject prefab, Vector3 position, Quaternion rotation) {
        if (prefab == null)
            return null;

        return Instantiate(prefab, position, rotation);
    }

    private void ApplyImpulse(GameObject instance) {
        if (instance == null)
            return;

        Rigidbody2D rb2D = instance.GetComponent<Rigidbody2D>();
        if (rb2D == null)
            return;

        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero)
            dir = Vector2.up;

        rb2D.AddForce(dir * impulseForce, ForceMode2D.Impulse);
    }

    private void OnValidate() {
        frequency = Mathf.Clamp01(frequency);

        if (minDrops < 0)
            minDrops = 0;

        if (maxDrops < minDrops)
            maxDrops = minDrops;

        if (weightedObjectList != null) {
            for (int i = 0; i < weightedObjectList.Count; i++) {
                WeightedObject w = weightedObjectList[i];
                if (w.weight < 0f) {
                    w.weight = 0f;
                    weightedObjectList[i] = w;
                }
            }
        }

        if (externalDropRateModifier < -0.99f)
            externalDropRateModifier = -0.99f;

        if (luckFrequencyFactor < 0f)
            luckFrequencyFactor = 0f;
    }

    // Optional quick test: player presses E while in trigger to open chest.
    private void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E)) {
            OpenChest();
        }
    }
}
