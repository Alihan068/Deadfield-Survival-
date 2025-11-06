using UnityEngine;

public class CollectableItem : MonoBehaviour {

    [SerializeField] CollectibleItemSO ItemSO;
    StatsManager statsManager;

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
    

}
