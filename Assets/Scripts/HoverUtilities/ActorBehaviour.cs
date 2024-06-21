using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;


public interface IActorBehavior
{
    GameObject GetGameObject();

    void MoveActorToDestination(Vector3 position);

    void PerformAbility(int abilitySlot = 0, GameObject target = null);

    void ClearCurrentCommand();

    void AddPursuer(GameObject pursuer);

    void RemovePursuer(GameObject pursuer);

}

public class ActorBehaviour : MonoBehaviour, IDebugLoggable, IInteractable, IActorBehavior
{
    //Declarations
    [Header("States")]
    [SerializeField] private bool _isInCombat = false;
    [SerializeField] private GameObject _currentTarget;
    [SerializeField] private List<GameObject> _activePursuers = new List<GameObject>();

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
                //add this object as a pursuer of the target. (duplicate pusuers are ignored)
                _currentTarget.GetComponent<IActorBehavior>().AddPursuer(gameObject);


                if (IsTargetInRange())
                {
                    //stop moving
                    _navAgent.ResetPath();

                    //perform the action
                    _basicAttack.GetComponent<IAbilityBehavior>().TriggerAbility();
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
                _currentTarget.GetComponent<IActorBehavior>().RemovePursuer(gameObject);

            _currentTarget = null;
        }

        //stop current pathing
        _navAgent.ResetPath();
        

    }

    public void ShowBeingPursuedVisual()
    {
        if (!_targetedVisualObj.activeSelf && _activePursuers.Count > 0)
            _targetedVisualObj.SetActive(true);
    }

    public void HideBeingPursuedVisual()
    {
        if (_targetedVisualObj.activeSelf && _activePursuers.Count < 1)
            _targetedVisualObj.SetActive(false);
    }

    public void AddPursuer(GameObject pursuer)
    {
        if (!_activePursuers.Contains(pursuer) && pursuer!= null)
        {
            _activePursuers.Add(pursuer);
            ShowBeingPursuedVisual();
        }
    }

    public void RemovePursuer(GameObject puruser)
    {
        if (_activePursuers.Contains(puruser) && puruser != null)
        {
            _activePursuers.Remove(puruser);
            HideBeingPursuedVisual();
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
