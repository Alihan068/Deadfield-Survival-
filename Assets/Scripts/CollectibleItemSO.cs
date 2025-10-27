using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Create ItemSO")]
public class CollectibleItemSO : MonoBehaviour
{
   public enum ItemRarity {
        common,
        //uncommon,
        rare,
        //epic,
        legendary,
        //mythic,
        unique,
    }

    PlayerStatsManager playerStatsManager;

    private void Start() {
        playerStatsManager = GetComponent<PlayerStatsManager>();
    }
}
