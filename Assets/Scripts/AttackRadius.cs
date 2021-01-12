using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRadius : MonoBehaviour
{

    #region --------------------    Public Enumerations



    #endregion

    #region --------------------    Public Events



    #endregion

    #region --------------------    Public Properties

    /// <summary>
    /// The combatant above the attack radius
    /// </summary>
    public iCombatable combatant { get; set; } = null;

    #endregion

    #region --------------------    Public Methods



    #endregion

    #region --------------------    Private Fields



    #endregion

    #region --------------------    Private Methods

    /// <summary>
    /// Gets the combatant component
    /// </summary>
    private void Start() => combatant = GetComponentInParent<iCombatable>();

    /// <summary>
    /// Targets an enemy when it enters the radius
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        /// Breakout if the other object is not tagged or is friendly
        if (!combatant.IsAlive()) return;
        if (!combatant.IsEnemy(other.gameObject)) return;
        combatant.GainTarget(other.gameObject);
    }

    /// <summary>
    /// Picks a new target if the previous target dies or leaves the radius
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (!combatant.IsAlive()) return;
        if (combatant.HasTarget()) return;
        if (!combatant.IsEnemy(other.gameObject)) return;
        combatant.GainTarget(other.gameObject);
    }

    /// <summary>
    /// Removes the current tower target when it leaves the radius
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (!combatant.IsAlive()) return;
        combatant.LoseTarget(other.gameObject);
    }

    #endregion

}