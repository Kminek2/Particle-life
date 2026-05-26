using UnityEngine;
using UnityEngine.UI;

public class ParticleDrawer : MonoBehaviour
{
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] RawImage _preview;
    [SerializeField] Vector2Int _resolution;
    [SerializeField] ParticleChunker _chunker;

    RenderTexture _rt;

    int _kernel;

    Vector3 _lastPixelStart = new();
    Vector2 _lastPixelSize = new();

    void Start()
    {
        _kernel = _computeShader.FindKernel("CSMain");

        _rt = new(_resolution.x, _resolution.y, 0);
        _rt.enableRandomWrite = true;
        _rt.Create();

        _preview.texture = _rt;

        ShaderParticleInteractionsManager interactionsManager = ShaderParticleInteractionsManager.Instance;

        _computeShader.SetTexture(_kernel, "Result", _rt);
        _computeShader.SetBuffer(_kernel, "ParticlesTypes", interactionsManager.ParticleTypesBuffer);
        _computeShader.SetBuffer(_kernel, "Particles", interactionsManager.ParticleBuffer);
        _computeShader.SetInt("ParticlesLength", interactionsManager.ParticleBufferSize);

        _computeShader.SetBuffer(_kernel, "Chunks", _chunker.ChunkingBuffer);
        _computeShader.SetVector("chunkData", new Vector4(_chunker.ChunkSize.x, _chunker.ChunkSize.y, _chunker.ChunkNum.x, _chunker.ChunkNum.y));
        _computeShader.SetVector("PlayAreaMin", ShaderParticleInteractionsManager.Instance.PlayArea.min);
        UpdatePixelVars();
    }

    void LateUpdate()
    {
        UpdatePixelVars();

        _computeShader.Dispatch(_kernel, Mathf.CeilToInt(_rt.width / 8.0f), Mathf.CeilToInt(_rt.height / 8.0f), 1);
    }

    private void UpdatePixelVars()
    {
        Vector3 pixelStart = _preview.transform.position - _preview.transform.lossyScale / 2;
        Vector2 pixelSize = new(_preview.transform.lossyScale.x / _rt.width, _preview.transform.lossyScale.y / _rt.height);

        if (pixelStart != _lastPixelStart)
            _computeShader.SetVector("pixelStart", pixelStart);
        if (pixelSize != _lastPixelSize)
            _computeShader.SetVector("pixelSize", pixelSize);

        _lastPixelStart = pixelStart;
        _lastPixelSize = pixelSize;
    }

}
