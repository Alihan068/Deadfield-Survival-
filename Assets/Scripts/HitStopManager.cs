using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class HitStopManager : MonoBehaviour {
    public static HitStopManager Instance { get; private set; }

    [Header("Hit Stop Settings")]
    [SerializeField] float baseFreezeTime = 0.05f;
    //[SerializeField] float damageMultiplier = 0.01f; // Additional freeze time per damage point
    [SerializeField] float maxFreezeTime = 0.3f;

    private Coroutine hitStopCoroutine;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Triggers a hit stop effect based on damage dealt
    /// </summary>
    /// <param name="damage">The amount of damage dealt</param>
    public void TriggerHitStop(float damage) {
        float freezeDuration = CalculateFreezeDuration(damage);

        if (hitStopCoroutine != null) {
            StopCoroutine(hitStopCoroutine);
        }

        hitStopCoroutine = StartCoroutine(HitStopCoroutine(freezeDuration));
    }

    /// <summary>
    /// Calculates freeze duration based on damage
    /// </summary>
    public float CalculateFreezeDuration(float damage) {
        float duration = baseFreezeTime /*+ (damage * damageMultiplier)*/;
        //Debug.Log("Duration: " + duration);
        return Mathf.Min(duration, maxFreezeTime);
    }

    IEnumerator HitStopCoroutine(float duration) {

            // Find all objects with CustomTime component
            CustomTime[] allTimedObjects = FindObjectsByType<CustomTime>(FindObjectsSortMode.None);

            // Freeze all objects
            foreach (CustomTime timedObject in allTimedObjects) {
                //Debug.Log(timedObject.name + "Freeze");
                timedObject.Freeze();
            }

            // Wait for the freeze duration (using unscaled time)
            yield return new WaitForSecondsRealtime(duration);
            // Unfreeze all objects
            foreach (CustomTime timedObject in allTimedObjects) {             
                if (timedObject != null) {
                    //Debug.Log(timedObject.name + "Unfreeze");
                    timedObject.Unfreeze();
                }
            }

            hitStopCoroutine = null;
        }
    

    /// <summary>
    /// Manually trigger a hit stop with custom duration
    /// </summary>
    public void TriggerHitStopCustom(float duration) {
        if (hitStopCoroutine != null) {
            StopCoroutine(hitStopCoroutine);
        }

        hitStopCoroutine = StartCoroutine(HitStopCoroutine(duration));
    }
}