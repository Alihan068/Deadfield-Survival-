using UnityEngine;

public class RangedParticleAttack : MonoBehaviour {
    ParticleSystem particleSys;
    ParticleSystem.EmissionModule emissionModule;
    ParticleSystem.MainModule mainModule;
    ParticleSystem.ShapeModule shapeModule;

    StatsManager statsManager;
    void Start() {
        particleSys = GetComponent<ParticleSystem>();
        emissionModule = particleSys.emission;
        mainModule = particleSys.main;
        shapeModule = particleSys.shape;
    }

    void Update() {

    }
    public void ParticleSystemPlay() {

        Debug.Log("ParticleSystemCalled");
        emissionModule.rateOverTime = statsManager.rangedSpeed;
        mainModule.startSpeedMultiplier = statsManager.projectileSpeed;
        mainModule.startSizeMultiplier = statsManager.projectileSize;
        shapeModule.randomDirectionAmount = statsManager.spread / 100;

    }
   

    private void OnParticleCollision(GameObject other) {
        if (other == null || other.layer == this.gameObject.layer) {
            Debug.Log("Target: " + other.name + " is the same layer with attacker: " + this.name);
            return;
        }
        HealthManager targetHealthManager = other.GetComponent<HealthManager>();
        if (targetHealthManager == null) {
            Debug.Log("Target: " + other.name + "Has no healthManager");
        }
        targetHealthManager.CalculateIncomingDamage(statsManager.rangedDamage);
        targetHealthManager.GetKnockback(transform, statsManager.haste);
    }
}
