using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Missile : MonoBehaviour
{

    #region --------------------    Public Properties

    /// <summary>
    /// Returns the damage for the missle
    /// </summary>
    public float damage => _damage;

    #endregion

    #region --------------------    Public Methods

    /// <summary>
    /// Fires the missle
    /// </summary>
    /// <param name="_pTag"></param>
    /// <param name="_pPosition"></param>
    /// <param name="_pDirection"></param>
    /// <param name="_pTarget"></param>
    public void Fire(float _pDamage, string _pTag, Vector3 _pPosition, Quaternion _pDirection)
    {
        _damage = _pDamage;
        tag = _pTag;
        transform.position = _pPosition;
        transform.rotation = _pDirection;
        gameObject.SetActive(true);
        _renderer.sharedMaterial = tag == "PlayerTeam" ? _friendly : _enemy;
    }

    #endregion

    #region --------------------    Private Fields

    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private ParticleSystem _trail = null;
    [SerializeField] private ParticleSystem _explosion = null;
    //[SerializeField] private AudioClip _trailSound = null;
    //[SerializeField] private AudioClip _explosionSound = null;
    private float _life = 2f;
    private MeshRenderer _renderer = null;
    [SerializeField] private Collider _explosionTrigger = null;
    [SerializeField] private Material _friendly = null;
    [SerializeField] private Material _enemy = null;

    #endregion

    #region --------------------    Private Methods

    /// <summary>
    /// Gets the mesh renderer
    /// </summary>
    private void Awake() => _renderer = GetComponentInChildren<MeshRenderer>();

    /// <summary>
    /// Enables the particles
    /// </summary>
    private void OnEnable()
    {
        _renderer.enabled = true;
        _life = 2f;
        _explosionTrigger.enabled = false;
        _trail.Play();
        //GameManager.instance.sfx.PlayOneShot(_trailSound);
    }

    /// <summary>
    /// Flies towards target 
    /// </summary>
    private void Update()
    {
        if (GameManager.state != GameManager.GameState.Gameplay) return;
        if (_life <= 0) return;
        transform.position += transform.forward * Time.deltaTime * _speed;
        _life -= Time.deltaTime;
        if (_life <= 0)
        {
            _Explode();
        }
    }

    /// <summary>
    /// Explode on contact with enemy
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Untagged" || other.CompareTag(tag)) return;
        if (_life <= 0f) return;
        _Explode();
    }

    /// <summary>
    /// Explodes the missile
    /// </summary>
    private void _Explode()
    {
        _life = 0f;
        _renderer.enabled = false;
        _explosionTrigger.enabled = true;
        //GameManager.instance.sfx.PlayOneShot(_explosionSound);
        _trail.Stop();
        _explosion.Play();
        transform.DOMove(transform.position, 0.5f)
            .OnComplete(() =>
            {
                _trail.Clear();
                _explosion.Clear();
                gameObject.SetActive(false);
            });
    }

    #endregion

}