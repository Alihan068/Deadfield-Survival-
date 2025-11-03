using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySetTarget : MonoBehaviour {
    Transform target;
    RangedParticleAttack rangedParticle;
    PlayerController player;

    void OnEnable() {
        rangedParticle = GetComponentInChildren<RangedParticleAttack>();
        player = FindFirstObjectByType<PlayerController>();
        target = player.transform;
        Debug.Log(target.transform);
    }

    // Update is called once per frame
    void Update() {
        AimForTarget();
    }

    void AimForTarget() {
        Vector2 targetPos = target.position;
        Vector2 targetDir = (target.position - transform.position);
        rangedParticle.transform.right = targetDir;


    }
}
