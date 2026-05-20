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
        VelocityDumping();
    }

    #region Movement functions

    private void VelocityDumping()
    {
        float dumping = _particle.particleSettings.dumping;
        float frameDumping = dumping * Time.deltaTime;
        float velocityPercentLeft = 1 - frameDumping;
        _velocity *= velocityPercentLeft;
    }

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

    #region Interaction functions
    public void Attract(Particle attractionParticle, ParticleAttraction attractionRule)
    {
        Vector3 attraction = CalculateAttraction(attractionParticle, attractionRule);
        _velocity += attraction * Time.deltaTime;
    }

    public Vector3 CalculateAttraction(Particle attractionParticle, ParticleAttraction attractionRule)
    {
        Vector3 pos = transform.position;
        Vector3 otherPos = attractionParticle.transform.position;
        Vector3 insidePosDiff = otherPos - pos;
        Vector3 posDiff = GetBoundPosDiff(insidePosDiff);
        Vector3 otherDir = posDiff.normalized;
        float dist = posDiff.magnitude;

        float attraction = GetAttraction(attractionRule, dist);

        return otherDir * attraction;
    }

    private float GetAttraction(ParticleAttraction attractionRule, float dist)
    {
        float attractionForce = attractionRule.force;

        // To keep particles enough away to avoid near infinite speeds
        float pushDistance = _particle.particleSettings.pushDistance;
        if (dist < _particle.particleSettings.pushDistance)
            attractionForce = -Mathf.Abs(attractionForce) - _particle.particleSettings.pushForce;

        float attraction = attractionForce / (dist * dist);

        return attraction;
    }

    /// <summary>
    /// Returns the Vec3 wrapped around the bound. Used for making the distance closest on wrapped worlds. (Where bounds mean teleport to the other side) 
    /// </summary>
    /// <param name="dist">The distance to be wrapped</param>
    /// <returns></returns>
    private Vector3 GetBoundPosDiff(Vector3 dist)
    {
        Vector3 boundsExtends = _playAreaBounds.extents;

        float x = GetBoundVariable(dist.x, boundsExtends.x);
        float y = GetBoundVariable(dist.y, boundsExtends.y);
        float z = GetBoundVariable(dist.z, boundsExtends.z);

        Vector3 aroundDist = new(x, y, z);

        return aroundDist;
    }

    /// <summary>
    /// Returns the variable wrapped around the bound. Used for making the distance closest on wrapped worlds. (Where bounds mean teleport to the other side) 
    /// </summary>
    /// <param name="x">distance for wrapping</param>
    /// <param name="extends">bounds extends</param>
    /// <returns></returns>
    private float GetBoundVariable(float x, float extends)
    {
        if (Mathf.Abs(x) > extends)
        {
            if (x < 0)
                x += extends * 2;
            else
                x -= extends * 2;
        }

        return x;
    }
    #endregion
}
