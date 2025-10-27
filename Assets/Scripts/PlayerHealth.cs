using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    PlayerStatsManager playerStatsManager;
    PlayerController playerController;
    public float playerHp = 100;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
