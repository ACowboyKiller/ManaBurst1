using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillVolume : MonoBehaviour
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



    #endregion

    #region --------------------    Private Methods

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerTeam" || other.tag == "EnemyTeam")
        {
            other.GetComponent<Minion>()?.Die();
        }
    }

    #endregion

}