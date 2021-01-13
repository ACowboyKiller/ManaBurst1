using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPooler
{

    #region --------------------    Public Properties

    /// <summary>
    /// The list of all pooled gameobjects in the current scene.
    /// </summary>
    public static List<GameObject> pool { get; private set; } = new List<GameObject>();

    #endregion

}

public static class GameObjectPoolerExtensions
{

    #region --------------------    Public Methods

    /// <summary>
    /// Instantiates a copy of the provided prefab and returns the clone as inactive.
    /// </summary>
    /// <param name="_pGameObject">The prefab or gameobject to clone.</param>
    public static GameObject PoolInstantiate(this GameObject _pGameObject)
    {
        GameObjectPooler.pool.RemoveAll(go => go == null);
        GameObject _return = GameObjectPooler.pool.Find(go => !go.activeSelf && go.name == _pGameObject.name);
        if (_return == null)
        {
            bool _wasActive = _pGameObject.activeSelf;
            _pGameObject.SetActive(false);
            _return = GameObject.Instantiate(_pGameObject);
            _return.name = _pGameObject.name;
            _pGameObject.SetActive(_wasActive);
            GameObjectPooler.pool.Add(_return);
        }
        return _return;
    }

    #endregion

}