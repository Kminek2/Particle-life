using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class ShaderParticleInteractionsManager : MonoBehaviour
{
    private Dictionary<ParticleSO, List<Particle>> _particles;
    [SerializeField] private Collider2D _playArea;
    private Bounds _playBounds;

    public static ShaderParticleInteractionsManager Instance
    {
        get; private set;
    }

    #region Shader Vars

    [SerializeField] ComputeShader _computeShader;
    private ComputeBuffer _particleTypesBuffer;
    private ComputeBuffer _particleInteractionsBuffer;
    private ComputeBuffer _particlesBuffer;

    private Dictionary<ParticleSO, int> _shaderParticleTypes = new();

    struct ShaderParticleType
    {
        public float dumping;
        int interactionsStart;
        int interactionsEnd;
    };

    private ShaderParticleType[] _shadersParticlesTypes;

    struct ShaderParticle
    {
        public float3 position;
        public float3 velocity;
        public int type;
    };

    private ShaderParticle[] _shadersParticles;

    struct ShaderParticleInteractions
    {
        public int type;
        public float force;
        public float pushDst;
        public float pushForce;
    };

    private ShaderParticleInteractions[] _shadersParticleInteractions;

    int _shaderKernel;

    #endregion

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

        _playBounds = _playArea.bounds;
    }

    private void Start()
    {
        InitShaderVariables();
        InitBuffers();
    }

    void InitShaderVariables()
    {
        _shaderKernel = _computeShader.FindKernel("CSMain");
        _shadersParticlesTypes = GenerateShaderParticleTypes();
        _shadersParticles = GenerateShaderParticles();
        _shadersParticleInteractions = GenerateShaderParticleInteractions();
    }

    void InitBuffers()
    {
        InitBuffer(ref _particleTypesBuffer, _shadersParticlesTypes);
        InitBuffer(ref _particleInteractionsBuffer, _shadersParticleInteractions);
        InitBuffer(ref _particlesBuffer, _shadersParticles);

        _particleTypesBuffer.SetData(_shadersParticlesTypes);
        _particleInteractionsBuffer.SetData(_shadersParticleInteractions);

        _computeShader.SetBuffer(_shaderKernel, "ParticlesTypes", _particleTypesBuffer);
        _computeShader.SetBuffer(_shaderKernel, "ParticlesInteractions", _particleInteractionsBuffer);


        _computeShader.SetVector("PlayAreaMin", _playBounds.min);
        _computeShader.SetVector("PlayAreaSize", _playBounds.max);

        _particlesBuffer.SetData(_shadersParticles);
        _computeShader.SetBuffer(_shaderKernel, "Particles", _particlesBuffer);
        _computeShader.SetInt("ParticlesLength", _shadersParticles.Count());
    }

    void InitBuffer<T>(ref ComputeBuffer buffer, T[] data)
    {
        buffer = new ComputeBuffer(
            data.Count(),
            Unsafe.SizeOf<T>()
        );
    }

    #endregion

    #region Update

    void Update()
    {
        DispatchComputes();
        UpdateParticles();
    }

    void DispatchComputes()
    {
        _computeShader.SetFloat("DeltaTime", Time.deltaTime);

        _computeShader.Dispatch(_shaderKernel, Mathf.CeilToInt(_shadersParticles.Count() / 64.0f), 1, 1);

        _particlesBuffer.GetData(_shadersParticles);
    }

    void UpdateParticles()
    {
        int i = 0;
        foreach (KeyValuePair<ParticleSO, List<Particle>> kvpParticle in _particles)
        {
            foreach (Particle particle in kvpParticle.Value)
            {
                ShaderParticle shaderParticle = _shadersParticles[i];
                particle.Velocity = shaderParticle.velocity;
                particle.transform.position = shaderParticle.position;
                i++;
            }
        }
    }

    #endregion

    #region Destruction

    void OnDestroy()
    {
        _particlesBuffer?.Release();
        _particleInteractionsBuffer?.Release();
        _particleTypesBuffer?.Release();
    }

    #endregion

    #region Shader Data Generation

    private ShaderParticleType[] GenerateShaderParticleTypes()
    {
        Dictionary<ParticleSO, List<Particle>>.KeyCollection particleTypesKeys = _particles.Keys;
        ShaderParticleType[] particleTypes = new ShaderParticleType[particleTypesKeys.Count];


        for (int i = 0; i < particleTypesKeys.Count; i++)
        {
            GetShaderParticleType(particleTypesKeys.ElementAt(i));
            ParticleSettingsSO particleSettings = particleTypesKeys.ElementAt(i).particleSettings;
            particleTypes[i] = new()
            {
                dumping = particleSettings.dumping
            };
        }

        return particleTypes;
    }

    private int GetShaderParticleType(ParticleSO particleSO)
    {
        if (_shaderParticleTypes.TryGetValue(particleSO, out int i))
            return i;

        i = _shaderParticleTypes.Count;
        _shaderParticleTypes.Add(particleSO, i);
        return i;
    }

    private ShaderParticleInteractions[] GenerateShaderParticleInteractions()
    {
        List<ShaderParticleInteractions> particleInteractions = new();

        Dictionary<ParticleSO, List<Particle>>.KeyCollection particleTypes = _particles.Keys;

        int particleIndex = 0;
        int interactionIndex = 0;
        foreach (ParticleSO particleSO in particleTypes)
        {
            foreach (ParticleAttraction attraction in particleSO.attractions)
            {
                ParticleSettingsSO particleSettings = particleSO.particleSettings;
                int type = GetTypeIndexFromSO(attraction.particle);
                particleInteractions.Add(new()
                {
                    type = type,
                    force = attraction.force,
                    pushDst = particleSettings.pushDistance,
                    pushForce = particleSettings.pushForce
                });
                interactionIndex++;
            }
            particleIndex++;
        }

        return particleInteractions.ToArray();
    }

    private int GetTypeIndexFromSO(ParticleSO particle)
    {
        Dictionary<ParticleSO, List<Particle>>.KeyCollection particleTypes = _particles.Keys;

        int i = 0;
        foreach (ParticleSO particleSO in particleTypes)
            if (particleSO == particle)
                return i;

        throw new System.Exception("IDK :D");
    }

    private ShaderParticle[] GenerateShaderParticles()
    {
        List<ShaderParticle> shaderParticles = new();
        foreach (KeyValuePair<ParticleSO, List<Particle>> kvpParticle in _particles)
        {
            ParticleSO particleSO = kvpParticle.Key;
            int particleType = GetTypeIndexFromSO(particleSO);

            foreach (Particle particle in kvpParticle.Value)
            {
                ShaderParticle shaderParticle = new()
                {
                    position = particle.transform.position,
                    velocity = particle.Velocity,
                    type = particleType
                };

                shaderParticles.Add(shaderParticle);
            }
        }

        return shaderParticles.ToArray();
    }

    #endregion
}
