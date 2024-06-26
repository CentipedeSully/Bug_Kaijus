using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;

public class BasicMelee : Ability
{
    //Declarations
    [Header("Basic Melee Utilities")]
    [SerializeField] private bool _isAttacking = false;
    [SerializeField] private float _warmupTime = .5f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private LayerMask _detectableInteractables;
    [SerializeField] private Vector3 _attackCastOrigin;
    [SerializeField] private Vector3 _attackRangeSize;
    private Vector3 _calculatedOrigin;

    [Header("Other Utilities")]
    [SerializeField] private GameObject _weaponCollider;
    [SerializeField] private Animator _actorAnimator;
    [SerializeField] private string _animAttackStateName;
    [SerializeField] private string _attackTriggerParamName = "onAttackTriggered";



    [Header("Debug Utilities")]
    [SerializeField] private bool _debugShowAttackRangeObject = false;
    [SerializeField] private GameObject _debugTestAttackRangeObject;





    //Monobehaviours
    private void Update()
    {
        UpdateTargetingUtils();
        ManageAttack();
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
        CancelInvoke(nameof(EnterAttack));
        LogDebug.Log($"Ability {this.name} interrupted!", this);
        EndAbility();
    }

    protected override void PerformConcreteAbilityLogic()
    {
        EnterAbility();
        WarmUpAttack();
    }

    private void WarmUpAttack()
    {
        Invoke(nameof(EnterAttack), _warmupTime);
        LogDebug.Log("Warming up melee...", this);
    }

    private void EnterAttack()
    {
        _actorAnimator.SetTrigger(_attackTriggerParamName);
        _isAttacking = true;
        _weaponCollider.SetActive(true);
    }

    private void ManageAttack()
    {
        if (_isAttacking)
        {
            AnimatorStateInfo currentStateInfo = _actorAnimator.GetCurrentAnimatorStateInfo(0);
            LogDebug.Log($"Attacking. Current Anim State Info: {currentStateInfo}");
            if (!currentStateInfo.IsName(_animAttackStateName))
            {
                _isAttacking = false;
                _weaponCollider.SetActive(false);
                EndAbility();
            }
        }
    }

    private Collider[] CastAttackDetection()
    {
        Vector3 halfExtendsAttackSize = new(_attackRangeSize.x / 2, _attackRangeSize.y / 2, _attackRangeSize.z / 2);
        return  Physics.OverlapBox(_calculatedOrigin, halfExtendsAttackSize, transform.rotation, _detectableInteractables);
    }

    //Externals
    public override bool IsObjectInRange(GameObject targetObject)
    {
        Collider[] hits = CastAttackDetection();
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == targetObject)
                return true;
        }

        return false;
    }


    //Debugging

}
