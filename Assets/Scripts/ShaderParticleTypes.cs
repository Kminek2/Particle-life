using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

public struct ShaderParticleType
{
    public float dumping;
    public int interactionsStart;
    public int interactionsEnd;
    public int changesStart;
    public int changesEnd;
    public ParticleVisual visual;
};

public struct ShaderParticle
{
    public float3 position;
    public float3 velocity;
    public int type;
    public int chunkId;
};

public struct ShaderParticleInteractions
{
    public int type;
    public float force;
    public float forceDst;
    public float pushDst;
    public float pushDstSmoothing;
    public float pushForce;
};

public struct ParticleVisual
{
    public float4 color;
    public float radius;
};

public struct Chunks
{
    public float3 pos;
    public int particlesStart;
    public int particlesNum;
}

public struct ShaderParticleChanges
{
    public int type;
    public float dist;
    public int typeTo;
}
