using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Particle : MonoBehaviour
{
    //Below values are serialized for debugging
    [SerializeField] private ParticleSO _particle;
    [SerializeField] private Vector3 _velocity;
    private SpriteRenderer spriteRenderer;

    public ParticleSO ParticleSO
    {
        get { return _particle; }
        set
        {
            _particle = value;
            SetSprite();
        }
    }

    public Vector3 Velocity
    {
        get { return _velocity; }
        set { _velocity = value; }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // For serialization
        if (_particle != null)
            SetSprite();
    }

    private void Update()
    {
        Move();
    }

    private void SetSprite()
    {
        //Global settings
        ParticleSettingsSO settings = _particle.particleSettings;
        spriteRenderer.sprite = settings.sprite;
        spriteRenderer.transform.localScale = Vector3.one * settings.size;

        //Private settings
        spriteRenderer.color = _particle.color;
    }

    private void Move()
    {
        transform.position += _velocity * Time.deltaTime;
    }
}
