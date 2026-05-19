using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleSO", menuName = "Scriptable Objects/ParticleSO")]
public class ParticleSO : ScriptableObject
{
    [Header("Interactions")]
    public List<ParticleAttraction> attractions;
    [Header("Global Settings")]
    public ParticleSettingsSO particleSettings;
    [Header("Visuals")]
    public Color color;
}

[Serializable]
public struct ParticleAttraction
{
    public ParticleSO particle;
    public float force;
}