using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelManager : MonoBehaviour
{
    StatsManager statsManager;
    [SerializeField] Slider xpBar;


    public int playerLevel = 1;
    public float playerCurrentXP = 0f;
    public float playerXpToNextLevel = 1000f;
    public int playerXPCap = 100;
    float excessXP;
    
    void Start()
    {
        statsManager = GetComponent<StatsManager>();
        xpBar.maxValue = playerXpToNextLevel;
        xpBar.value = playerCurrentXP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GainXP(float gainedXpAmount) {
        playerCurrentXP += gainedXpAmount;
        if (playerCurrentXP > playerXpToNextLevel) {
            excessXP = playerCurrentXP - playerXpToNextLevel;
            IncreaseLevel();
        }
        else {
            UpdateXPBar();
        }
    
    }

    void IncreaseLevel() {
        playerLevel++;
        playerCurrentXP = excessXP;
        UpdateNextLevelXpREq();
        UpdateXPBar();
    }
    void UpdateXPBar() {
        xpBar.maxValue = playerXpToNextLevel;
        xpBar.value = playerCurrentXP;        
    }

    void UpdateNextLevelXpREq() {
        playerXpToNextLevel =Mathf.RoundToInt(playerXpToNextLevel + playerXpToNextLevel / 100 * 20);

    }
}
