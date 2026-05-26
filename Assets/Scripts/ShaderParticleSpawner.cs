using System.Collections.Generic;
using UnityEngine;

public class ShaderParticleSpawner : MonoBehaviour
{
    [SerializeField] Collider2D _playArea;
    [SerializeField] SpawnSettingsSO _spawnSettings;

    private readonly Dictionary<ParticleSO, int> _shaderParticleTypes = new();
    private readonly List<ShaderParticle> _spawnedParticles = new();
    private readonly List<ShaderParticleInteractions> _interactions = new();

    private void Start()
    {
        if (ShaderParticleInteractionsManager.Instance.Particles == null)
            GenerateData();

        _shaderParticleTypes.Clear();
        _spawnedParticles.Clear();
        _interactions.Clear();

        gameObject.SetActive(false);
    }

    private void GenerateData()
    {
        ShaderParticleType[] particleTypes = GenerateParticleTypes();
        ShaderParticle[] shaderParticles = _spawnedParticles.ToArray();
        ShaderParticleInteractions[] shaderParticleInteractions = _interactions.ToArray();

        ShaderParticleInteractionsManager.Instance.SetShaderData(particleTypes, shaderParticles, shaderParticleInteractions);
    }

    private ShaderParticleType[] GenerateParticleTypes()
    {
        List<ShaderParticleType> particleTypes = new();
        List<ParticleSpawnSettings> particleSpawnSettings = _spawnSettings.particleNum;

        for (int i = 0; i < particleSpawnSettings.Count; i++)
        {
            ParticleSpawnSettings particleSpawn = particleSpawnSettings[i];
            ParticleSO particleSO = particleSpawn.particle;
            int particleTypeIndex = GetShaderParticleType(particleSO);
            ParticleSettingsSO particleSettings = particleSO.particleSettings;
            ParticleVisual visual = new()
            {
                color = new(particleSO.color.r, particleSO.color.g, particleSO.color.b, particleSO.color.a),
                radius = particleSettings.size
            };
            particleTypes.Add(new()
            {
                dumping = particleSettings.dumping,
                visual = visual,
                interactionsStart = _interactions.Count,
                interactionsEnd = _interactions.Count + particleSO.attractions.Count
            });

            SpawnParticles(particleSpawn, particleTypeIndex);
            GenerateAttraction(particleSO);
        }

        return particleTypes.ToArray();
    }

    private void SpawnParticles(ParticleSpawnSettings spawnSettings, int typeIndex)
    {
        for (int i = 0; i < spawnSettings.num; i++)
        {
            ShaderParticle shaderParticle = new()
            {
                position = GetRandomSpawnPos(),
                velocity = new(GetRandomNormalizedVec2() * spawnSettings.spawnVelocity, 0),
                type = typeIndex
            };

            _spawnedParticles.Add(shaderParticle);
        }
    }

    private void GenerateAttraction(ParticleSO particleSO)
    {
        foreach (ParticleAttraction attraction in particleSO.attractions)
        {
            ParticleSettingsSO particleSettings = particleSO.particleSettings;
            int type = GetShaderParticleType(attraction.particle);
            _interactions.Add(new()
            {
                type = type,
                force = attraction.force,
                pushDst = particleSettings.pushDistance,
                pushForce = particleSettings.pushForce
            });
        }
    }

    private Vector3 GetRandomSpawnPos()
    {
        Bounds playAreaBounds = _playArea.bounds;
        Vector3 boundsMin = playAreaBounds.min;
        Vector3 boundsMax = playAreaBounds.max;

        float x = Random.Range(boundsMin.x, boundsMax.x);
        float y = Random.Range(boundsMin.y, boundsMax.y);
        float z = Random.Range(boundsMin.z, boundsMax.z);

        return new(x, y, z);
    }

    private Vector2 GetRandomNormalizedVec2()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);

        Vector2 vec = new(x, y);
        return vec.normalized;
    }

    private int GetShaderParticleType(ParticleSO particleSO)
    {
        if (_shaderParticleTypes.TryGetValue(particleSO, out int i))
            return i;

        i = _shaderParticleTypes.Count;
        _shaderParticleTypes.Add(particleSO, i);
        return i;
    }
}
