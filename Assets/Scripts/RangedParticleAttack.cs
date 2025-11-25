using UnityEngine;

public class RangedParticleAttack : MonoBehaviour
{
    ParticleSystem particleSys;
    ParticleSystem.EmissionModule emissionModule;
    ParticleSystem.MainModule mainModule;
    ParticleSystem.ShapeModule shapeModule;

    StatsManager statsManager;
    bool isPlaying = false;

    void Start()
    {
        statsManager = GetComponentInParent<StatsManager>();
        particleSys = GetComponent<ParticleSystem>();

        if (particleSys != null)
        {
            emissionModule = particleSys.emission;
            mainModule = particleSys.main;
            shapeModule = particleSys.shape;
        }
    }

    public void ParticleSystemUpdateStats()
    {
        if (particleSys == null || statsManager == null) return;

        // Fire rate depends on effective attack speed
        emissionModule.rateOverTime = statsManager.EffectiveAttackSpeed;

        // Projectile flight speed
        mainModule.startSpeedMultiplier = statsManager.EffectiveProjectileSpeed;

        // Spread still directly uses spread percentage
        shapeModule.randomDirectionAmount = statsManager.spread / 100f;
    }

    public void ParticleSystemToggle(bool state)
    {
        if (particleSys == null) return;

        ParticleSystemUpdateStats();

        if (state)
        {
            if (!isPlaying)
            {
                isPlaying = true;
                particleSys.Play();
            }
        }
        else
        {
            isPlaying = false;
            particleSys.Stop();
        }
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("HIT! " + other.name);

        if (other == null || other.layer == this.gameObject.layer)
        {
#if UNITY_EDITOR
            Debug.Log("Target: " + other.name + " is the same layer with attacker: " + this.name);
#endif
            return;
        }

        if (other.gameObject.CompareTag("Weapon"))
        {
            // Deflect or parry effects can be added here in future
            return;
        }

        if (!other.TryGetComponent<HealthManager>(out HealthManager targetHealthManager) || targetHealthManager == null)
        {
            Debug.Log("Target: " + other.name + " has no HealthManager");
            return;
        }

        float damage = statsManager.EffectiveDamage;
        targetHealthManager.CalculateIncomingDamage(damage);
    }
}
