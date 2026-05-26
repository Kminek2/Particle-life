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
    [SerializeField] ParticleChunker chunker;
    [SerializeField] bool CPUChunking;
    private ComputeBuffer _particleTypesBuffer;
    private ComputeBuffer _particleInteractionsBuffer;
    private ComputeBuffer _particlesBuffer;

    private int _shaderParticleCount;

    int _shaderKernel;

    #endregion

    #region Public
    public Dictionary<ParticleSO, List<Particle>> Particles
    {
        get { return _particles; }
        set { _particles = value; }
    }

    public ComputeBuffer ParticleBuffer
    {
        get { return _particlesBuffer; }
        set { _particlesBuffer = value; }
    }

    public int ParticleBufferSize
    {
        get { return _shaderParticleCount; }
    }

    public ComputeBuffer ParticleTypesBuffer
    {
        get { return _particleTypesBuffer; }
        set { _particleTypesBuffer = value; }
    }

    public Bounds PlayArea
    {
        get { return _playArea.bounds; }
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
        chunker.SetupShader();
    }

    public void SetShaderData(ShaderParticleType[] shadersParticlesTypes, ShaderParticle[] shadersParticles, ShaderParticleInteractions[] shadersParticleInteractions)
    {
        InitBuffer(ref _particleTypesBuffer, shadersParticlesTypes);
        InitBuffer(ref _particleInteractionsBuffer, shadersParticleInteractions);
        InitBuffer(ref _particlesBuffer, shadersParticles);

        _particleTypesBuffer.SetData(shadersParticlesTypes);
        _particleInteractionsBuffer.SetData(shadersParticleInteractions);

        _computeShader.SetBuffer(_shaderKernel, "ParticlesTypes", _particleTypesBuffer);
        _computeShader.SetBuffer(_shaderKernel, "ParticlesInteractions", _particleInteractionsBuffer);


        _computeShader.SetVector("PlayAreaMin", _playBounds.min);
        _computeShader.SetVector("PlayAreaSize", _playBounds.size);

        _shaderParticleCount = shadersParticles.Count();

        _particlesBuffer.SetData(shadersParticles);
        _computeShader.SetBuffer(_shaderKernel, "Particles", _particlesBuffer);
        _computeShader.SetInt("ParticlesLength", _shaderParticleCount);
    }

    void InitShaderVariables()
    {
        _shaderKernel = _computeShader.FindKernel("CSMain");
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
        if (Time.frameCount < 10)
            return;

        if (!CPUChunking)
            chunker.Chunk();
        else
            chunker.CPUChunk();
    }

    void DispatchComputes()
    {
        _computeShader.SetFloat("DeltaTime", Time.deltaTime);
        _computeShader.SetVector("chunkData", new Vector4(chunker.ChunkSize.x, chunker.ChunkSize.y, chunker.ChunkNum.x, chunker.ChunkNum.y));

        _computeShader.Dispatch(_shaderKernel, Mathf.CeilToInt(_shaderParticleCount / 64.0f), 1, 1);
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

}
