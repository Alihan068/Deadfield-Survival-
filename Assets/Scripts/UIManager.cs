using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [Header("Pickup Popup")]
    [SerializeField] private GameObject popupRoot;          // Parent panel
    [SerializeField] private Image popupIcon;               // Item image
    [SerializeField] private TextMeshProUGUI popupText;     // Description
    [SerializeField] private float popupDuration = 1.5f;

    private Coroutine popupRoutine;

    private void Awake() {
        if (popupRoot != null)
            popupRoot.SetActive(false);
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
