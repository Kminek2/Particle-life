using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Particle : MonoBehaviour
{
    [SerializeField] private ParticleSO _particle;
    [SerializeField] private Vector3 velocity;
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
        transform.position += velocity * Time.deltaTime;
    }
}
