using UnityEngine;

public class WeaponFlipper : MonoBehaviour {
    Transform enemyRootTransform;
    Transform playerTransform;
    Vector3 baseScale;

    void Awake() {
        var ec = GetComponentInParent<EnemyController>();
        if (ec != null) enemyRootTransform = ec.transform;
        else enemyRootTransform = transform.root;
    }

    void OnEnable() {
        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) playerTransform = pc.transform;
        else playerTransform = null;

        baseScale = transform.localScale;
        baseScale.x = Mathf.Abs(baseScale.x);
    }

    void Update() {
        if (playerTransform == null || enemyRootTransform == null) return;

        float dx = playerTransform.position.x - enemyRootTransform.position.x;
        if (Mathf.Abs(dx) <= Mathf.Epsilon) return;

        float sign;
        if (dx < 0f) sign = -1f;
        else sign = 1f;

        transform.localScale = new Vector3(baseScale.x, baseScale.y * sign, baseScale.z);
    }
}