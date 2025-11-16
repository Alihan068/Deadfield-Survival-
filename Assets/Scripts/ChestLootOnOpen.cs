using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DropCountWeight {
    public int count;     // how many items to drop
    public float weight;  // probability weight
}

public class ChestLootOnOpen : MonoBehaviour {
    [Header("Loot Source")]
    public LootTableSO lootTable;

    [Header("Local Loot Table (used if LootTable is null)")]
    [Range(0f, 1f)]
    public float frequency = 1f;                // Base chance that the chest drops anything at all.
    public List<WeightedObject> weightedObjectList = new List<WeightedObject>();

    [Header("Drop Amount (Fallback Min/Max)")]
    [Tooltip("Used when DropCountWeights is empty. Random.Range(minDrops, maxDrops+1).")]
    public int minDrops = 1;
    public int maxDrops = 1;

    [Header("Drop Count Weights (Overrides Min/Max if not empty)")]
    [Tooltip("If this list has entries with weight > 0, it overrides min/max logic.")]
    public List<DropCountWeight> dropCountWeights = new List<DropCountWeight>();

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

    [Tooltip("How much each point of luck scales loot drop chance. 0.01 = +1% per luck.")]
    public float luckFrequencyFactor = 0.01f;

    [Header("Chest Visuals / State")]
    [Tooltip("SpriteRenderer used to display the chest. If null, will try GetComponent<SpriteRenderer>().")]
    public SpriteRenderer chestSpriteRenderer;

    [Tooltip("Sprite while chest is closed.")]
    public Sprite closedSprite;

    [Tooltip("Sprite after chest is opened.")]
    public Sprite openedSprite;

    [Tooltip("Collider used for interaction (e.g. trigger). Will be disabled after opening.")]
    public Collider2D interactionCollider;

    [Tooltip("Destroy the chest GameObject after opening?")]
    public bool destroyAfterOpen = false;

    [Tooltip("Delay before destroying chest after opening.")]
    public float destroyDelay = 0.5f;

    private bool isOpened = false;

    DifficulityManager difficulityManager;
    private void Awake() {

        difficulityManager = FindFirstObjectByType<DifficulityManager>();
        
        if (chestSpriteRenderer == null) {
            chestSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (interactionCollider == null) {
            interactionCollider = GetComponent<Collider2D>();
        }

        if (chestSpriteRenderer != null && closedSprite != null) {
            chestSpriteRenderer.sprite = closedSprite;
        }
    }

    /// <summary>
    /// Call this to open the chest (e.g., player presses E or animation event).
    /// </summary>
    public void OpenChest() {
        if (isOpened)
            return;

        isOpened = true;

        // 1) Loot logic
        TryDropLoot();

        // 2) Visual / state / FX logic
        UpdateVisualOnOpen();
        ApplyOpeningEffects();
        HandlePostOpenLifecycle();
    }

    /// <summary>
    /// Placeholder for opening effects (VFX, SFX, camera shake, etc.).
    /// Not implemented yet; use this in the future.
    /// </summary>
    public void ApplyOpeningEffects() {
        // Intentionally left empty for future use.
        // Example: play sound, spawn particle, camera shake, etc.
    }

    private void UpdateVisualOnOpen() {
        if (chestSpriteRenderer != null && openedSprite != null) {
            chestSpriteRenderer.sprite = openedSprite;
        }

        if (interactionCollider != null) {
            interactionCollider.enabled = false;
        }
    }

    private void HandlePostOpenLifecycle() {
        if (destroyAfterOpen) {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void TryDropLoot() {
        List<WeightedObject> activeList = GetActiveWeightedList();
        if (activeList == null || activeList.Count == 0)
            return;

        float effectiveFrequency = GetEffectiveFrequency();
        if (effectiveFrequency <= 0f)
            return;

        // Roll once: does this chest drop anything at all?
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
            modified *= difficulityManager.playerLuck * luckFrequencyFactor;
        }

        return Mathf.Clamp01(modified);
    }

    private int GetDropCount() {
        // If no custom weights defined → use default min-max uniform distribution
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
            if (pick <= cumulative) {
                return entry.count;
            }
        }

        return dropCountWeights[dropCountWeights.Count - 1].count;
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

        if (dropCountWeights != null) {
            for (int i = 0; i < dropCountWeights.Count; i++) {
                if (dropCountWeights[i].weight < 0f) {
                    var d = dropCountWeights[i];
                    d.weight = 0f;
                    dropCountWeights[i] = d;
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
