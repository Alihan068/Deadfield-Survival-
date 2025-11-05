using UnityEngine;

public class ForceSettings : MonoBehaviour {
    [SerializeField] int targetFrameRate = 60;

    private static ForceSettings instance;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Makes this GameObject persist across scenes

        ApplySettings();
    }

    private void ApplySettings() {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }
}