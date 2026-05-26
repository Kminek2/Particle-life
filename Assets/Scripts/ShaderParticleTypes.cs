using System.Runtime.InteropServices;
using Unity.Mathematics;

struct ShaderParticleType
{
    public float dumping;
    int interactionsStart;
    int interactionsEnd;
    public ParticleVisual visual;
};

struct ShaderParticle
{
    public float3 position;
    public float3 velocity;
    public int type;
};

struct ShaderParticleInteractions
{
    public int type;
    public float force;
    public float pushDst;
    public float pushForce;
};

struct ParticleVisual
{
    public float4 color;
    public float radius;
};
