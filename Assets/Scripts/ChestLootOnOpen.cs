using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DropCountWeight {
    public int count;
    public float weight;
}

public class ChestLootOnOpen : MonoBehaviour, IInteractable {
    [Header("Loot Source")]
    public LootTableSO lootTable;

    [Header("Local Loot Table (used if LootTable is null)")]
    [Range(0f, 1f)]
    public float frequency = 1f;
    public List<WeightedObject> weightedObjectList = new List<WeightedObject>();

    [Header("Drop Amount (Fallback Min/Max)")]
    public int minDrops = 1;
    public int maxDrops = 1;

    [Header("Drop Count Weights (Overrides Min/Max if not empty)")]
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
    public float externalDropRateModifier = 0f;

    [Header("Luck")]
    public bool useLuckFrequencyModifier = true;
    public float luckFrequencyFactor = 0.01f;

    [Header("Chest Visuals / State")]
    public SpriteRenderer chestSpriteRenderer;
    public Sprite closedSprite;
    public Sprite openedSprite;
    public Collider2D interactionCollider;
    public bool destroyAfterOpen = false;
    public float destroyDelay = 0.5f;

    [Header("Spawn Timing")]
    [Tooltip("Delay between each item dropping out of the chest.")]
    public float dropInterval = 0.3f;

     bool isOpened = false;
     DifficulityManager difficulityManager;

     void Awake() {
        difficulityManager = FindFirstObjectByType<DifficulityManager>();

        if (chestSpriteRenderer == null)
            chestSpriteRenderer = GetComponent<SpriteRenderer>();

        if (interactionCollider == null)
            interactionCollider = GetComponent<Collider2D>();

        if (chestSpriteRenderer != null && closedSprite != null)
            chestSpriteRenderer.sprite = closedSprite;
    }

    public void OpenChest() {
        if (isOpened)
            return;

        isOpened = true;

        // Spawn loot with delay between items
        StartCoroutine(TryDropLootWithDelay());

        UpdateVisualOnOpen();
        ApplyOpeningEffects();
        HandlePostOpenLifecycle();
    }

    // IInteractable
    public void Interact(GameObject interactor) {
        // Key, stats etc. checks can be applied here in future
        OpenChest();
    }

    // IInteractable
    public Vector3 GetPosition() {
        return transform.position;
    }

    public void ApplyOpeningEffects() {
        // FX or SFX can be added here in future
    }

     void UpdateVisualOnOpen() {
        if (chestSpriteRenderer != null && openedSprite != null)
            chestSpriteRenderer.sprite = openedSprite;

        if (interactionCollider != null)
            interactionCollider.enabled = false;
    }

     void HandlePostOpenLifecycle() {
        if (destroyAfterOpen)
            Destroy(gameObject, destroyDelay);
    }

     System.Collections.IEnumerator TryDropLootWithDelay() {
        List<WeightedObject> activeList = GetActiveWeightedList();
        if (activeList == null || activeList.Count == 0)
            yield break;

        float effectiveFrequency = GetEffectiveFrequency();
        if (effectiveFrequency <= 0f)
            yield break;

        if (Random.value > effectiveFrequency)
            yield break;

        int dropsToSpawn = GetDropCount();
        if (dropsToSpawn <= 0)
            yield break;

        for (int i = 0; i < dropsToSpawn; i++) {
            float totalWeight = CalculateTotalWeight(activeList);
            if (totalWeight <= 0f)
                yield break;

            if (!TryGetRandomWeightedObject(activeList, totalWeight, out WeightedObject selectedEntry))
                yield break;

            GameObject prefab = selectedEntry.GetRandomPrefab();
            if (prefab == null)
                continue;

            Vector3 spawnPos = GetSpawnPosition();
            Quaternion spawnRot = Quaternion.identity;

            GameObject instance = SpawnLootObject(prefab, spawnPos, spawnRot);
            if (usePhysicsImpulse) {
                ApplyImpulse(instance);
            }

            if (dropInterval > 0f)
                yield return new WaitForSeconds(dropInterval);
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

        if (useLuckFrequencyModifier && difficulityManager != null) {
            float luck = Mathf.Max(0f, difficulityManager.playerLuck);
            float luckBonus = luck * luckFrequencyFactor;
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
            if (pick <= cumulative) {
                return entry.count;
            }
        }

        return dropCountWeights[dropCountWeights.Count - 1].count;
    }

     float CalculateTotalWeight(List<WeightedObject> list) {
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

     bool TryGetRandomWeightedObject(List<WeightedObject> list, float totalWeight, out WeightedObject selected) {
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

     float GetRarityMultiplier(ItemRarity rarity) {
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

     Vector3 GetSpawnPosition() {
        if (!useRandomSpread || dropRadius <= 0f)
            return transform.position;

        Vector2 offset = Random.insideUnitCircle * dropRadius;
        return transform.position + (Vector3)offset;
    }

     GameObject SpawnLootObject(GameObject prefab, Vector3 position, Quaternion rotation) {
        if (prefab == null)
            return null;

        return Instantiate(prefab, position, rotation);
    }

     void ApplyImpulse(GameObject instance) {
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

     void OnValidate() {
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

    // Temporary direct input (test only). Remove when PlayerInteractor is active.
     void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E)) {
            OpenChest();
        }
    }
}
