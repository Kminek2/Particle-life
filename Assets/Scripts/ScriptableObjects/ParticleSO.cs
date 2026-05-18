using UnityEngine;

[CreateAssetMenu(fileName = "ParticleSO", menuName = "Scriptable Objects/ParticleSO")]
public class ParticleSO : ScriptableObject
{
    [Header("Global Settings")]
    public ParticleSettingsSO particleSettings;
    [Header("Visuals")]
    public Color color;
}
