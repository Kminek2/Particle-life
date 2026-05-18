using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSettingsSO", menuName = "Scriptable Objects/SpawnSettingsSO")]
public class SpawnSettingsSO : ScriptableObject
{
    public List<ParticleSpawnSettings> particleNum;
}

[Serializable]
public struct ParticleSpawnSettings
{
    public ParticleSO particle;
    public int num;
    public float spawnVelocity;
}