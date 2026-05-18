using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Particle : MonoBehaviour
{
    #region Private variables
    //Below values are serialized for debugging
    [SerializeField] private ParticleSO _particle;
    [SerializeField] private Vector3 _velocity;
    private SpriteRenderer _spriteRenderer;
    private static Bounds _playAreaBounds;
    #endregion

    #region Getters and Setters
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

    public static Bounds PlayAreaBounds
    {
        get { return _playAreaBounds; }
        set { _playAreaBounds = value; }
    }
    #endregion

    #region Setup functions
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // For serialization
        if (_particle != null)
            SetSprite();
    }

    private void SetSprite()
    {
        //Global settings
        ParticleSettingsSO settings = _particle.particleSettings;
        _spriteRenderer.sprite = settings.sprite;
        _spriteRenderer.transform.localScale = Vector3.one * settings.size;

        //Private settings
        _spriteRenderer.color = _particle.color;
    }
    #endregion

    private void Update()
    {
        Move();
        BoundsTeleport();
    }

    #region Movement functions

    private void Move()
    {
        transform.position += _velocity * Time.deltaTime;
    }

    private void BoundsTeleport()
    {
        Vector3 pos = transform.position;
        Vector3 playAreaMin = PlayAreaBounds.min;
        Vector3 playAreaSize = PlayAreaBounds.size;

        Vector3 playAreaAlignedPos = pos - playAreaMin;
        Vector3 playAreaPositivePos = playAreaAlignedPos + playAreaSize;

        //New pos
        float x = playAreaSize.x == 0 ? 0 : playAreaPositivePos.x % playAreaSize.x;
        float y = playAreaSize.y == 0 ? 0 : playAreaPositivePos.y % playAreaSize.y;
        float z = playAreaSize.z == 0 ? 0 : playAreaPositivePos.z % playAreaSize.z;

        Vector3 areaPos = new(x, y, z);
        transform.position = areaPos + playAreaMin;
    }

    #endregion
}
