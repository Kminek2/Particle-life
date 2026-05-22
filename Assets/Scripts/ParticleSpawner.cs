using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    #region Serialized variables
    [SerializeField] Collider2D playArea;
    [SerializeField] SpawnSettingsSO spawnSettings;
    [SerializeField] Particle particlePrefab;
    [SerializeField] Transform particleParent;
    #endregion

    private void Start()
    {
        Particle.PlayAreaBounds = playArea.bounds;
        Dictionary<ParticleSO, List<Particle>> spawnedParticles = SpawnAll();
        ParticleInteractions.Instance.Particles = spawnedParticles;
        ShaderParticleInteractionsManager.Instance.Particles = spawnedParticles;
    }

    /// <summary>
    /// Spawns all particles set in spawnSettings
    /// </summary>
    /// <returns>Dictionary off all spawned particles grouped by their class</returns>
    private Dictionary<ParticleSO, List<Particle>> SpawnAll()
    {
        Dictionary<ParticleSO, List<Particle>> particles = new();
        foreach (ParticleSpawnSettings particleSpawnSettings in spawnSettings.particleNum)
        {
            ParticleSO particleSO = particleSpawnSettings.particle;
            particles.Add(particleSO, new());

            int spawnNum = particleSpawnSettings.num;
            for (int i = 0; i < spawnNum; i++)
            {
                Particle particle = InstantiateParticle(particleSO);
                SetUpParticle(particle, particleSpawnSettings);
                particles[particleSO].Add(particle);
            }
        }

        return particles;
    }

    #region Single particle spawning functions
    private void SetUpParticle(Particle particle, ParticleSpawnSettings spawnSettings)
    {
        ParticleSO particleSO = spawnSettings.particle;
        Vector3 startingMoveDir = GetRandomNormalizedVec2();
        Vector3 startingMovement = startingMoveDir * spawnSettings.spawnVelocity;

        particle.ParticleSO = particleSO;
        particle.Velocity = startingMovement;
    }

    private Particle InstantiateParticle(ParticleSO particle)
    {
        Vector3 pos = GetRandomSpawnPos();
        Quaternion rot = GetRandomRotation();
        return Instantiate(particlePrefab, pos, rot, particleParent);
    }
    #endregion

    #region Helper functions
    private Vector2 GetRandomNormalizedVec2()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);

        Vector2 vec = new(x, y);
        return vec.normalized;
    }

    private Vector3 GetRandomSpawnPos()
    {
        Bounds playAreaBounds = playArea.bounds;
        Vector3 boundsMin = playAreaBounds.min;
        Vector3 boundsMax = playAreaBounds.max;

        float x = Random.Range(boundsMin.x, boundsMax.x);
        float y = Random.Range(boundsMin.y, boundsMax.y);
        float z = Random.Range(boundsMin.z, boundsMax.z);

        return new(x, y, z);
    }

    //For now rotation is unused. But if I were to ever change it in the future it'll be easier this way (having a function already)
    private Quaternion GetRandomRotation()
    {
        return Quaternion.identity;
    }
    #endregion
}
