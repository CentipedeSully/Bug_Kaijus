using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;


public interface IActorBehavior
{
    GameObject GetGameObject();

    void MoveActorToDestination(Vector3 position);

    void AttackTarget(GameObject target);

    void PerformAbility(int abilitySlot);

    GameObject GetCurrentWeapon();

    void SetCurrentWeapon(GameObject weaponObject);

    void ClearCurrentCommand();

    void ChangeAnimState(AnimState newState);

}



public enum AnimState
{
    Idling,
    InCombat,
    OnDamaged,
    WarmingAttack,
    Attacking,
    RecoveringAttack,
    CancelingAttack,
    Stunned
}

public enum AnimLerpDirection
{
    Unset,
    IntoCombat,
    OutOfCombat
}

public class ActorBehaviour : MonoBehaviour, IDebugLoggable, IInteractable, IActorBehavior, IDamageable
{
    //Declarations
    [Header("Basic Combat Utilities")]
    [SerializeField] private float _revertToIdleTime = 1;
    [SerializeField] private float _attackCooldown = .5f;
    [SerializeField] private bool _isAttackReady = true;
    [SerializeField] private bool _isAttacking = false;
    //[SerializeField] private bool _isStunned = false;
    [SerializeField] private bool _isInCombat = false;
    [SerializeField] private GameObject _currentTarget;
    [SerializeField] private GameObject _currentWeapon;
    [SerializeField] private LayerMask _detectableInteractables;
    [SerializeField] private Vector3 _attackCastOrigin;
    [SerializeField] private Vector3 _attackRangeSize;
    private Vector3 _calculatedOrigin;
    [SerializeField] private bool _debugShowAttackRangeObject = false;
    [SerializeField] private GameObject _debugTestAttackRangeObject;
    private IEnumerator _currentAttackSequenceTracker;
    private IEnumerator _revertToIdleCounter;
    private bool _interruptAttackCmd = false;

    [Header("Abilities Utilities")]
    [SerializeField] private List<GameObject> _abilityList = new List<GameObject>();

    [Header("Animation Utilities")]
    private AnimLerpDirection _idleStanceLerpDirection = AnimLerpDirection.Unset;
    [SerializeField] private AnimState _currentAnimState = AnimState.Idling;
    [SerializeField] private string _onDamagedParam = "onDamaged";
    [SerializeField] private string _attackParam= "onAttackTriggered";
    [SerializeField] private string _cancelAttackParam = "onAttackCanceled";
    [SerializeField] private float _idleToCombatTime = .2f;
    [SerializeField] private Animator _actorAnimator;
    [SerializeField] private string _enterAttackClipName;
    [SerializeField] private string _castAttackClipName;
    [SerializeField] private string _recoverAttackClipName;
    [SerializeField] private float _enterAttackAnimLength;
    [SerializeField] private float _castAttackAnimLength;
    [SerializeField] private float _recoverAttackAnimLength;
    //[SerializeField] private string _enterAttackName

    [Header("OnDamaged Settings")]
    [SerializeField] private float _shakeDuration = .1f;
    [SerializeField] private float _shakeMagnitude = .1f;
    [SerializeField] private bool _toggleScreenShakeOnHit = true;
    [SerializeField] private CameraController _cameraController;

    [Header("Manipulation Settings")]
    private Color _originalColor;
    [SerializeField] private bool _isSelected = false;
    [SerializeField] private Color _onHoverColor;
    [SerializeField] private Color _onSelectedColor;
    private NavMeshAgent _navAgent;
    [SerializeField] private Renderer _modelRenderer;



    //Monobehaviours
    private void Start()
    {
        if (_modelRenderer != null)
            _originalColor = _modelRenderer.material.color;
        _navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        UpdateTargetingUtils();
        LerpTransitionAnimations();
        InterruptAttackOnCommand();
        PursueTarget();
        
    }



    //Internals
    private void SetColor(Color newColor)
    {
        if (_modelRenderer != null)
            _modelRenderer.material.color = newColor;
    }

    private void ShowHoverVisual()
    {
        if (!_isSelected)
            SetColor(_onHoverColor);
    }

    private void HideHoverVisual()
    {
        if (!_isSelected)
            SetColor(_originalColor);
    }

    private void ShowSelectedVisual()
    {
        _isSelected = true;
        SetColor(_onSelectedColor);
    }

    private void HideSelectedVisual()
    {
        _isSelected = false;
        SetColor(_originalColor);
    }


    private void PursueTarget()
    {
        //if we're in combat with a target and not currently performing an action
        if (_isInCombat && !_isAttacking && _currentTarget != null)
        {
            //are we in range?
            if (IsTargetInRange())
            {
                //stop moving
                _navAgent.ResetPath();

                //perform the attack
                PerformAttack();

            }
                    

            //else approach target
            else
                MoveActorToDestination(_currentTarget.transform.position);
            
        }

        //are we in a combat state without a target?
        else if (_isInCombat && _currentTarget == null)
        {
            //if we aren't yet reverting to a passive idle state
            if (_revertToIdleCounter == null)
            {
                //setup our counter utility
                _revertToIdleCounter = CountTimeUntilRevertToIdle();

                //Trigger the countdown
                StartCoroutine(_revertToIdleCounter);

            }
        }
    }

    private bool IsTargetInRange()
    {
        Vector3 halfExtendsAttackSize = new(_attackRangeSize.x / 2, _attackRangeSize.y / 2, _attackRangeSize.z / 2);
        Collider[] hits = Physics.OverlapBox(_calculatedOrigin, halfExtendsAttackSize, transform.rotation, _detectableInteractables);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == _currentTarget)
                return true;
        }

        return false;
    }

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

    private void ResetDamagedVisual()
    {
        //_isInDamagedState = false;
        SetColor(_originalColor);
    }

    private void PerformAttack()
    {
        if (_isAttackReady)
        {
            //enter attack state
            _isAttackReady = false;
            _isAttacking = true;

            //setup the animation-tracking manager
            _currentAttackSequenceTracker = TimeAttackLogicToAnimationLengths();

            //Make sure our animationClip times match the current weapon's type (spear anims for the spear weapon)
            ReadAnimationClipLengths();

            //Begin the attack animation chain
            _actorAnimator.SetFloat("AttackPhase", 0);
            ChangeAnimState(AnimState.Attacking);

            //Begin the animation tracker
            StartCoroutine(_currentAttackSequenceTracker);

        }
    }

    private void ReadyAttack()
    {
        _isAttackReady = true;
    }

    private void InterruptAttackOnCommand()
    {
        if (_interruptAttackCmd && _isAttacking)
        {
            //IMMEDIATELY Stop the current attack sequence manager
            StopCoroutine(_currentAttackSequenceTracker);
            _currentAttackSequenceTracker = null;

            //Disable the actor's weapon's collider in case we're mid attack
            _currentWeapon.GetComponent<IWeaponBehavior>().ToggleDamageCollider(false);


            //reset the attack command and update the attack state 
            _interruptAttackCmd = false;
            _isAttacking = false;

            //trigger the cancelAttack animation
            ChangeAnimState(AnimState.CancelingAttack);
            _currentAnimState = AnimState.InCombat;

            //reset the attack phase
            _actorAnimator.SetFloat("AttackPhase", 0);

            //begin cooling down the Attack ability
            Invoke(nameof(ReadyAttack), _attackCooldown);
        }

    }

    private IEnumerator TimeAttackLogicToAnimationLengths()
    {
        //update the current actor's state (for debugging)
        _currentAnimState = AnimState.WarmingAttack;

        //wait for the actor to finish entering its attack
        yield return new WaitForSeconds(_enterAttackAnimLength);




        //update into the next state
        _currentAnimState = AnimState.Attacking;
        _actorAnimator.SetFloat("AttackPhase", .5f);

        //turn on the actor's damaging weapon collider
        _currentWeapon.GetComponent<IWeaponBehavior>().ToggleDamageCollider(true);

        //wait for this attack animation to complete
        yield return new WaitForSeconds(_castAttackAnimLength);




        //update into the final attack-sequence state
        _currentAnimState = AnimState.RecoveringAttack;
        _actorAnimator.SetFloat("AttackPhase", 1);

        //turn off the actor's damaging weapon collider
        _currentWeapon.GetComponent<IWeaponBehavior>().ToggleDamageCollider(false);

        //wait for this last animation to complete
        yield return new WaitForSeconds(_recoverAttackAnimLength);




        //now we've exited the attack!
        _currentAnimState = AnimState.InCombat;
        _actorAnimator.SetFloat("AttackPhase", 0);

        // clear this utility for another time
        _currentAttackSequenceTracker = null;

        //exit attack state
        _isAttacking = false;

        //begin cooling down the attack ability
        Invoke(nameof(ReadyAttack), _attackCooldown);
    }

    private void ReadAnimationClipLengths()
    {
        AnimationClip[] allClips = _actorAnimator.runtimeAnimatorController.animationClips;
        LogDebug.Log($"Found anim clips: {allClips.Length}", this);

        switch (_currentWeapon.GetComponent<IWeaponBehavior>().Type)
        {
            case WeaponType.Spear:
                _enterAttackAnimLength = GetClipLength(_enterAttackClipName, allClips);
                _castAttackAnimLength = GetClipLength(_castAttackClipName, allClips);
                _recoverAttackAnimLength = GetClipLength(_recoverAttackClipName, allClips);
                break;
        }
    }

    private float GetClipLength(string desiredClip, AnimationClip[] clipCollection)
    {
        for (int i = 0; i < clipCollection.Length; i++)
        {
            if (clipCollection[i].name == desiredClip)
                return clipCollection[i].length;
        }

        LogDebug.Error($"Specified animationClip ({desiredClip}) doesn't exist within the current runtime controller. " +
            $"Returning -1 as the clip's duration",this);
        return -1;
    }

    private void LerpTransitionAnimations()
    {
        //Smooth out the idle transition each frame if our smoothing switch is triggered
        if (_idleStanceLerpDirection != AnimLerpDirection.Unset)
        {
            // The CombatStance parameter goes from 0f to 1f
            // 0: means out of combat
            // 1: means in combat
            // anything float in between is a blend of the two animations


            // is our lerp switch set to enter combat?
            if (_idleStanceLerpDirection == AnimLerpDirection.IntoCombat)
            {
                //Lerp the current idle parameter into combat if the parameter isn't 1 
                if (_actorAnimator.GetFloat("CombatStance") < 1)
                {
                    _actorAnimator.SetFloat("CombatStance", 1, _idleToCombatTime, Time.deltaTime);

                    //fix the .999999 != 1 lerping issue
                    //Just snap to complete when "close enough"
                    if (_actorAnimator.GetFloat("CombatStance") > .95)
                        _actorAnimator.SetFloat("CombatStance", 1);
                }
                    

                //otherwise, lerp completed. Reset the lerp direction
                else
                    _idleStanceLerpDirection = AnimLerpDirection.Unset;
            }

            // is our lerp switch set to leave combat?
            else if (_idleStanceLerpDirection == AnimLerpDirection.OutOfCombat)
            {
                //Lerp the current idle parameter OUT of combat if the parameter isn't 0
                if (_actorAnimator.GetFloat("CombatStance") > 0)
                {
                    _actorAnimator.SetFloat("CombatStance", 0, _idleToCombatTime, Time.deltaTime);

                    //fix the .00001 != 0 lerping issue
                    //Just snap to complete when "close enough"
                    if (_actorAnimator.GetFloat("CombatStance") < .05)
                        _actorAnimator.SetFloat("CombatStance", 0);
                }
                    

                //otherwise, lerp completed. Reset the lerp direction
                else
                    _idleStanceLerpDirection = AnimLerpDirection.Unset;
            }
        }
    }

    private IEnumerator CountTimeUntilRevertToIdle()
    {
        //wait
        yield return new WaitForSeconds(_revertToIdleTime);

        //clear this counter
        _revertToIdleCounter = null;

        //enter the idleState
        _isInCombat = false;
        ChangeAnimState(AnimState.Idling);
    }




    //Externals
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public InteractableType Type()
    {
        return InteractableType.Actor;
    }

    public void OnAuxiliaryClick()
    {
        LogDebug.Log($"AuxClick detected on actor: ${name}", this);
        
    }

    public void OnContextAction()
    {
        
    }

    public void OnDeselect()
    {
        HideSelectedVisual();
    }

    public void OnHoverEnter()
    {
        ShowHoverVisual();
    }

    public void OnHoverExit()
    {
        HideHoverVisual();
    }

    public void OnSelect()
    {
        ShowSelectedVisual();
    }

    public void MoveActorToDestination(Vector3 destination)
    {
        //Clear the standby counter on move


        //move
        _navAgent.SetDestination(destination);
    }

    public void PerformAbility(int abilitySlot)
    {
        //Be sure to clear the standby counter if it's ticking
        //Be sure to enter the combat state
        //BE SURE TO LIKE AND SUBSCRIBE!

        throw new NotImplementedException();
    }

    public void ClearCurrentCommand()
    {
        //Clear current target
        _currentTarget = null;

        //stop current pathing
        _navAgent.ResetPath();

        //Trigger the StandbyWait
        if (_revertToIdleCounter == null)
        {
            _revertToIdleCounter = CountTimeUntilRevertToIdle();
            StartCoroutine(_revertToIdleCounter);
        }
        

    }

    public void OnDamaged(int damage)
    {
        ChangeAnimState(AnimState.OnDamaged);

        if (_toggleScreenShakeOnHit)
            _cameraController.ShakeCamera(_shakeDuration, _shakeMagnitude);
        /*
        if (_isInDamagedState)
            CancelInvoke(nameof(ResetDamagedVisual));

        _isInDamagedState = true;
        SetColor(_onDamagedColor);
        Invoke(nameof(ResetDamagedVisual), _damagedDuration);
        */
    }

    public void AttackTarget(GameObject target)
    {
        //clear the standby timer if it's active
        if (_revertToIdleCounter != null)
        {
            StopCoroutine(_revertToIdleCounter);
            _revertToIdleCounter = null;
        }

        //enter combat if not already in combat
        if (!_isInCombat)
        {
            _isInCombat = true;
            ChangeAnimState(AnimState.InCombat);
        }

        //set new target
        _currentTarget = target;
    }

    public GameObject GetCurrentWeapon()
    {
        return _currentWeapon;
    }

    public void SetCurrentWeapon(GameObject newBehavior)
    {
        if (newBehavior != null && newBehavior.GetComponent<IWeaponBehavior>() != null)
            _currentWeapon = newBehavior;
    }

    public void ChangeAnimState(AnimState newState)
    {
        switch (newState) 
        {
            case AnimState.Idling:
                _idleStanceLerpDirection = AnimLerpDirection.OutOfCombat;
                break;

            case AnimState.InCombat:
                _idleStanceLerpDirection = AnimLerpDirection.IntoCombat;
                break;

            case AnimState.OnDamaged:
                _actorAnimator.SetTrigger(_onDamagedParam);
                break;

            case AnimState.Stunned:
                break;

            case AnimState.WarmingAttack:
                LogDebug.Warn($"Attempted to forcefully enter a TRANSITIONAL animation state ({newState}). Ignoring Cmd.",this);
                break;

            case AnimState.Attacking:
                _actorAnimator.SetBool(_attackParam, true);
                break;

            case AnimState.RecoveringAttack:
                LogDebug.Warn($"Attempted to forcefully enter a TRANSITIONAL animation state ({newState}). Ignoring Cmd.", this);
                break;

            case AnimState.CancelingAttack:
                _actorAnimator.SetTrigger(_cancelAttackParam);
                break;

            default:
                LogDebug.Warn($"Attempted to change into an unsupported animation state: {newState}",this);
                break;
        }


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
