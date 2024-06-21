using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;

public class BasicMelee : Ability
{
    //Declarations
    [Header("Basic Melee Utilities")]
    [SerializeField] private LayerMask _detectableInteractables;
    [SerializeField] private Vector3 _attackCastOrigin;
    [SerializeField] private Vector3 _attackRangeSize;
    private Vector3 _calculatedOrigin;
    [SerializeField] private bool _debugShowAttackRangeObject = false;
    [SerializeField] private GameObject _debugTestAttackRangeObject;





    //Monobehaviours
    private void Update()
    {
        UpdateTargetingUtils();
    }



    //Internals
    private void UpdateTargetingUtils()
    {
        //update the displaced attack origin (for the boxcast)
        _calculatedOrigin = transform.position + transform.TransformDirection(_attackCastOrigin);

        //update the debug visual's position & rotation (this is done to verify the boxcast's true position)
        _debugTestAttackRangeObject.transform.localScale = _attackRangeSize;
        _debugTestAttackRangeObject.transform.SetPositionAndRotation(_calculatedOrigin, transform.rotation);

        //show the debug object if it's toggled.
        if (_debugShowAttackRangeObject)
        {
            if (!_debugTestAttackRangeObject.activeSelf)
                _debugTestAttackRangeObject.SetActive(true);
        }

        //otherwise, hide it if it's still on
        else
        {
            if (_debugTestAttackRangeObject.activeSelf)
                _debugTestAttackRangeObject.SetActive(false);
        }
    }

    protected override void PerformConcreteInterruption()
    {
        LogDebug.Log($"Ability {this.name} interrupted!", this);
        EndAbility();
    }

    protected override void PerformConcreteAbilityLogic()
    {
        EnterAbility();
        LogDebug.Log("Performed Melee!",this);
        EndAbility();
    }

    //Externals


    public override bool IsObjectInRange(GameObject targetObject)
    {
        Vector3 halfExtendsAttackSize = new(_attackRangeSize.x / 2, _attackRangeSize.y / 2, _attackRangeSize.z / 2);
        Collider[] hits = Physics.OverlapBox(_calculatedOrigin, halfExtendsAttackSize,transform.rotation, _detectableInteractables);
        
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == targetObject)
                return true;
        }

        return false;
    }


    //Debugging

}
