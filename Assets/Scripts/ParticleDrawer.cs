using UnityEngine;
using UnityEngine.UI;

public class ParticleDrawer : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    [SerializeField] RawImage preview;
    [SerializeField] Vector2Int resolution;

    RenderTexture rt;

    int kernel;

    void Start()
    {
        kernel = computeShader.FindKernel("CSMain");

        rt = new(resolution.x, resolution.y, 0);
        rt.enableRandomWrite = true;
        rt.Create();

        preview.texture = rt;

        ShaderParticleInteractionsManager interactionsManager = ShaderParticleInteractionsManager.Instance;

        computeShader.SetTexture(kernel, "Result", rt);
        computeShader.SetBuffer(kernel, "ParticlesTypes", interactionsManager.ParticleTypesBuffer);
        computeShader.SetBuffer(kernel, "Particles", interactionsManager.ParticleBuffer);
        computeShader.SetInt("ParticlesLength", interactionsManager.ParticleBufferSize);
    }

    void LateUpdate()
    {
        Vector3 pixelStart = preview.transform.position - preview.transform.lossyScale / 2;
        Vector2 pixelSize = new(preview.transform.lossyScale.x / rt.width, preview.transform.lossyScale.y / rt.height);
        computeShader.SetVector("pixelStart", pixelStart);
        computeShader.SetVector("pixelSize", pixelSize);

        computeShader.Dispatch(kernel, Mathf.CeilToInt(rt.width / 8.0f), Mathf.CeilToInt(rt.height / 8.0f), 1);
    }

}
