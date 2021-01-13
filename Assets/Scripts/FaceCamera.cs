using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
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

    [SerializeField] private Transform _camera = null;

    #endregion

    #region --------------------    Private Methods

    private void LateUpdate()
    {
        if (GameManager.state != GameManager.GameState.Gameplay) return;
        transform.LookAt(_camera, Vector3.up);
    }

    #endregion

}