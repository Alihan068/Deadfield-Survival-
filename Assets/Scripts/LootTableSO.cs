using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Loot Table", fileName = "LootTable")]
public class LootTableSO : ScriptableObject {
    [Range(0f, 1f)]
    public float frequency = 1f; // Base chance to drop at all.

    public List<WeightedObject> entries = new List<WeightedObject>();
}
