using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface iSpawnable
{

    #region --------------------    Public Methods

    void Spawn(Transform _pTransform, GameManager.Lane _pLane);

    #endregion

}