using UnityEngine;

[System.Serializable]
public struct WeightedObject {
    public ItemRarity Rarity;
    public GameObject[] collectibleItem;
    public float weight;

    /// <summary>
    /// Returns a random prefab from this entry's collectibleItem array.
    /// Can return null if the array is null or empty.
    /// </summary>
    public GameObject GetRandomPrefab() {
        if (collectibleItem == null || collectibleItem.Length == 0) {
            return null;
        }

        if (collectibleItem.Length == 1) {
            return collectibleItem[0];
        }

        int index = Random.Range(0, collectibleItem.Length);
        return collectibleItem[index];
    }
}

[System.Serializable]
public struct RarityWeightMultiplier {
    public ItemRarity rarity;
    public float multiplier;
}
