using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CollectibleItem {
    public GameObject collectibleItemPrefab;
    public ItemRarity itemRarity;
    [Min(0f)] public float weight;
}

public class DropLootOnDeath : MonoBehaviour {
    [Range(0f, 1f)]
    public float generalDropFrequency = 1f; // 0 - never create. 1 - always create. 0.5 - create 50% of the time.

    public List<CollectibleItem> collectibleItems;

    // Use this for initialization
    void Start() {
        ValidateWeightedObjects();
    }

    private void OnDestroy() {
        if (Application.isPlaying && !gameObject.scene.isLoaded)
            return;

        if (Random.Range(0f, 0.99f) < generalDropFrequency) {
            GameObject gameObjectToCreate = SelectWeightedObject();
            if (gameObjectToCreate != null) {
                Instantiate(gameObjectToCreate, transform.position, transform.rotation);
            }
        }
    }

    private GameObject SelectWeightedObject() {
        List<CollectibleItem> validObjects = GetValidObjects();

        if (validObjects.Count == 0) {
            Debug.LogWarning($"{name}: No valid weighted objects to spawn!");
            return null;
        }

        GameObject selected = null;
        float maxChoice = CalculateSumOfWeights(validObjects);
        float randChoice = Random.Range(0f, maxChoice);
        float weightSum = 0f;

        foreach (CollectibleItem weightedObject in validObjects) {
            weightSum += weightedObject.weight;
            if (randChoice <= weightSum) {
                selected = weightedObject.collectibleItemPrefab;
                break;
            }
        }

        return selected;
    }

    private List<CollectibleItem> GetValidObjects() {
        return collectibleItems;
    }

    private float CalculateSumOfWeights(List<CollectibleItem> objects) {
        float sumOfWeights = 0f;
        foreach (CollectibleItem weightedObject in objects) {
            sumOfWeights += weightedObject.weight;
        }

        return sumOfWeights;
    }

    private float SumOfWeights {
        get {
            return CalculateSumOfWeights(collectibleItems);
        }
    }

    // Validation method to check if prefabs have GenericItem component
    private void ValidateWeightedObjects() {
        foreach (CollectibleItem collectibleItem in collectibleItems) {
            if (collectibleItem.collectibleItemPrefab == null) {
                Debug.LogWarning($"{name}: WeightedObject has null prefab!");
                continue;
            }

            CollectableItem genericItem = collectibleItem.collectibleItemPrefab.GetComponent<CollectableItem>();
            if (genericItem == null) {
                Debug.LogError($"{name}: Prefab '{collectibleItem.collectibleItemPrefab.name}' is missing GenericItem component!");
            }
        }
    }

    // Helper method to add weighted objects at runtime if needed
    public void AddWeightedObject(GameObject prefab, ItemRarity rarity, float weight) {
        CollectibleItem newWeightedObject = new CollectibleItem {
            collectibleItemPrefab = prefab,
            itemRarity = rarity,
            weight = weight
        };

        collectibleItems.Add(newWeightedObject);
    }

    // Helper method to get total drop chance for a specific rarity
    public float GetRarityDropChance(ItemRarity rarity) {
        float rarityWeight = 0f;
        float totalWeight = SumOfWeights;

        foreach (CollectibleItem weightedObject in collectibleItems) {
            if (weightedObject.itemRarity == rarity) {
                rarityWeight += weightedObject.weight;
            }
        }

        return totalWeight > 0 ? (rarityWeight / totalWeight) * generalDropFrequency : 0f;
    }
}
