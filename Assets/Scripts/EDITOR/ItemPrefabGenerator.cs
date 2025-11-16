using System.IO;
using UnityEditor;
using UnityEngine;

public class ItemPrefabGenerator : EditorWindow {
    [SerializeField]  DefaultAsset itemSOFolder;
    [SerializeField]  DefaultAsset outputFolder;
    [SerializeField]  GameObject basePrefab;
    [SerializeField]  GameObject shineParticlePrefab;

    // Physics layer used by interactable items
     const string ItemLayerName = "Interactable";

    // Sorting layer used by item sprites
     const string ItemSortingLayerName = "Items";

    [MenuItem("Tools/Item Prefab Generator")]
    public static void OpenWindow() {
        GetWindow<ItemPrefabGenerator>("Item Prefab Generator");
    }

     void OnGUI() {
        EditorGUILayout.LabelField("Source and Output", EditorStyles.boldLabel);
        itemSOFolder = (DefaultAsset)EditorGUILayout.ObjectField("Item SO Folder", itemSOFolder, typeof(DefaultAsset), false);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Prefab Folder", outputFolder, typeof(DefaultAsset), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
        basePrefab = (GameObject)EditorGUILayout.ObjectField("Base Prefab", basePrefab, typeof(GameObject), false);
        shineParticlePrefab = (GameObject)EditorGUILayout.ObjectField("Shine Particle Prefab", shineParticlePrefab, typeof(GameObject), false);

        EditorGUILayout.Space();
        GUI.enabled = itemSOFolder != null && outputFolder != null;
        if (GUILayout.Button("Generate Item Prefabs")) {
            GeneratePrefabs();
        }
        GUI.enabled = true;
    }

     void GeneratePrefabs() {
        string soFolderPath = AssetDatabase.GetAssetPath(itemSOFolder);
        string outFolderPath = AssetDatabase.GetAssetPath(outputFolder);

        if (!AssetDatabase.IsValidFolder(soFolderPath) || !AssetDatabase.IsValidFolder(outFolderPath)) {
            Debug.LogError("Invalid source or output folder.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:CollectibleItemSO", new[] { soFolderPath });
        if (guids.Length == 0) {
            Debug.LogWarning("No CollectibleItemSO assets found.");
            return;
        }

        foreach (string guid in guids) {
            string soPath = AssetDatabase.GUIDToAssetPath(guid);
            var itemSO = AssetDatabase.LoadAssetAtPath<CollectibleItemSO>(soPath);
            if (itemSO == null) continue;
            if (!itemSO.generateDefaultPrefab) continue;

            CreateItemPrefab(itemSO, outFolderPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Item prefab generation completed.");
    }

     void CreateItemPrefab(CollectibleItemSO itemSO, string rootOutputPath) {
        // Ensure rarity-based subfolder exists
        string rarityFolderPath = EnsureRaritySubfolder(rootOutputPath, itemSO.itemRarity);

        // Prevent duplicate prefab creation for the same SO
        if (PrefabAlreadyExists(rarityFolderPath, itemSO)) {
            Debug.Log($"Prefab already exists for '{itemSO.name}', skipping.");
            return;
        }

        GameObject go;

        // Use base prefab if provided
        if (basePrefab != null) {
            go = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            go.name = GetItemName(itemSO);
        }
        else {
            go = new GameObject(GetItemName(itemSO));

            // SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            if (itemSO.itemIcon != null)
                sr.sprite = itemSO.itemIcon;

            sr.sortingLayerName = ItemSortingLayerName;
            sr.sortingOrder = 1;

            // Collider
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            if (sr.sprite != null)
                col.size = sr.sprite.bounds.size;

            // Rigidbody for loot physics
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            rb.linearDamping = 4f;
            rb.angularDamping = 4f;
        }

        // Attach shine particle child if provided
        if (shineParticlePrefab != null) {
            GameObject shineInstance =
                (GameObject)PrefabUtility.InstantiatePrefab(shineParticlePrefab, go.transform);

            shineInstance.name = "ShineParticle";
            shineInstance.transform.localPosition = Vector3.zero;
            shineInstance.transform.localRotation = Quaternion.identity;
            shineInstance.transform.localScale = Vector3.one;

            // Force shine particle color based on item rarity
            var shinePs = shineInstance.GetComponent<ParticleSystem>();
            if (shinePs != null) {
                var main = shinePs.main;
                main.startColor = GetRarityColorForItem(itemSO);
            }
        }

        // Ensure CollectibleItem exists and bind SO
        var collectible = go.GetComponent<CollectibleItem>();
        if (collectible == null)
            collectible = go.AddComponent<CollectibleItem>();

        collectible.ItemSO = itemSO;

        // Also enforce correct color on any particle under the root
        ApplyRarityColorToParticles(go, itemSO);

        // Apply physics layer for interactables
        int itemLayer = LayerMask.NameToLayer(ItemLayerName);
        if (itemLayer != -1)
            go.layer = itemLayer;

        // Ensure sprite renderer uses desired sorting settings even when basePrefab is used
        var srExisting = go.GetComponentInChildren<SpriteRenderer>();
        if (srExisting != null) {
            srExisting.sortingLayerName = ItemSortingLayerName;
            srExisting.sortingOrder = 1;
        }

        // Save prefab into rarity folder
        string prefabPath = Path.Combine(rarityFolderPath, go.name + ".prefab").Replace("\\", "/");
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
    }

     void ApplyRarityColorToParticles(GameObject root, CollectibleItemSO itemSO) {
        if (root == null || itemSO == null) return;

        ParticleSystem ps = root.GetComponentInChildren<ParticleSystem>();
        if (ps == null) return;

        var main = ps.main;
        main.startColor = GetRarityColorForItem(itemSO);
    }

     Color GetRarityColorForItem(CollectibleItemSO so) {
        if (so == null)
            return Color.white;

        // If SO has a custom color set, use it
        if (so.rarityColor.a > 0f)
            return so.rarityColor;

        // Fallback to default mapping (mirrors CollectibleItem.GetDefaultRarityColor)
        switch (so.itemRarity) {
            case ItemRarity.common: return Color.white;
            case ItemRarity.uncommon: return new Color(0.4f, 1f, 0.4f);
            case ItemRarity.rare: return Color.cyan;
            case ItemRarity.epic: return new Color(0.7f, 0.3f, 1f);
            case ItemRarity.legendary: return new Color(1f, 0.5f, 0f);
            case ItemRarity.mythic: return Color.magenta;
            case ItemRarity.unique: return Color.red;
            default: return Color.gray;
        }
    }

     string GetItemName(CollectibleItemSO itemSO) {
        string cleanName = itemSO.name.Replace(" ", "");
        return $"{itemSO.itemRarity}_Item_{cleanName}";
    }

     string EnsureRaritySubfolder(string rootOutputPath, ItemRarity rarity) {
        rootOutputPath = rootOutputPath.Replace("\\", "/");
        string rarityFolderName = rarity.ToString();
        string rarityFolderPath = Path.Combine(rootOutputPath, rarityFolderName).Replace("\\", "/");

        if (!AssetDatabase.IsValidFolder(rarityFolderPath)) {
            AssetDatabase.CreateFolder(rootOutputPath, rarityFolderName);
        }

        return rarityFolderPath;
    }

     bool PrefabAlreadyExists(string rarityFolderPath, CollectibleItemSO itemSO) {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { rarityFolderPath });

        foreach (string guid in prefabGuids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var collectible = prefab.GetComponent<CollectibleItem>();
            if (collectible == null) continue;

            if (collectible.ItemSO == itemSO)
                return true;
        }

        return false;
    }
}
