using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractible
{
    GameObject GetGameObject();

    void OnHoverEnter();

    void OnHoverExit();

    void OnSelect();

    void OnDeselect();

    void OnContextAction();

    void OnAuillaryClick();
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
    [SerializeField] private GameObject _closestDetection;
    [SerializeField] private Vector3 _pointOfHoveringContact;
    [SerializeField] private GameObject _lastObjectHovered;
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

                //Get Camera world coords & forwards direction
                _cameraOrigin = _camController.GetCameraPosition();
                _cameraForwardsDirection = _camController.GetForwardCameraPerspectiveVector();

                //Get worldMousePosition
                _mouseWorldPosition = _camController.GetWorldPositionFromScreenPoint(_mouseScreenPosition);

                //Capture the closest interactible that our mouse is currently hovering over
                DetectClosestInteractible();

                //Move terrain visualizer to contact point
                _terrainVisualizerObj.transform.position = _pointOfHoveringContact;
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

    private void DetectClosestInteractible()
    {
        Vector3 mouseDirection = (_mouseWorldPosition - _cameraOrigin).normalized;
        RaycastHit[] hits = Physics.RaycastAll(_cameraOrigin, mouseDirection, _maxCastDistance, _interactiblesLayer);

        if (hits.Length > 0)
        {
            GameObject detectedObject = hits[hits.Length - 1].transform.gameObject;

            //save the contact point
            _pointOfHoveringContact = hits[hits.Length - 1].point;

            // Trigger OnHover Enter/Exit functions if applicable
            if (_lastObjectHovered == null)
            {
                //save the new valid hovered object, and enter its hovered state
                _lastObjectHovered = detectedObject;
                _lastObjectHovered.GetComponent<IInteractible>().OnHoverEnter();
            }
            else if (_lastObjectHovered != detectedObject)
            {
                //update the previously hovered object as no longer being hovered over
                _lastObjectHovered.GetComponent<IInteractible>().OnHoverExit();

                //save the new valid hovered object, and enter its hovered state
                _lastObjectHovered = detectedObject;
                _lastObjectHovered.GetComponent<IInteractible>().OnHoverEnter();

            }

            //save the detected game object
            _closestDetection = hits[hits.Length - 1].transform.gameObject;
        }

        else if (_lastObjectHovered != null)
        {
            //Trigger the onHoverExit function on the last object and forget it
            _lastObjectHovered.GetComponent<IInteractible>().OnHoverExit();
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



    //External Utils
    public void SetTerrainVisualizerPosition(Vector3 newPosition)
    {
        _terrainVisualizerObj.transform.position = newPosition;
    }

    public Vector3 GetCurrentHoverContactPoint()
    {
        return _pointOfHoveringContact;
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



