using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Minion : MonoBehaviour, iSpawnable, iDirectable, iCombatable
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
    /// Stores the current health of the minion
    /// </summary>
    public float health { get; set; } = 0f;

    /// <summary>
    /// Stores the lane of the minion
    /// </summary>
    public GameManager.Lane lane { get; private set; } = GameManager.Lane.Mid;

    /// <summary>
    /// The attackers currently targetting the minion
    /// </summary>
    public List<iCombatable> attackers { get; set; } = new List<iCombatable>();

    /// <summary>
    /// Stores the current target gameobject
    /// </summary>
    public GameObject target { get; set; } = null;

    /// <summary>
    /// Returns whether or not the minion is correcting its orientation
    /// </summary>
    public bool isCorrecting => !_agent.enabled && _body.velocity.magnitude < 1f;

    /// <summary>
    /// Returns whether or not the minion has a target
    /// </summary>
    public bool hasTarget => target != null && target.activeInHierarchy;

    /// <summary>
    /// Returns whether or not the minion is ready to attack
    /// </summary>
    public bool isReadyToAttack => _attackTimer == _attackCooldown;

    #endregion

    #region --------------------    Public Methods

    /// <summary>
    /// Spawns the minion in the provided position
    /// </summary>
    /// <param name="_pTransform"></param>
    public void Spawn(Transform _pTransform, GameManager.Lane _pLane)
    {
        transform.position = _pTransform.position;
        transform.rotation = _pTransform.rotation;
        lane = _pLane;
        health = _maxHealth;
        isAlive = true;
        gameObject.SetActive(true);
        //  TODO:   Play some animation
        _agent.enabled = true;
        _body.isKinematic = true;
        Direct(GameManager.instance.NextTower(tag, lane).transform.position);
    }

    /// <summary>
    /// Directs the minion to travel to the provided position
    /// </summary>
    /// <param name="_pPosition"></param>
    public void Direct(Vector3 _pPosition)
    {
        if (!isAlive) return;
        _agent?.SetDestination(_pPosition);
    }

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
    public void GainTarget(GameObject _pTarget) => target = (!hasTarget) ? _pTarget : null;

    /// <summary>
    /// Removes the target if the provided object is the target
    /// </summary>
    /// <param name="_pTarget"></param>
    public void LoseTarget(GameObject _pTarget) => target = (target == _pTarget) ? null : target;

    /// <summary>
    /// Returns whether or not the combatant has a target
    /// </summary>
    /// <returns></returns>
    public bool HasTarget() => hasTarget;

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
        _missileLauncher.LookAt(target.transform.position + (Vector3.up * 0.05f), Vector3.up);
        _missile.Fire(_damage, tag, _missileLauncher.position, _missileLauncher.rotation);
    }

    /// <summary>
    /// Damages the minion by the provided amount
    /// </summary>
    /// <param name="_pAmount"></param>
    public void Damage(float _pAmount, Vector3 _pPosition)
    {
        if (!isAlive) return;
        _correctingTime = 0f;
        _body.isKinematic = false;
        _agent.enabled = false;
        _body.AddExplosionForce(150f, _pPosition - (Vector3.up * 0.5f), 2f);
        health = Mathf.Max(health - _pAmount, 0f);
        if (health == 0f)
        {
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
        target = null;
        attackers.ForEach(a => a?.LoseTarget(gameObject));
        attackers.Clear();
        gameObject.SetActive(false);
    }

    #endregion

    #region --------------------    Private Fields

    [Header("Minion Configurations")]
    [SerializeField] private NavMeshAgent _agent = null;
    [SerializeField] private Rigidbody _body = null;
    private float _correctingTime = 0f;

    [Header("Combat Configurations")]
    [SerializeField] private float _maxHealth = 3f;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _attackCooldown = 1.5f;
    private float _attackTimer = 0f;
    [SerializeField] private GameObject _missilePrefab = null;
    [SerializeField] private Transform _missileLauncher = null;

    #endregion

    #region --------------------    Private Methods

    /// <summary>
    /// Either follows the target & attacks if in range & cooldown is done or begins walking towards the next tower
    /// </summary>
    private void Update()
    {
        if (!isAlive) return;

        /// Reenable the agent when the velocity dies down
        if (isCorrecting)
        {
            _correctingTime += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 2f * Time.deltaTime);
            if (_correctingTime >= 0.5f)
            {
                _body.isKinematic = true;
                _agent.enabled = true;
            }
        }

        /// Performs target check
        bool _t = hasTarget;

        /// Move towards target otherwise move towards next tower
        target = (_t)? target : null;
        if (_agent.enabled) _agent.SetDestination((_t) ? target.transform.position : GameManager.instance.NextTower(tag, lane).transform.position);

        /// Continue attacks if target is in range
        _attackTimer = (_t) ? Mathf.Min(_attackTimer + Time.deltaTime, _attackCooldown) : 0f;
        if (isReadyToAttack && _t) Attack(target);
    }

    #endregion

}