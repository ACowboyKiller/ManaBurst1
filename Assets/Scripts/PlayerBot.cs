using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using Cinemachine;

public class PlayerBot : MonoBehaviour, iSpawnable, iCombatable
{

    #region --------------------    Public Enumerations



    #endregion

    #region --------------------    Public Events



    #endregion

    #region --------------------    Public Properties

    /// <summary>
    /// Stores whether or not the minion is alive
    /// </summary>
    public bool isAlive { get; set; } = true;

    /// <summary>
    /// Returns the player's max health
    /// </summary>
    public float maxHealth => _maxHealth;

    /// <summary>
    /// Stores the current health of the minion
    /// </summary>
    public float health { get; set; } = 0f;

    /// <summary>
    /// Stores the lane of the minion
    /// </summary>
    public GameManager.Lane lane { get; set; } = GameManager.Lane.Mid;

    /// <summary>
    /// The attackers currently targetting the minion
    /// </summary>
    public List<iCombatable> attackers { get; set; } = new List<iCombatable>();

    /// <summary>
    /// Returns whether or not the minion is ready to attack
    /// </summary>
    public bool isReadyToAttack => _attackTimer == _attackCooldown;

    /// <summary>
    /// Returns whether or not the player can be damagaed
    /// </summary>
    public bool canBeDamaged => _damageCooldown == 0f;

    /// <summary>
    /// Returns the ground pound damage
    /// </summary>
    public float groundPoundDamage => _groundPoundDamage;

    #endregion

    #region --------------------    Public Methods

    /// <summary>
    /// Spawns the player bot in the provided position
    /// </summary>
    /// <param name="_pTransform"></param>
    public void Spawn(Transform _pTransform, GameManager.Lane _pLane)
    {
        _agent.enabled = false;
        transform.position = _pTransform.position;
        transform.rotation = _pTransform.rotation;
        health = _maxHealth;
        isAlive = true;
        gameObject.SetActive(true);
        //  TODO:   Play some animation
        _agent.enabled = true;
        _leftTrack = KeyCode.Q;
        _rightTrack = KeyCode.P;
        _reverse = KeyCode.W;
        _shoot = KeyCode.O;
        _leftTrackImage.rectTransform.anchoredPosition = _keyImagePos[_leftTrack];
        _reverseImage.rectTransform.anchoredPosition = _keyImagePos[_reverse];
        _shootImage.rectTransform.anchoredPosition = _keyImagePos[_shoot];
        _rightTrackImage.rectTransform.anchoredPosition = _keyImagePos[_rightTrack];
    }

    /// <summary>
    /// Returns the combatant's game object
    /// </summary>
    /// <returns></returns>
    public GameObject GetGameObject() => gameObject;

    /// <summary>
    /// Returns whether or not the combatant is alive
    /// </summary>
    /// <returns></returns>
    public bool IsAlive() => isAlive;

    /// <summary>
    /// Returns whether or not the provided game object is an enemy object
    /// </summary>
    /// <param name="_pTarget"></param>
    /// <returns></returns>
    public bool IsEnemy(GameObject _pTarget) => (!_pTarget.CompareTag(tag) && _pTarget.tag != "Untagged" && _pTarget.tag != "Ground" && _pTarget.name != "Missile");

    /// <summary>
    /// Targets the provided target
    /// </summary>
    public void GainTarget(GameObject _pTarget) { }

    /// <summary>
    /// Removes the target if the provided object is the target
    /// </summary>
    /// <param name="_pTarget"></param>
    public void LoseTarget(GameObject _pTarget) { }

    /// <summary>
    /// Returns whether or not the combatant has a target
    /// </summary>
    /// <returns></returns>
    public bool HasTarget() => false;

    /// <summary>
    /// Sets the combatant as the target for another combatant
    /// </summary>
    /// <param name="_pAttacker"></param>
    public void BecomeTargetted(iCombatable _pAttacker)
    {
        if (!isAlive) return;
        attackers.Remove(_pAttacker);
        attackers.Add(_pAttacker);
    }

    /// <summary>
    /// Attacks the target gameobject
    /// </summary>
    /// <param name="_pTarget"></param>
    public void Attack(GameObject _pTarget)
    {
        if (!isAlive) return;
        _attackTimer = 0f;
        //  TODO:   Play some animation
        Missile _missile = _missilePrefab.PoolInstantiate().GetComponent<Missile>();
        _missile.Fire(_damage, tag, _missileLauncher.position, _missileLauncher.rotation);
        DOTween.To(() => _dof.parameters[1].GetValue<float>(), x => _dof.parameters[1].SetValue(new FloatParameter(x, true)), 5f, 0.25f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { DOTween.To(() => _dof.parameters[1].GetValue<float>(), y => _dof.parameters[1].SetValue(new FloatParameter(y, true)), 7f, 1.5f); });
        DOTween.To(() => _dof.parameters[2].GetValue<float>(), x => _dof.parameters[2].SetValue(new FloatParameter(x, true)), 7f, 0.25f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { DOTween.To(() => _dof.parameters[2].GetValue<float>(), y => _dof.parameters[2].SetValue(new FloatParameter(y, true)), 10f, 1.5f); });
        DOTween.To(() => _bloom.parameters[0].GetValue<float>(), x => _bloom.parameters[0].SetValue(new FloatParameter(x, true)), 0.9f, 0.25f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { DOTween.To(() => _bloom.parameters[0].GetValue<float>(), y => _bloom.parameters[0].SetValue(new FloatParameter(y, true)), 1f, 1.5f); });
    }

    /// <summary>
    /// Damages the minion by the provided amount
    /// </summary>
    /// <param name="_pAmount"></param>
    public void Damage(float _pAmount, Vector3 _pPosition)
    {
        if (!isAlive || !canBeDamaged || _isJumping) return;
        health = Mathf.Max(health - _pAmount, 0f);
        _damageCooldown = 1f;
        DOTween.To(() => _noise.m_AmplitudeGain, x => _noise.m_AmplitudeGain = x, 2f, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { DOTween.To(() => _noise.m_AmplitudeGain, y => _noise.m_AmplitudeGain = y, 0.4f, 1f); });
        DOTween.To(() => _noise.m_FrequencyGain, x => _noise.m_FrequencyGain = x, 3f, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { DOTween.To(() => _noise.m_FrequencyGain, y => _noise.m_FrequencyGain = y, 0.15f, 1f); });
        DOTween.To(() => _vignette.parameters[2].GetValue<float>(), x => _vignette.parameters[2].SetValue(new FloatParameter(x, true)), 0.4f, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { DOTween.To(() => _vignette.parameters[2].GetValue<float>(), y => _vignette.parameters[2].SetValue(new FloatParameter(y, true)), 0.2f, 1f); });
        if (health == 0f)
        {
            _healthBar.percent = 0f;
            Die();
        }
    }

    /// <summary>
    /// Kills the minion & adds it back to the object pool
    /// </summary>
    public void Die()
    {
        if (!isAlive) return;
        //  TODO:   Play death animation
        isAlive = false;
        attackers.ForEach(a => a?.LoseTarget(gameObject));
        attackers.Clear();
        gameObject.SetActive(false);
        GameManager.instance.GoToResultsLose();
    }

    #endregion

    #region --------------------    Private Fields

    [Header("Player Configurations")]
    [SerializeField] private NavMeshAgent _agent = null;
    [SerializeField] private float _speed = 4f;

    [Header("Combat Configurations")]
    [SerializeField] private float _maxHealth = 3f;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _groundPoundDamage = 10f;
    [SerializeField] private float _attackCooldown = 1.5f;
    private float _attackTimer = 0f;
    [SerializeField] private GameObject _missilePrefab = null;
    [SerializeField] private Transform _missileLauncher = null;
    private float _damageCooldown = 0f;
    [SerializeField] private CinemachineVirtualCamera _cam = null;
    private CinemachineBasicMultiChannelPerlin _noise = null;

    private bool _isJumping = false;
    [SerializeField] private Rigidbody _body = null;
    [SerializeField] private Collider _groundPound = null;
    private DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> _poundTween = null;
    [SerializeField] private ParticleSystem _poundParticles = null;

    private bool isScrambling = false;

    [Header("UI Configurations")]
    [SerializeField] private CustomProgressBar _healthBar = null;
    [SerializeField] private Volume _pp = null;
    private VolumeComponent _bloom = null;
    private VolumeComponent _vignette = null;
    private VolumeComponent _dof = null;
    [SerializeField] private CustomProgressBar _attackBar = null;
    [SerializeField] private Image _leftTrackImage = null;
    [SerializeField] private Image _reverseImage = null;
    [SerializeField] private Image _shootImage = null;
    [SerializeField] private Image _rightTrackImage = null;
    private Dictionary<KeyCode, Vector3> _keyImagePos = new Dictionary<KeyCode, Vector3>();

    private List<KeyCode> _keys = new List<KeyCode>() { KeyCode.Q, KeyCode.W, KeyCode.O, KeyCode.P };
    private KeyCode _leftTrack = KeyCode.Q;
    private KeyCode _rightTrack = KeyCode.P;
    private KeyCode _reverse = KeyCode.W;
    private KeyCode _shoot = KeyCode.O;

    #endregion

    #region --------------------    Private Methods

    /// <summary>
    /// Disable agent rotation on player
    /// </summary>
    private void Start()
    {
        health = _maxHealth;
        _agent.updateRotation = false;
        _bloom = _pp.sharedProfile.components[1];
        _vignette = _pp.sharedProfile.components[2];
        _dof = _pp.sharedProfile.components[3];
        _noise = _cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _keyImagePos.Add(KeyCode.Q, _leftTrackImage.rectTransform.anchoredPosition);
        _keyImagePos.Add(KeyCode.W, _reverseImage.rectTransform.anchoredPosition);
        _keyImagePos.Add(KeyCode.O, _shootImage.rectTransform.anchoredPosition);
        _keyImagePos.Add(KeyCode.P, _rightTrackImage.rectTransform.anchoredPosition);
    }

    /// <summary>
    /// Either follows the target & attacks if in range & cooldown is done or begins walking towards the next tower
    /// </summary>
    private void Update()
    {
        if (GameManager.state != GameManager.GameState.Gameplay) return;
        if (!isAlive) return;

        /// Update UI
        _healthBar.percent = health / _maxHealth;
        _attackBar.percent = _attackTimer / _attackCooldown;

        /// Reduce damage cooldown
        _damageCooldown = Mathf.Max(_damageCooldown - Time.deltaTime, 0f);

        bool _qDown = Input.GetKey(_leftTrack);
        bool _pDown = Input.GetKey(_rightTrack);
        bool _bothDown = _qDown && _pDown;

        /// Apply Tank Rotation
        if (!isScrambling && !_isJumping) transform.Rotate(new Vector3(0f, (_bothDown) ? 0f : ((_qDown) ? 1f : ((_pDown) ? -1f : 0f)), 0f) * Time.deltaTime * _speed * (Input.GetKey(_reverse) ? -1f : 1f));

        /// Apply movement
        if (!isScrambling && !_isJumping) _agent.SetDestination((_bothDown)? (transform.position +  (transform.forward * (Input.GetKey(_reverse) ? -1f : 1f))) : _agent.nextPosition);

        /// Continue attacks if target is in range
        _attackTimer = Mathf.Min(_attackTimer + Time.deltaTime, _attackCooldown);
        if (!isScrambling && !_isJumping && isReadyToAttack & Input.GetKeyDown(_shoot)) Attack(null);

        /// Jump for ground pound / control scramble
        if (!isScrambling && !_isJumping && !_qDown && !_pDown && !Input.GetKey(_shoot) && !Input.GetKey(_reverse) && Input.GetKeyDown(KeyCode.Space))
        {
            _isJumping = true;
            _damageCooldown = 0.2f;
            _body.isKinematic = false;
            _agent.enabled = false;
            _body.AddForce(Vector3.up * 350f);
        }

        /// Disable ground pound collider when done jumping
        if (!_isJumping && _groundPound.enabled && !isScrambling) _groundPound.enabled = false;

        /// Complete a jump
        if (_isJumping && _body.velocity.sqrMagnitude == 0f && _damageCooldown == 0f)
        {
            _ScrambleControls();
            _isJumping = false;
            Damage(groundPoundDamage / 3, transform.position);
            _groundPound.enabled = true;
            _body.isKinematic = true;
            _agent.enabled = true;
            _poundParticles.Play();
        }
    }

    /// <summary>
    /// Scrambles the controls for the player
    /// </summary>
    private void _ScrambleControls()
    {
        isScrambling = true;
        List<KeyCode> _codes = new List<KeyCode>();
        _keys.ForEach(k => _codes.Add(k));

        /// Configure new keys
        while (_codes.Count > 0)
        {
            int _index = Random.Range(0, _codes.Count);
            switch (_codes.Count)
            {
                case 4:
                    _leftTrack = _codes[_index];
                    break;
                case 3:
                    _reverse = _codes[_index];
                    break;
                case 2:
                    _shoot = _codes[_index];
                    break;
                default:
                    _rightTrack = _codes[_index];
                    break;
            }
            _codes.RemoveAt(_index);
        }

        void _MoveImage(Image _image, KeyCode _key)
        {
            /// Move images to match new keys
            _image.rectTransform.DOAnchorPosY(Random.Range(300f, 350f), 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => _image.rectTransform.DOAnchorPosY(0f, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => { isScrambling = false; }));
            _image.rectTransform.DOAnchorPosX(_keyImagePos[_key].x, 0.5f)
                .SetEase(Ease.OutQuad);
        }

        _MoveImage(_leftTrackImage, _leftTrack);
        _MoveImage(_reverseImage, _reverse);
        _MoveImage(_shootImage, _shoot);
        _MoveImage(_rightTrackImage, _rightTrack);

    }

    #endregion

}