using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour {
    [Header("Pickup Popup")]
    [SerializeField]  GameObject popupRoot;
    [SerializeField]  Image popupIcon;
    [SerializeField]  TextMeshProUGUI popupText;
    [SerializeField]  float popupDuration = 1.5f;

    [Header("Pause/Tutorial")]
    [SerializeField]  GameObject tutorialCanvas;
    [SerializeField]  GameObject mainCanvas;
    [SerializeField]  InputActionReference pauseAction;

     Coroutine popupRoutine;
     bool gameStarted = false;
     bool isPaused = false;

     void Awake() {
        if (popupRoot != null)
            popupRoot.SetActive(false);

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);

        TogglePause(false);
    }

     void OnEnable() {
        if (pauseAction != null) {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePerformed;
        }
    }

     void OnDisable() {
        if (pauseAction != null) {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

     void OnPausePerformed(InputAction.CallbackContext context) {
        if (!gameStarted) {
            gameStarted = true;
            if (tutorialCanvas != null) {
                tutorialCanvas.SetActive(false);
                if (mainCanvas != null && !mainCanvas.activeInHierarchy)
                    mainCanvas.SetActive(true);
            }

            TogglePause(true);
        }
        else {
            isPaused = !isPaused;
            TogglePause(!isPaused);
        }
    }

    public void TogglePause(bool isGameRunning) {
        Time.timeScale = isGameRunning ? 1 : 0;

        CustomTime[] allCustomTimes = FindObjectsByType<CustomTime>(FindObjectsSortMode.None);
        foreach (CustomTime ct in allCustomTimes) {
            ct.timeScale = isGameRunning ? 1f : 0f;
        }
    }

    public void ShowItemPickup(CollectibleItemSO itemSO) {
        if (itemSO == null || itemSO.itemEffects == null || itemSO.itemEffects.Count == 0)
            return;

        // Artık isim direkt ScriptableObject adından geliyor
        string title = itemSO.name;

        List<string> parts = new List<string>();
        foreach (var effect in itemSO.itemEffects) {
            if (effect == null) continue;

            string sign = effect.ifIncrease ? "+" : "-";
            string value = effect.ifPercentage
                ? (effect.effectValue.ToString("0") + "%")
                : effect.effectValue.ToString("0.##");

            string statLabel = string.IsNullOrEmpty(effect.customLabel)
                ? effect.targetStat.ToString()
                : effect.customLabel;

            parts.Add($"{sign}{value} {statLabel}");
        }

        string body = string.Join(", ", parts);
        popupText.text = $"{title}\n{body}";

        if (popupIcon != null) {
            if (itemSO.itemIcon != null) {
                popupIcon.sprite = itemSO.itemIcon;
                popupIcon.enabled = true;
            }
            else {
                popupIcon.enabled = false;
            }
        }

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoot.SetActive(true);
        popupRoutine = StartCoroutine(HidePickupPopup());
    }

     IEnumerator HidePickupPopup() {
        yield return new WaitForSeconds(popupDuration);
        popupRoot.SetActive(false);
        popupRoutine = null;
    }
}
