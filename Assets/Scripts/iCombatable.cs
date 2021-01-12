using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface iCombatable
{

    #region --------------------    Public Methods

    bool IsAlive();

    bool IsEnemy(GameObject _pTarget);

    void GainTarget(GameObject _pObject);

    void LoseTarget(GameObject _pTarget);

    bool HasTarget();

    void BecomeTargetted(iCombatable _pObject);

    void Attack(GameObject _pTarget);

    void Damage(float _pAmount, Vector3 _pPosition);

    void Die();

    #endregion

}