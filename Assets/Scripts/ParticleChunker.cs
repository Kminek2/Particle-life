using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class ParticleChunker : MonoBehaviour
{
    [Header("Shader")]
    [SerializeField] private Collider2D _playArea;
    [SerializeField] ComputeShader _sortingShader;
    [SerializeField] ComputeShader _chunkingShader;
    [SerializeField] int _sortingPasses;
    [SerializeField] private Vector2 _chunkNum;

    private ComputeBuffer _chunkBuffer;

    Chunks[] _chunksDefault;

    int _sortingShaderKernel;
    int _chunkingShaderKernel;

    ShaderParticle[] _particles;

    public Vector2 ChunkSize
    {
        get { return new(_playArea.bounds.size.x / _chunkNum.x, _playArea.bounds.size.y / _chunkNum.y); }
        set { _chunkNum = new(_playArea.bounds.size.x / value.x, _playArea.bounds.size.y / value.y); }
    }

    public Vector2 ChunkNum
    {
        get { return _chunkNum; }
        set { _chunkNum = value; }
    }

    public ComputeBuffer ChunkingBuffer
    {
        get { return _chunkBuffer; }
    }

    void Awake()
    {
        InitVars();
        InitChunks();
    }

    void InitChunks()
    {
        _chunksDefault = new Chunks[(int)_chunkNum.x * (int)_chunkNum.y];

        for (int i = 0; i < _chunkNum.x * _chunkNum.y; i++)
        {
            _chunksDefault[i].particlesNum = 0;
            _chunksDefault[i].particlesStart = -1;
            _chunksDefault[i].pos = GetChunkPos(i);
        }

        _chunkBuffer = new(_chunksDefault.Count(), Unsafe.SizeOf<Chunks>());
        _chunkBuffer.SetData(_chunksDefault);
    }

    float3 GetChunkPos(int id)
    {
        float y = id / ChunkNum.x;
        float x = id % ChunkNum.x;

        float posX = _playArea.bounds.min.x + x * ChunkSize.x + ChunkSize.x / 2;
        float posY = _playArea.bounds.min.y + y * ChunkSize.y + ChunkSize.y / 2;
        float posZ = 0;

        return new float3(posX, posY, posZ);
    }

    void InitVars()
    {
        _sortingShaderKernel = _sortingShader.FindKernel("CSMain");
        _chunkingShaderKernel = _chunkingShader.FindKernel("CSMain");
    }

    public void SetupShader()
    {
        _sortingShader.SetBuffer(_sortingShaderKernel, "Particles", ShaderParticleInteractionsManager.Instance.ParticleBuffer);
        _sortingShader.SetInt("ParticlesLength", ShaderParticleInteractionsManager.Instance.ParticleBufferSize);

        _chunkingShader.SetBuffer(_chunkingShaderKernel, "Particles", ShaderParticleInteractionsManager.Instance.ParticleBuffer);
        _chunkingShader.SetInt("ParticlesLength", ShaderParticleInteractionsManager.Instance.ParticleBufferSize);

        _chunkingShader.SetBuffer(_chunkingShaderKernel, "Chunks", _chunkBuffer);

        _particles = new ShaderParticle[ShaderParticleInteractionsManager.Instance.ParticleBufferSize];
    }

    public void Chunk()
    {
        ShaderSort();
        ShaderChunk();
    }

    private void ShaderSort()
    {
        for (int i = 0; i < _sortingPasses; i++)
        {
            _sortingShader.SetBool("odd", (Time.frameCount + i) % 2 == 0);
            _sortingShader.Dispatch(_sortingShaderKernel, Mathf.CeilToInt(ShaderParticleInteractionsManager.Instance.ParticleBufferSize / 64.0f) / 2, 1, 1);
        }
    }

    private void ShaderChunk()
    {
        _chunkBuffer.SetData(_chunksDefault);

        _chunkingShader.Dispatch(_chunkingShaderKernel, 1, 1, 1);
    }

    public void CPUChunk()
    {
        ShaderParticleInteractionsManager.Instance.ParticleBuffer.GetData(_particles);
        List<ShaderParticle> particleList = _particles.ToList();
        particleList.Sort((a, b) => a.chunkId - b.chunkId);
        ShaderParticleInteractionsManager.Instance.ParticleBuffer.SetData(particleList.ToArray());

        CPUChunk(particleList.ToArray());
    }

    private void CPUChunk(ShaderParticle[] particles)
    {
        for (int i = 0; i < _chunksDefault.Count(); i++)
        {
            Chunks chunk = _chunksDefault[i];
            chunk.particlesNum = 0;
            chunk.particlesStart = -1;
            _chunksDefault[i] = chunk;
        }

        for (int i = 0; i < particles.Count(); i++)
        {
            ShaderParticle particle = particles[i];
            if (_chunksDefault[particle.chunkId].particlesStart == -1)
                _chunksDefault[particle.chunkId].particlesStart = i;
            _chunksDefault[particle.chunkId].particlesNum++;
        }

        _chunkBuffer.SetData(_chunksDefault);
    }

    #region Destruction

    void OnDestroy()
    {
        _chunkBuffer?.Release();
    }

    #endregion
}
