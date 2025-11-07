// Put this in an Editor folder, e.g. Assets/Editor/ItemPrefabGenerator.cs

using System.IO;
using UnityEditor;
using UnityEngine;

public class ItemPrefabGenerator : EditorWindow {
    // Folder that contains CollectibleItemSO assets
    [SerializeField] private DefaultAsset itemSOFolder;

    // Output folder for generated prefabs
    [SerializeField] private DefaultAsset outputFolder;

    // Optional: base prefab to clone (if null, a fresh GameObject will be created)
    [SerializeField] private GameObject basePrefab;

    [MenuItem("Tools/Item Prefab Generator")]
    public static void OpenWindow() {
        GetWindow<ItemPrefabGenerator>("Item Prefab Generator");
    }

    private void OnGUI() {
        EditorGUILayout.LabelField("Source & Output", EditorStyles.boldLabel);
        itemSOFolder = (DefaultAsset)EditorGUILayout.ObjectField("Item SO Folder", itemSOFolder, typeof(DefaultAsset), false);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Prefab Folder", outputFolder, typeof(DefaultAsset), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
        basePrefab = (GameObject)EditorGUILayout.ObjectField("Base Prefab", basePrefab, typeof(GameObject), false);

        EditorGUILayout.Space();
        GUI.enabled = itemSOFolder != null && outputFolder != null;
        if (GUILayout.Button("Generate Item Prefabs")) {
            GeneratePrefabs();
        }
        GUI.enabled = true;
    }

    private void GeneratePrefabs() {
        string soFolderPath = AssetDatabase.GetAssetPath(itemSOFolder);
        string outFolderPath = AssetDatabase.GetAssetPath(outputFolder);

        if (!AssetDatabase.IsValidFolder(soFolderPath) || !AssetDatabase.IsValidFolder(outFolderPath)) {
            Debug.LogError("Invalid folder selection.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:CollectibleItemSO", new[] { soFolderPath });
        if (guids.Length == 0) {
            Debug.LogWarning("No CollectibleItemSO found in the selected folder.");
            return;
        }

        foreach (string guid in guids) {
            var soPath = AssetDatabase.GUIDToAssetPath(guid);
            var itemSO = AssetDatabase.LoadAssetAtPath<CollectibleItemSO>(soPath);
            if (itemSO == null) continue;

            CreateItemPrefab(itemSO, outFolderPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Item prefabs generated.");
    }

    private void CreateItemPrefab(CollectibleItemSO itemSO, string outFolderPath) {
        GameObject go;

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

            // Collider
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            // Rigidbody (optional, good for triggers)
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Ensure CollectibleItem component exists
        var collectible = go.GetComponent<CollectibleItem>();
        if (collectible == null)
            collectible = go.AddComponent<CollectibleItem>();

        // Assign ItemSO (private [SerializeField]) via SerializedObject
        var soCollectible = new SerializedObject(collectible);
        var itemSOProp = soCollectible.FindProperty("ItemSO");
        if (itemSOProp != null) {
            itemSOProp.objectReferenceValue = itemSO;
            soCollectible.ApplyModifiedProperties();
        }
        else {
            Debug.LogWarning($"Could not find 'ItemSO' field on CollectibleItem for {go.name}.");
        }

        // Save as prefab
        string prefabPath = Path.Combine(outFolderPath, go.name + ".prefab");
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
    }

    private string GetItemName(CollectibleItemSO itemSO) {
        if (!string.IsNullOrEmpty(itemSO.itemName))
            return "Item_" + itemSO.itemName.Replace(" ", "");
        return "Item_" + itemSO.name.Replace(" ", "");
    }
}
