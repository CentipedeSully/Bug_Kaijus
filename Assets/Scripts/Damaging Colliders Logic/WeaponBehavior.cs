using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WeaponType
{
    Spear
}

public interface IWeaponBehavior
{
    

    WeaponType Type { get; } 

    string Name { get; set; }

    string Description { get; set; }

    int Damage { get; set; }

    GameObject GetGameObject();

    List<Collider> GetSafeColliders();

    void ToggleDamageCollider(bool newState);



}

public class WeaponBehavior : MonoBehaviour, IDebugLoggable, IWeaponBehavior
{
    //Delcarations
    [Header("Weapon Settings")]
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private WeaponType _weaponType;
    [SerializeField][Min(1)] private int _damage;
    [SerializeField] private List<Collider> _safeColliders;
    [SerializeField] private Collider _weaponCollider;



    //Monobehaviours
    private void OnCollisionEnter(Collision collision)
    {
        if (!_safeColliders.Contains(collision.collider))
        {
             collision.gameObject?.GetComponent<IDamageable>().OnDamaged(_damage);
        }
    }




    //Internals




    //Externals
    public WeaponType Type => _weaponType;

    public string Name { get => _name; set { } }

    public string Description { get => _description; set { } }

    public int Damage { get => _damage; set { } }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public List<Collider> GetSafeColliders()
    {
        return _safeColliders;
    }

    public void ToggleDamageCollider(bool newState)
    {
        _weaponCollider.enabled = newState;
    }



    //Debugging
    public int LoggableID()
    {
        return GetInstanceID();
    }

    public string LoggableName()
    {
        return name;
    }

    
}
