using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMelee : Ability
{
    //Declarations
    [Header("Basic Melee Utilities")]
    [SerializeField] private LayerMask _detectableInteractables;
    [SerializeField] private Color _attackRangeGizmoColor = Color.red;
    [SerializeField] private Vector3 _attackCastOrigin;
    [SerializeField] private Vector3 _attackRangeSize;
    private Vector3 _calculatedOrigin;




    //Monobehaviours
    private void OnDrawGizmosSelected()
    {
        DrawAttackRangeGizmo();
    }



    //Internals
    private void DrawAttackRangeGizmo()
    {
        Gizmos.color = _attackRangeGizmoColor;
        _calculatedOrigin = transform.position + transform.TransformDirection(_attackCastOrigin);
        Gizmos.DrawWireCube(_calculatedOrigin, _attackRangeSize);
    }



    //Externals
    public override void InterruptAbility()
    {
        if (IsAbilityInProgress())
        {
            LogDebug.Log($"Ability {this.name} interrupted!",this);
            EndAbility();
        }
    }

    public override void PerformAbility()
    {
        EnterAbility();
        LogDebug.Log("Melee Atk Triggered", this);
        EndAbility();
    }

    public override bool IsObjectInRange(GameObject targetObject)
    {
        Vector3 halfExtendsAttackSize = new(_attackRangeSize.x / 2, _attackRangeSize.y / 2, _attackRangeSize.z / 2);
        Collider[] hits = Physics.OverlapBox(_calculatedOrigin, halfExtendsAttackSize,Quaternion.identity, _detectableInteractables);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == targetObject)
                return true;
        }

        return false;
    }


    //Debugging

}
