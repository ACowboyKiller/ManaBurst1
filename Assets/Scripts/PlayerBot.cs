using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

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

    #endregion

    #region --------------------    Public Methods

    /// <summary>
    /// Spawns the player bot in the provided position
    /// </summary>
    /// <param name="_pTransform"></param>
    public void Spawn(Transform _pTransform, GameManager.Lane _pLane)
    {
        transform.position = _pTransform.position;
        transform.rotation = _pTransform.rotation;
        health = _maxHealth;
        isAlive = true;
        gameObject.SetActive(true);
        //  TODO:   Play some animation
        _agent.enabled = true;
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
    }

    /// <summary>
    /// Damages the minion by the provided amount
    /// </summary>
    /// <param name="_pAmount"></param>
    public void Damage(float _pAmount, Vector3 _pPosition)
    {
        if (!isAlive) return;
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
        attackers.ForEach(a => a?.LoseTarget(gameObject));
        attackers.Clear();
        gameObject.SetActive(false);
    }

    #endregion

    #region --------------------    Private Fields

    [Header("Player Configurations")]
    [SerializeField] private NavMeshAgent _agent = null;
    [SerializeField] private float _speed = 4f;

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
    /// Disable agent rotation on player
    /// </summary>
    private void Start() => _agent.updateRotation = false;

    /// <summary>
    /// Either follows the target & attacks if in range & cooldown is done or begins walking towards the next tower
    /// </summary>
    private void Update()
    {
        if (!isAlive) return;

        bool _qDown = Input.GetKey(KeyCode.Q);
        bool _pDown = Input.GetKey(KeyCode.P);
        bool _bothDown = _qDown && _pDown;

        /// Apply Tank Rotation
        transform.Rotate(new Vector3(0f, (_bothDown) ? 0f : ((_qDown) ? -1f : ((_pDown) ? 1f : 0f)), 0f) * Time.deltaTime * _speed * (Input.GetKey(KeyCode.W) ? -1f : 1f));

        /// Apply movement
        _agent.SetDestination((_bothDown)? (transform.position +  (transform.forward * (Input.GetKey(KeyCode.W) ? -1f : 1f))) : _agent.nextPosition);

        /// Continue attacks if target is in range
        _attackTimer = Mathf.Min(_attackTimer + Time.deltaTime, _attackCooldown);
        if (isReadyToAttack & Input.GetKeyDown(KeyCode.O)) Attack(null);
    }

    #endregion

}