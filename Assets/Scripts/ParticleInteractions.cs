using System.Collections.Generic;
using UnityEngine;

public class ParticleInteractions : MonoBehaviour
{
    private Dictionary<ParticleSO, List<Particle>> _particles;

    public static ParticleInteractions Instance
    {
        get; private set;
    }

    #region Getters And Setters
    public Dictionary<ParticleSO, List<Particle>> Particles
    {
        get { return _particles; }
        set { _particles = value; }
    }

    #endregion

    #region Init
    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;
    }

    #endregion

    #region Updates
    private void Update()
    {
        if (_particles == null)
            return;

        foreach (ParticleSO particleSO in _particles.Keys)
            UpdateParticles(particleSO);
    }

    private void UpdateParticles(ParticleSO particleSO)
    {
        List<Particle> particles = GetParticles(particleSO);
        List<ParticleAttraction> particleAttractions = particleSO.attractions;

        foreach (ParticleAttraction attraction in particleAttractions)
            UpdateAttractions(particles, attraction);
    }

    private void UpdateAttractions(List<Particle> particles, ParticleAttraction particleAttraction)
    {
        ParticleSO attractionParticleSO = particleAttraction.particle;
        List<Particle> attractionParticles = GetParticles(attractionParticleSO);

        foreach (Particle particle in particles)
            foreach (Particle attractionParticle in attractionParticles)
                if (attractionParticle != particle)
                    particle.Attract(attractionParticle, particleAttraction);
    }

    #endregion

    #region Helpers
    private List<Particle> GetParticles(ParticleSO particleSO)
    {
        return _particles.GetValueOrDefault(particleSO);
    }

    #endregion
}
