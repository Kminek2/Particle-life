#ifndef COMMON_PARTICLE
#define COMMON_PARTICLE


struct Particle
{
    float3 position;
    float3 velocity;
    int type;
    int chunkId;
};

struct ParticleVisual{
    float4 color;
    float radius;
};

struct ParticleType
{
    float dumping;
    int interactionsStart;
    int interactionsEnd;
    int changesStart;
    int changesEnd;
    ParticleVisual visual;
};

struct ParticleInteractions
{
    int type;
    float force;
    float forceDst;
    float pushDst;
    float pushForce;
};

struct Chunk
{
    float3 pos;
    int particlesStart;
    int particlesNum;
};

struct ParticleChanges
{
    int type;
    float dist;
    int typeTo;
};

#endif