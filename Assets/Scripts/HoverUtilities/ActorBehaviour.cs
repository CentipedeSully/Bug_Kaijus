using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ActorBehaviour : MonoBehaviour, IDebugLoggable, IInteractable
{
    //Declarations
    private Color _originalColor;
    [SerializeField] private bool _isSelected = false;
    [SerializeField] private Color _onHoverColor;
    [SerializeField] private Color _onSelectedColor;
    private NavMeshAgent _navAgent;




    //Monobehaviours
    private void Start()
    {
        _originalColor = GetComponent<Renderer>().material.color;
        _navAgent = GetComponent<NavMeshAgent>();
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



    //Externals
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public InteractableType Type()
    {
        return InteractableType.Actor;
    }

    public void OnAuillaryClick()
    {
        throw new System.NotImplementedException();
    }

    public void OnContextAction()
    {
        throw new System.NotImplementedException();
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

    public void MoveActor(Vector3 destination)
    {
        _navAgent.SetDestination(destination);
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
