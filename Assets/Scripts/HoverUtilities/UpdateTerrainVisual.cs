using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateTerrainVisual : MonoBehaviour, IInteractible, IDebugLoggable
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
        throw new System.NotImplementedException();
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
        throw new System.NotImplementedException();
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
