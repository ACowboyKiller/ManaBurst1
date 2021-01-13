using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPlatform : MonoBehaviour
{

    #region --------------------    Public Enumerations



    #endregion

    #region --------------------    Public Events



    #endregion

    #region --------------------    Public Properties



    #endregion

    #region --------------------    Public Methods



    #endregion

    #region --------------------    Private Fields

    [SerializeField] private PlayerBot _player = null;
    [SerializeField] private ParticleSystem _healingParticles = null;

    #endregion

    #region --------------------    Private Methods

    private void OnTriggerEnter(Collider other) => _player = other.GetComponent<PlayerBot>();

    private void OnTriggerStay(Collider other) => _player = _player == null ? other.GetComponent<PlayerBot>() : _player;

    private void OnTriggerExit(Collider other) => _player = _player != null && _player.gameObject == other.gameObject ? null : _player;

    private void Update()
    {
        if (GameManager.state != GameManager.GameState.Gameplay) return;
        if (_player != null)
        {
            _player.health = Mathf.Min(_player.health + (3f * Time.deltaTime), _player.maxHealth);
            if (_healingParticles.isStopped) _healingParticles.Play();
        }
        else
        {
            if (!_healingParticles.isStopped) _healingParticles.Stop();
        }
    }

    #endregion

}