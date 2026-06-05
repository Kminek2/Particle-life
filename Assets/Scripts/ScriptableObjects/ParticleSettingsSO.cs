using UnityEngine;

[CreateAssetMenu(fileName = "ParticleSettingsSO", menuName = "Scriptable Objects/ParticleSettingsSO")]
public class ParticleSettingsSO : ScriptableObject
{
    public Sprite sprite;

    public float size;

    public float dumping;
    public float pushDistance;
    public float pushDstSmoothing;
    public float pushForce;
}
