using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public interface IActorBehavior
{
    GameObject GetGameObject();

    void MoveActorToDestination(Vector3 position);

    void PerformAbility(int abilitySlot = 0, GameObject target = null);

    void ClearCurrentCommand();

}

public class ActorBehaviour : MonoBehaviour, IDebugLoggable, IInteractable, IActorBehavior
{
    //Declarations
    [Header("States")]
    [SerializeField] private bool _isInCombat = false;
    [SerializeField] private bool _isFacingTarget = false;
    [SerializeField] private float _distanceFromTarget;
    [SerializeField] private GameObject _currentTarget;

    [Header("Settings")]
    private Color _originalColor;
    [SerializeField] private bool _isSelected = false;
    [SerializeField] private Color _onHoverColor;
    [SerializeField] private Color _onSelectedColor;
    [SerializeField] private GameObject _targetedVisualObj;
    private NavMeshAgent _navAgent;
    [SerializeField] private GameObject _basicAttack;



    //Monobehaviours
    private void Start()
    {
        _originalColor = GetComponent<Renderer>().material.color;
        _navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        PursueTarget();
    }



    //Internals
    private void SetColor(Color newColor)
    {
        GetComponent<Renderer>().material.color = newColor;
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
        //if we're in combat and not currently performing an ability
        if (_isInCombat && !_basicAttack.GetComponent<IAbilityBehavior>().IsAbilityInProgress())
        {
            //Leave combat if the target vanished
            if (_currentTarget == null)
                ClearCurrentCommand();

            else
            {
                //calculate the target's distance
                _distanceFromTarget = transform.InverseTransformVector(_currentTarget.transform.position).magnitude;


                if (IsTargetInRange())
                {
                    //stop moving
                    _navAgent.ResetPath();

                    //perform the action
                    _basicAttack.GetComponent<IAbilityBehavior>().PerformAbility();
                }
                    

                //else approach target
                else
                    MoveActorToDestination(_currentTarget.transform.position);
            }

        }
    }

    private bool IsTargetInRange()
    {
        return _basicAttack.GetComponent<IAbilityBehavior>().IsObjectInRange(_currentTarget);
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
        //LogDebug.Log($"ContextCommand detected on actor: ${name}", this);
        
        //Trigger hostile actionVisual
        OnTargetedByPlayer();
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

    public void OnTargetedByPlayer()
    {
        if (!_targetedVisualObj.activeSelf)
            _targetedVisualObj.SetActive(true);
    }

    public void OnUntargetedByPlayer()
    {
        if (_targetedVisualObj.activeSelf)
            _targetedVisualObj.SetActive(false);
    }

    public void MoveActorToDestination(Vector3 destination)
    {
        _navAgent.SetDestination(destination);
    }

    public void PerformAbility(int abilitySlot = 0, GameObject target = null)
    {
        // 0 is the default core ability (currently basic attack)
        if (abilitySlot == 0 && target != null)
        {
            _isInCombat = true;
            _currentTarget = target;
        }
    }

    public void ClearCurrentCommand()
    {
        if (_isInCombat)
        {
            _isInCombat = false;

            //Deselect and clear the target
            if (_currentTarget != null)
                _currentTarget.GetComponent<IInteractable>().OnUntargetedByPlayer();

            _currentTarget = null;
        }

        //stop current pathing
        _navAgent.ResetPath();
        

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
