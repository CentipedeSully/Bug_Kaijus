using System;
using System.Collections;
using System.Collections.Generic;
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
    ResetingAttack,
    Stunned
}

public class ActorBehaviour : MonoBehaviour, IDebugLoggable, IInteractable, IActorBehavior, IDamageable
{
    //Declarations
    [Header("Basic Combat Utilities")]
    [SerializeField] private float _attackCooldown = .5f;
    [SerializeField] private bool _isAttackReady = true;
    [SerializeField] private bool _isPerformingAction = false;
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

    [Header("Abilities Utilities")]
    [SerializeField] private List<GameObject> _abilityList = new List<GameObject>();

    [Header("Animation Utilities")]
    [SerializeField] private AnimState _currentAnimState = AnimState.Idling;
    [SerializeField] private string _combateStateParamName = "isInCombat";
    [SerializeField] private string _onDamagedParamName;
    [SerializeField] private string _attackingStateParamName;
    [SerializeField] private Animator _actorAnimator;

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
        //if we're in combat and not currently performing an action
        if (_isInCombat && !_isPerformingAction)
        {
            //Leave combat if the target vanished
            if (_currentTarget == null)
                ClearCurrentCommand();

            else
            {
                if (IsTargetInRange())
                {
                    //stop moving
                    _navAgent.ResetPath();

                    //perform the attack
                    //...
                }
                    

                //else approach target
                else
                    MoveActorToDestination(_currentTarget.transform.position);
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
        _navAgent.SetDestination(destination);
    }

    public void PerformAbility(int abilitySlot)
    {
        throw new NotImplementedException();
    }

    public void ClearCurrentCommand()
    {
        //Clear current target
        _currentTarget = null;

        //stop current pathing
        _navAgent.ResetPath();
        

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

    public void SetCurrentWeapon(GameObject weaponObject)
    {
        if (weaponObject != null)
            _currentWeapon = weaponObject;
    }

    public void ChangeAnimState(AnimState newState)
    {
        switch (newState) 
        {
            case AnimState.Idling:
                _actorAnimator.SetBool(_combateStateParamName, false);
                break;

            case AnimState.InCombat:
                _actorAnimator.SetBool(_combateStateParamName, true);
                break;

            case AnimState.OnDamaged:
                _actorAnimator.SetTrigger(_onDamagedParamName);
                break;

            case AnimState.Stunned:
                break;

            case AnimState.WarmingAttack:
                break;

            case AnimState.Attacking:
                _actorAnimator.SetBool(_attackingStateParamName, true);
                break;

            case AnimState.ResetingAttack:
                break;

            default:
                LogDebug.Log($"Attempted to change Anim State to nonexistent state: {newState}");
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
