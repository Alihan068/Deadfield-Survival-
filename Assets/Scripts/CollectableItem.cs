using UnityEngine;

public class CollectableItem : MonoBehaviour {

    [SerializeField] CollectibleItemSO ItemSO;
    StatsManager statsManager;
    ParticleSystem particleSys;

    private void OnEnable() {

        particleSys = GetComponent<ParticleSystem>();
        if (particleSys != null && ItemSO != null) {
            RarityColor(ItemSO.itemRarity);
        }
        
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        statsManager = collision.GetComponent<StatsManager>();
 
        if (statsManager == null || !statsManager.canCollectItems) return;
        foreach (var itemEffect in ItemSO.itemEffects) {
            if (itemEffect != null) { 
                statsManager.ApplyEffect(itemEffect);
                Debug.Log(itemEffect.targetStat +" " + itemEffect.effectValue + " applied.\nPercentage: " + itemEffect.ifPercentage);
            }
            else {
                Debug.Log(itemEffect.targetStat + " does not found.");
            }
        }
        Destroy(gameObject);
    }

    public void RarityColor(ItemRarity itemRarity) {
        Color rarityColor;

        switch (itemRarity) {
            case ItemRarity.common:
                rarityColor = Color.white;
                break;
            case ItemRarity.rare:
                rarityColor = Color.lightBlue;
                break;
            case ItemRarity.legendary:
                rarityColor = Color.orange;
                break;
            case ItemRarity.unique:
                rarityColor = Color.darkRed;
                break;
            default: rarityColor = Color.gray;
                break;
        }
        var main = particleSys.main;
        main.startColor = rarityColor;
    }

}
