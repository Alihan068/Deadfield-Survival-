using System.Collections.Generic;
using UnityEngine;

public class LootDropOnDeath : MonoBehaviour {
    [Header("Guaranteed Drops")]
    public List<GameObject> guaranteedPrefabs = new List<GameObject>();

    [Header("Loot Source")]
    public LootTableSO lootTable;

    [Header("Local Loot Table")]
    [Range(0f, 1f)]
    public float frequency = 1f;
    public List<WeightedObject> weightedObjectList = new List<WeightedObject>();

    [Header("Drop Amount (Fallback Min/Max)")]
    public int minDrops = 1;
    public int maxDrops = 1;

    [Header("Drop Count Weights")]
    public List<DropCountWeight> dropCountWeights = new List<DropCountWeight>();

    [Header("Drop Positioning")]
    public float dropRadius = 0.5f;
    public bool useRandomSpread = true;

    [Header("Physics")]
    public bool usePhysicsImpulse = true;
    public float impulseForce = 3f;

    [Header("Weight Scaling")]
    public bool useRarityWeightMultipliers = false;
    public List<RarityWeightMultiplier> rarityWeightMultipliers = new List<RarityWeightMultiplier>();

    [Header("External Modifiers")]
    public float externalDropRateModifier = 0f;

    [Header("Luck")]
    public bool useLuckFrequencyModifier = true;
    public float luckFrequencyFactor = 0.01f;

     DifficulityManager difficultyManager;

    void Awake() {
        difficultyManager = FindFirstObjectByType<DifficulityManager>();
    }

    public void IfDestroy() {
        SpawnGuaranteedDrops();
        TryDropLoot();
    }

     void SpawnGuaranteedDrops() {
        if (guaranteedPrefabs == null || guaranteedPrefabs.Count == 0)
            return;

        foreach (var prefab in guaranteedPrefabs) {
            if (prefab == null)
                continue;

            Vector3 spawnPos = GetSpawnPosition();
            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);

            if (usePhysicsImpulse) {
                ApplyImpulse(instance);
            }
        }
    }

     void TryDropLoot() {
        var activeList = GetActiveWeightedList();
        if (activeList == null || activeList.Count == 0)
            return;

        float effectiveFrequency = GetEffectiveFrequency();
        if (Random.value > effectiveFrequency)
            return;

        int dropsToSpawn = GetDropCount();
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
            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);

            if (usePhysicsImpulse) {
                ApplyImpulse(instance);
            }
        }
    }

     List<WeightedObject> GetActiveWeightedList() {
        if (lootTable != null && lootTable.entries != null && lootTable.entries.Count > 0)
            return lootTable.entries;

        return weightedObjectList;
    }

     float GetEffectiveFrequency() {
        float baseFreq = lootTable != null ? lootTable.frequency : frequency;
        baseFreq = Mathf.Clamp01(baseFreq);

        float modified = baseFreq * (1f + externalDropRateModifier);

        if (useLuckFrequencyModifier && difficultyManager != null) {
            float luck = Mathf.Max(0f, difficultyManager.playerLuck);
            float luckBonus = luck * luckFrequencyFactor;   // örn: luck=1, factor=0.2 → +20%

            modified *= (1f + luckBonus);
        }

        return Mathf.Clamp01(modified);
    }


     int GetDropCount() {
        if (dropCountWeights == null || dropCountWeights.Count == 0) {
            if (maxDrops < minDrops)
                maxDrops = minDrops;

            return Random.Range(minDrops, maxDrops + 1);
        }

        float total = 0f;
        foreach (var entry in dropCountWeights) {
            if (entry.weight > 0f)
                total += entry.weight;
        }
        if (total <= 0f)
            return 0;

        float pick = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var entry in dropCountWeights) {
            if (entry.weight <= 0f)
                continue;

            cumulative += entry.weight;
            if (pick <= cumulative)
                return entry.count;
        }

        return dropCountWeights[dropCountWeights.Count - 1].count;
    }

     float CalculateTotalWeight(List<WeightedObject> list) {
        float sum = 0f;

        foreach (var entry in list) {
            if (entry.collectibleItem == null || entry.collectibleItem.Length == 0)
                continue;

            float w = Mathf.Max(0f, entry.weight);
            if (useRarityWeightMultipliers)
                w *= GetRarityMultiplier(entry.Rarity);

            sum += w;
        }

        return sum;
    }

     bool TryGetRandomWeightedObject(List<WeightedObject> list, float totalWeight, out WeightedObject selected) {
        float pick = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in list) {
            if (entry.collectibleItem == null || entry.collectibleItem.Length == 0)
                continue;

            float w = Mathf.Max(0f, entry.weight);
            if (useRarityWeightMultipliers)
                w *= GetRarityMultiplier(entry.Rarity);

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

     float GetRarityMultiplier(ItemRarity rarity) {
        if (!useRarityWeightMultipliers || rarityWeightMultipliers == null)
            return 1f;

        foreach (var r in rarityWeightMultipliers) {
            if (r.rarity == rarity)
                return Mathf.Max(0f, r.multiplier);
        }

        return 1f;
    }

     Vector3 GetSpawnPosition() {
        if (!useRandomSpread || dropRadius <= 0f)
            return transform.position;

        Vector2 offset = Random.insideUnitCircle * dropRadius;
        return transform.position + (Vector3)offset;
    }

     void ApplyImpulse(GameObject instance) {
        if (instance == null)
            return;

        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero)
            dir = Vector2.up;

        rb.AddForce(dir * impulseForce, ForceMode2D.Impulse);
    }

     void OnValidate() {
        frequency = Mathf.Clamp01(frequency);

        if (minDrops < 0)
            minDrops = 0;

        if (maxDrops < minDrops)
            maxDrops = minDrops;

        if (externalDropRateModifier < -0.99f)
            externalDropRateModifier = -0.99f;

        if (luckFrequencyFactor < 0f)
            luckFrequencyFactor = 0f;
    }
}
