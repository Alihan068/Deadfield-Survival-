using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour {
    [Header("Pickup Popup")]
    [SerializeField] private GameObject popupRoot;          // Parent panel
    [SerializeField] private Image popupIcon;               // Item image
    [SerializeField] private TextMeshProUGUI popupText;     // Description
    [SerializeField] private float popupDuration = 1.5f;

    [Header("Pause/Tutorial")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private InputActionReference pauseAction;


    private Coroutine popupRoutine;
    private bool gameStarted = false;
    private bool isPaused = false;

    private void Awake() {
        if (popupRoot != null)
            popupRoot.SetActive(false);

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);
        //if (mainCanvas != null) mainCanvas.SetActive(false);

        TogglePause(false);
    }

    private void OnEnable() {
        if (pauseAction != null) {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePerformed;
        }
    }

    private void OnDisable() {
        if (pauseAction != null) {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context) {
        if (!gameStarted) {
            gameStarted = true;
            if (tutorialCanvas != null) {
                tutorialCanvas.SetActive(false);
                if (mainCanvas!= null && !mainCanvas.activeInHierarchy)
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

        string title = string.IsNullOrEmpty(itemSO.itemName) ? itemSO.name : itemSO.itemName;

        List<string> parts = new List<string>();
        foreach (var effect in itemSO.itemEffects) {
            if (effect == null) continue;

            string sign = effect.ifIncrease ? "+" : "-";
            string value = effect.ifPercentage
                ? (effect.effectValue.ToString("0") + "%")
                : effect.effectValue.ToString("0.##");

            parts.Add($"{sign}{value} {effect.targetStat}");
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

    private IEnumerator HidePickupPopup() {
        yield return new WaitForSeconds(popupDuration);
        popupRoot.SetActive(false);
        popupRoutine = null;
    }
}