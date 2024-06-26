using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEntitiesOnTouch : MonoBehaviour
{
    [SerializeField] private List<Collider> _safeColliders;
    [SerializeField] private int _damage;



    private void OnCollisionEnter(Collision collision)
    {
        if (!_safeColliders.Contains(collision.collider))
        {
             collision.gameObject?.GetComponent<IDamageable>().OnDamaged(_damage);
        }
    }
}
