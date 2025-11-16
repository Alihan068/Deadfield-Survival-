using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour {
    [Header("Interaction Settings")]
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private LayerMask interactableMask;

    // Preallocated buffer to avoid GC allocations
    private readonly Collider2D[] _overlapResults = new Collider2D[16];

    private Transform _origin;
    private ContactFilter2D _contactFilter;

    private void Awake() {
        _origin = transform;

        // Contact filter setup for OverlapCircle
        _contactFilter = new ContactFilter2D {
            useLayerMask = true,
            layerMask = interactableMask,
            useTriggers = true
        };
    }

    private void OnValidate() {
        // Keep contact filter in sync when mask changes in inspector
        _contactFilter = new ContactFilter2D {
            useLayerMask = true,
            layerMask = interactableMask,
            useTriggers = true
        };
    }

    // Input System callback for "Interact" action
    private void OnInteract(InputValue value) {
        if (value.Get<float>() < 1f)
            return;

        TryInteract();
    }

    private void TryInteract() {
        Vector2 center = _origin.position;

        // New API: OverlapCircle with ContactFilter2D instead of OverlapCircleNonAlloc
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

    private void OnDrawGizmosSelected() {
        if (_origin == null)
            _origin = transform;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_origin.position, interactRadius);
    }
}
