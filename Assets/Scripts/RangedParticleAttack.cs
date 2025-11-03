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
        emissionModule.rateOverTime = statsManager.rangedSpeed;
        mainModule.startSpeedMultiplier = statsManager.projectileSpeed;
        mainModule.startSizeMultiplier = statsManager.projectileSize;
        shapeModule.randomDirectionAmount = statsManager.spread / 100;

    }

    public void ParticleSystemToggle(bool state) {
        
        ParticleSystemUpdateStats();
        if (state) {
            if (!isPlaying) {
                isPlaying = true;
                Debug.Log("PlayRangedAttack");
                particleSys.Play();
            }
        }
        else {
            isPlaying = false;
            particleSys.Stop();
        }
    }


    private void OnParticleCollision(GameObject other) {
        Debug.Log(other.name);
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
