#ifndef COMMON_PARTICLE
#define COMMON_PARTICLE


struct Particle
{
    float3 position;
    float3 velocity;
    int type;
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
    ParticleVisual visual;
};

struct ParticleInteractions
{
    int type;
    float force;
    float pushDst;
    float pushForce;
};

#endif