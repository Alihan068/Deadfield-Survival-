using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour {
    [Header("Interaction Settings")]
    [SerializeField]  float interactRadius = 2f;
    [SerializeField]  LayerMask interactableMask;

    [Header("Interaction Cooldown")]
    [SerializeField]  float interactionCooldown = 0.1f;

    // Preallocated buffer to avoid GC allocations
     readonly Collider2D[] _overlapResults = new Collider2D[16];

     Transform _origin;
     ContactFilter2D _contactFilter;

    // Cooldown state
     bool interactionLocked = false;
     Coroutine interactionCooldownCoroutine;

     void Awake() {
        _origin = transform;

        // Contact filter setup for OverlapCircle
        _contactFilter = new ContactFilter2D {
            useLayerMask = true,
            layerMask = interactableMask,
            useTriggers = true
        };
    }

     void OnValidate() {
        // Keep contact filter in sync when mask changes in inspector
        _contactFilter = new ContactFilter2D {
            useLayerMask = true,
            layerMask = interactableMask,
            useTriggers = true
        };
    }

    // Input System callback for "Interact" action
     void OnInteract(InputValue value) {
        if (value.Get<float>() < 1f)
            return;

        TryInteract();
    }

     void TryInteract() {
        // Global interaction cooldown to prevent multiple interactables firing in the same press
        if (interactionLocked)
            return;

        interactionLocked = true;

        if (interactionCooldownCoroutine != null) {
            StopCoroutine(interactionCooldownCoroutine);
        }
        interactionCooldownCoroutine = StartCoroutine(InteractionCooldownRoutine());

        Vector2 center = _origin.position;

        // OverlapCircle with ContactFilter2D
        int hitCount = Physics2D.OverlapCircle(
            center,
            interactRadius,
            _contactFilter,
            _overlapResults
        );

        if (hitCount <= 0)
            return;

        IInteractable closest = null;
        float closestSqrDist = float.MaxValue;
        Vector3 originPos = _origin.position;

        for (int i = 0; i < hitCount; i++) {
            Collider2D col = _overlapResults[i];
            if (col == null)
                continue;

            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = col.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            Vector3 targetPos = interactable.GetPosition();
            float sqrDist = (targetPos - originPos).sqrMagnitude;

            if (sqrDist < closestSqrDist) {
                closestSqrDist = sqrDist;
                closest = interactable;
            }
        }

        if (closest != null) {
            closest.Interact(gameObject);
        }
    }

     IEnumerator InteractionCooldownRoutine() {
        yield return new WaitForSeconds(interactionCooldown);
        interactionLocked = false;
        interactionCooldownCoroutine = null;
    }

     void OnDrawGizmosSelected() {
        if (_origin == null)
            _origin = transform;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_origin.position, interactRadius);
    }
}
