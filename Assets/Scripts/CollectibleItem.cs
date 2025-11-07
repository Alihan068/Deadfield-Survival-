using UnityEngine;

public class CollectibleItem : MonoBehaviour {
    [SerializeField] private CollectibleItemSO ItemSO;
    private StatsManager statsManager;
    private ParticleSystem particleSys;
    private UIManager uiManager;

    private void OnEnable() {
        particleSys = GetComponent<ParticleSystem>();
        uiManager = FindFirstObjectByType<UIManager>();

        if (particleSys != null && ItemSO != null)
            RarityColor(ItemSO.itemRarity);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        statsManager = collision.GetComponent<StatsManager>();
        if (statsManager == null || !statsManager.canCollectItems) return;

        foreach (var itemEffect in ItemSO.itemEffects) {
            if (itemEffect != null)
                statsManager.ApplyEffect(itemEffect);
        }

        if (uiManager != null)
            uiManager.ShowItemPickup(ItemSO);

        Destroy(gameObject);
    }

    public void RarityColor(ItemRarity itemRarity) {
        if (particleSys == null) return;

        Color rarityColor;
        switch (itemRarity) {
            case ItemRarity.common: rarityColor = Color.white; break;
            case ItemRarity.rare: rarityColor = Color.cyan; break;
            case ItemRarity.legendary: rarityColor = new Color(1f, 0.5f, 0f); break;
            case ItemRarity.unique: rarityColor = Color.red; break;
            default: rarityColor = Color.gray; break;
        }

        var main = particleSys.main;
        main.startColor = rarityColor;
    }
}
