using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBehavior : MonoBehaviour, IInteractable, IDebugLoggable
{
    //Declarations
    [SerializeField] private ManipulatorController _manipulator;
    [SerializeField] private bool _isCurrentlyHovered = false;

    //Monobehaviours
    private void Update()
    {
        UpdateTerrainVisualizer();
    }


    //Internals
    private void UpdateTerrainVisualizer()
    {
        if (_isCurrentlyHovered)
        {
            //activate the terrain visualizer
            _manipulator.ShowTerrainVisualizer();

            //move the visualizer to the point of contact
            _manipulator.SetTerrainVisualizerPosition(_manipulator.GetCurrentHoverContactPoint());
        }
        else
        {
            //deactivate the terrain visualizer
            _manipulator.HideTerrainVisualizer();
        }
    }


    //Externals
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public InteractableType Type()
    {
        return InteractableType.Terrain;
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
        LogDebug.Log("Terrain deselected",this);
    }

    public void OnHoverEnter()
    {
        _isCurrentlyHovered = true;
    }

    public void OnHoverExit()
    {
        _isCurrentlyHovered = false;
    }

    public void OnSelect()
    {
        _manipulator.TriggerPlayerMoveCommand(_manipulator.GetSelectionContactPoint());
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
