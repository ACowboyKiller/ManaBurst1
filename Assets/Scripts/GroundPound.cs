using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPound : MonoBehaviour
{

    #region --------------------    Private Methods

    /// <summary>
    /// Explode on contact with enemy
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Untagged" || other.CompareTag(transform.parent.tag)) return;
        other.GetComponent<iCombatable>()?.Damage(transform.GetComponentInParent<PlayerBot>().groundPoundDamage, transform.position);
    }

    #endregion

}