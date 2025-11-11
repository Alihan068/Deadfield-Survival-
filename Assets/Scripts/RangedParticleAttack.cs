using UnityEngine;

public class RangedParticleAttack : MonoBehaviour {
    ParticleSystem particleSys;
    ParticleSystem.EmissionModule emissionModule;
    ParticleSystem.MainModule mainModule;
    ParticleSystem.ShapeModule shapeModule;

    StatsManager statsManager;
    bool isPlaying = false;
    void Start() {
        statsManager = GetComponentInParent<StatsManager>();
        particleSys = GetComponent<ParticleSystem>();
        emissionModule = particleSys.emission;
        mainModule = particleSys.main;
        shapeModule = particleSys.shape;
    }

    void Update() {

    }
    public void ParticleSystemUpdateStats() {
        if (particleSys == null) return;  
        emissionModule.rateOverTime = statsManager.attackSpeed;
        mainModule.startSpeedMultiplier = statsManager.projectileSpeed;
        mainModule.startSizeMultiplier = statsManager.projectileSize;
        shapeModule.randomDirectionAmount = statsManager.spread / 100;

    }

    public void ParticleSystemToggle(bool state) {

        ParticleSystemUpdateStats();
        if (state) {
            if (!isPlaying) {
                isPlaying = true;
                particleSys.Play();
            }
        }
        else {
            isPlaying = false;
            if (particleSys != null) {
                particleSys.Stop();
            }
        }
    }


    private void OnParticleCollision(GameObject other) {
        Debug.Log("HIT! " + other.name);
        if (other == null || other.layer == this.gameObject.layer) {
    #if UNITY_EDITOR
            Debug.Log("Target: " + other.name + " is the same layer with attacker: " + this.name);
#endif
            return;
        }

        if (other.gameObject.CompareTag("Weapon")) {
            //TODO: DeflectEffects
            return;
        }
        other.TryGetComponent<HealthManager>(out HealthManager targetHealthManager);

        if (targetHealthManager == null) {
            Debug.Log("Target: " + other.name + "Has no healthManager");
        }
        else {
            targetHealthManager.CalculateIncomingDamage(statsManager.baseDamage + statsManager.baseDamage);
        }
    }
}
