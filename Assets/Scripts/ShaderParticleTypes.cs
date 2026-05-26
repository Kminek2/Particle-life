using System.Runtime.InteropServices;
using Unity.Mathematics;

public struct ShaderParticleType
{
    public float dumping;
    int interactionsStart;
    int interactionsEnd;
    public ParticleVisual visual;
};

public struct ShaderParticle
{
    public float3 position;
    public float3 velocity;
    public int type;
};

public struct ShaderParticleInteractions
{
    public int type;
    public float force;
    public float pushDst;
    public float pushForce;
};

public struct ParticleVisual
{
    public float4 color;
    public float radius;
};
