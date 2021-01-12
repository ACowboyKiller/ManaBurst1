﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Tower : MonoBehaviour, iCombatable
{

    #region --------------------    Public Enumerations



    #endregion

    #region --------------------    Public Events



    #endregion

    #region --------------------    Public Properties

    /// <summary>
    /// Stores whether or not the tower is alive
    /// </summary>
    public bool isAlive { get; set; } = true;

    /// <summary>
    /// The health of the tower
    /// </summary>
    public float health { get; set; } = 0f;

    /// <summary>
    /// Returns the lane of the tower
    /// </summary>
    public GameManager.Lane lane => _lane;

    /// <summary>
    /// The attackers currently targetting the minion
    /// </summary>
    public List<iCombatable> attackers { get; set; } = new List<iCombatable>();

    /// <summary>
    /// Stores the target for the tower
    /// </summary>
    public GameObject target { get; set; } = null;

    /// <summary>
    /// Returns whether or not the minion has a target
    /// </summary>
    public bool hasTarget => target != null && target.activeInHierarchy;

    /// <summary>
    /// Returns whether or not the tower is ready to fire
    /// </summary>
    public bool isReadyToAttack => _attackTimer == _attackCooldown;

    /// <summary>
    /// Returns whether or not the tower is ready to spawn minions
    /// </summary>
    public bool isReadyToSpawnMinions => _minionSpawnTimer == _minionSpawnCooldown;

    #endregion

    #region --------------------    Public Methods

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
        Missile _missile = _missilePrefab.PoolInstantiate().GetComponent<Missile>();
        _missileLauncher.LookAt(target.transform.position + (Vector3.up * 0.1f));
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
        target = null;
        attackers.ForEach(a => a.LoseTarget(gameObject));
        attackers.Clear();
        gameObject.SetActive(false);
    }

    #endregion

    #region --------------------    Private Fields

    [Header("Tower Configurations")]
    [SerializeField] private GameManager.Lane _lane = GameManager.Lane.Mid;
    [SerializeField] private bool _isMinionSpawner = false;
    [SerializeField] private GameObject _minionPrefab = null;
    [SerializeField] private float _minionSpawnCooldown = 25f;
    private float _minionSpawnTimer = 20f;
    [SerializeField] private Transform _minionSpawner = null;
    [SerializeField] private int _minionSpawnCount = 8;

    [Header("Combat Configurations")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _attackCooldown = 2f;
    private float _attackTimer = 0f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private GameObject _missilePrefab = null;
    [SerializeField] private Transform _missileLauncher = null;

    [Header("UI Configurations")]
    [SerializeField] private CustomProgressBar _healthBar = null;

    #endregion

    #region --------------------    Private Methods

    private void Awake()
    {
        health = _maxHealth;
    }

    /// <summary>
    /// Spawns minions and fires at enemy minions when they are close
    /// </summary>
    private void Update()
    {
        if (!isAlive) return;

        /// Update UI
        _healthBar.percent = health / _maxHealth;

        /// Performs target check
        bool _t = hasTarget;

        /// Move towards target otherwise move towards next tower
        target = (_t) ? target : null;

        /// Continue attacks if target is in range
        _attackTimer = (_t) ? Mathf.Min(_attackTimer + Time.deltaTime, _attackCooldown) : 0f;
        if (isReadyToAttack && _t) Attack(target);

        /// Spawn Minions when ready
        _minionSpawnTimer = Mathf.Min(_minionSpawnTimer + Time.deltaTime, _minionSpawnCooldown);
        if (_isMinionSpawner && isReadyToSpawnMinions)
        {
            _minionSpawnTimer = -1f * _minionSpawnCount;
            StartCoroutine(_SpawnMinions());
        }
    }

    /// <summary>
    /// Spawns minions
    /// </summary>
    private IEnumerator _SpawnMinions()
    {
        for (int i = 0; i < _minionSpawnCount; i ++)
        {
            yield return new WaitForSeconds(1f);
            Minion _minion = _minionPrefab.PoolInstantiate().GetComponent<Minion>();
            _minion.tag = tag;
            _minion.Spawn(_minionSpawner, _lane);
        }
    }

    #endregion

}