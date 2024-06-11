using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IInteractable
{
    GameObject GetGameObject();

    InteractableType Type();

    void OnHoverEnter();

    void OnHoverExit();

    void OnSelect();

    void OnDeselect();

    void OnContextAction();

    void OnAuillaryClick();
}

public enum InteractableType
{
    Terrain,
    Actor,
    Interactable
}

public class ManipulatorController : MonoBehaviour, IDebugLoggable
{
    //Delcarations
    [Header("Manipulator Settings")]
    [Tooltip("" +
        "Anything belonging to this layermask will be detected by the manipulator. " +
        "Interactibles include terrain, actors, and other physical objects of interest")]
    [SerializeField] private LayerMask _interactiblesLayer;
    [SerializeField] [Min(0)] private float _maxCastDistance = 400;


    [Header("States, Utilities & References")]
    [SerializeField] private Vector2 _mouseScreenPosition;
    [SerializeField] private GameObject _selectedObject;
    private Vector3 _selectionPoint;
    [SerializeField] private Vector3 _pointOfHoveringContact;
    [SerializeField] private GameObject _lastObjectHovered;
    [SerializeField] private GameObject _playerActor;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private CameraController _camController;
    private bool _isCameraGizmoDrawable = false;

    private Vector3 _cameraOrigin;
    private Vector3 _cameraForwardsDirection;
    private Vector3 _mouseWorldPosition;

    [Header("Debug Settings")]
    [SerializeField] private bool _isDebugActive = false;
    [SerializeField] private GameObject _terrainVisualizerPrefab;
    private GameObject _terrainVisualizerObj;

    [SerializeField] private Color _cameraProjectionGizmoColor = Color.magenta;
    [SerializeField] private Color _mousePositionGizmoColor = Color.green;

    




    //Monobehaviours
    private void Start()
    {
        CreateTerrainVisualizer();
    }

    private void Update()
    {
        TrackMousePointer();
    }

    private void OnDrawGizmosSelected()
    {
        DrawMousePointerGizmos();
    }


    //Internal Utils
    private void CreateTerrainVisualizer()
    {
        if (_terrainVisualizerPrefab != null)
        {
            _terrainVisualizerObj = Instantiate(_terrainVisualizerPrefab);
            _terrainVisualizerObj.transform.SetParent(transform);
            _terrainVisualizerObj.transform.position = Vector3.zero;
        }
        else
            LogDebug.Error("TerrainVisualObj is empty", this);
            
    }

    private void UpdateRaycastUtilities()
    {
        //Get Camera world coords & forwards direction
        _cameraOrigin = _camController.GetCameraPosition();
        _cameraForwardsDirection = _camController.GetForwardCameraPerspectiveVector();

        //Get worldMousePosition
        _mouseWorldPosition = _camController.GetWorldPositionFromScreenPoint(_mouseScreenPosition);
    }

    private void TrackMousePointer()
    {
        //Validate the existence of our tools
        if (_inputReader != null && _camController != null)
        {
            //Get Mouse position
            _mouseScreenPosition = _inputReader.GetMousePositionOnScreen();

            //Is our mouse onScreen?
            if (_mouseScreenPosition.magnitude >= 0)
            {
                
                //enable gizmo visibility to help visualize our mouse-pointer raycast
                _isCameraGizmoDrawable = true;

                UpdateRaycastUtilities();

                //Capture the closest interactible that our mouse is currently hovering over
                CaptureHoveredObjects();

                //Move terrain visualizer to contact point if ther object is terrain
                //_terrainVisualizerObj.transform.position = _pointOfHoveringContact;

            }

            //otherwise log warning of missing mouse
            else
            {
                LogDebug.Warn("Invalid  mouse position detected. Manipulator standing by for valid mouse position", this);

                //Stop drawing gizmos
                _isCameraGizmoDrawable = false;
            }

        }

        //Stop drawing gizmos. Our utilities are probably out of date.
        else
            _isCameraGizmoDrawable = false;


    }

    private void CaptureHoveredObjects()
    {
        Vector3 mouseDirection = (_mouseWorldPosition - _cameraOrigin).normalized;
        RaycastHit[] hits = Physics.RaycastAll(_cameraOrigin, mouseDirection, _maxCastDistance, _interactiblesLayer);

        if (hits.Length > 0)
        {
            //prioritize detecting actors first
            (GameObject,Vector3) closestDetection = FindClosestInteractableWithType(InteractableType.Actor, hits);

            //Look for Terrain objects if no actors were found
            if (closestDetection.Item1 == null)
                closestDetection = FindClosestInteractableWithType(InteractableType.Terrain, hits);

            LogDebug.Log($"{hits.Length} Detected Hoverables: Selecting {closestDetection.Item1.name} from collection",this);

            //save the contact point
            _pointOfHoveringContact = closestDetection.Item2;

            // Trigger OnHover Enter/Exit functions if applicable
            if (_lastObjectHovered == null)
            {
                //save the new valid hovered object, and enter its hovered state
                _lastObjectHovered = closestDetection.Item1;
                _lastObjectHovered.GetComponent<IInteractable>().OnHoverEnter();
            }
            else if (_lastObjectHovered != closestDetection.Item1)
            {
                //update the previously hovered object as no longer being hovered over
                _lastObjectHovered.GetComponent<IInteractable>().OnHoverExit();

                //save the new valid hovered object, and enter its hovered state
                _lastObjectHovered = closestDetection.Item1;
                _lastObjectHovered.GetComponent<IInteractable>().OnHoverEnter();

            }

        }
        
        //Remove the last object hovered if we aren't hovering over anything anymore
        else if (_lastObjectHovered != null)
        {
            //Trigger the onHoverExit function on the last object and forget it
            _lastObjectHovered.GetComponent<IInteractable>().OnHoverExit();
            _lastObjectHovered = null;
        }
    }

    private void DrawMousePointerGizmos()
    {
        if (_isCameraGizmoDrawable)
        {
            Gizmos.color = _cameraProjectionGizmoColor;

            //Make sure we're looking in the same direction as our camera
            Gizmos.DrawLine(_cameraOrigin, _cameraOrigin + (_cameraForwardsDirection * 200));



            Gizmos.color = _mousePositionGizmoColor;

            //Draw the mouse's relational position vector
            Vector3 mouseDirection = (_mouseWorldPosition - _cameraOrigin).normalized;
            Gizmos.DrawLine(_cameraOrigin, _cameraOrigin + (mouseDirection * _maxCastDistance));
        }
    }

    private void CaptureSelection()
    {
        UpdateRaycastUtilities();

        Vector3 mouseDirection = (_mouseWorldPosition - _cameraOrigin).normalized;
        RaycastHit[] hits = Physics.RaycastAll(_cameraOrigin, mouseDirection, _maxCastDistance, _interactiblesLayer);

        if (hits.Length > 0)
        {
            //prioritize detecting actors first
            (GameObject, Vector3) closestDetection = FindClosestInteractableWithType(InteractableType.Actor, hits);

            //Look for Terrain objects if no actors were found
            if (closestDetection.Item1 == null)
                closestDetection = FindClosestInteractableWithType(InteractableType.Terrain, hits);

            //save the contact point
            _selectionPoint = closestDetection.Item2;

            // Trigger OnSelect/Deselect functions if applicable
            if (_selectedObject == null)
            {
                //save the selection
                _selectedObject = closestDetection.Item1;

                //Trigger the object's onSelect function
                _selectedObject.GetComponent<IInteractable>().OnSelect();
            }

            else if (_selectedObject != closestDetection.Item1)
            {
                //Deselect the previous object
                _selectedObject.GetComponent<IInteractable>().OnDeselect();

                //Save the detected object as the new selection, then trigger the detected object's onSelect function
                _selectedObject = closestDetection.Item1;
                _selectedObject.GetComponent<IInteractable>().OnSelect();
            }

            // Deselect the object if double clicked
            else if (_selectedObject == closestDetection.Item1)
            {
                //Reselect the terrain object if the detected object is terrain
                if (closestDetection.Item1.GetComponent<IInteractable>().Type() == InteractableType.Terrain)
                    _selectedObject.GetComponent<IInteractable>().OnSelect();

                else
                {
                    //deselect the current selected object
                    _selectedObject.GetComponent<IInteractable>().OnDeselect();
                    _selectedObject = null;
                }
            }   
        }
    }

    private (GameObject, Vector3) FindClosestInteractableWithType(InteractableType type, RaycastHit[] detections)
    {
        for (int i = detections.Length -1; i >= 0; i--)
        {
            GameObject possibleMatch = detections[i].collider.gameObject;
            if (possibleMatch.GetComponent<IInteractable>().Type() == type)
            {
                return (possibleMatch, detections[i].point);
            }
                
        }

        return (null,Vector3.zero);
    }

    //External Utils
    public void SetTerrainVisualizerPosition(Vector3 newPosition)
    {
        _terrainVisualizerObj.transform.position = newPosition;
    }

    public Vector3 GetCurrentHoverContactPoint()
    {
        return _pointOfHoveringContact;
    }

    public Vector3 GetSelectionContactPoint()
    {
        return _selectionPoint;
    }

    public void ShowTerrainVisualizer()
    {
        if (!_terrainVisualizerObj.activeSelf)
            _terrainVisualizerObj.SetActive(true);
    }

    public void HideTerrainVisualizer()
    {
        if (_terrainVisualizerObj.activeSelf)
            _terrainVisualizerObj.SetActive(false);
    }

    public void TriggerPlayerMoveCommand(Vector3 destination)
    {
        _playerActor.GetComponent<ActorBehaviour>().MoveActor(destination);
    }

    public void CaptureSelectionOnInput()
    {
        CaptureSelection();
    }



    //Debug
    public int LoggableID()
    {
        return GetInstanceID();
    }

    public string LoggableName()
    {
        return name;
    }


}



