using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulatorController : MonoBehaviour, IDebugLoggable
{
    //Delcarations
    [SerializeField] private Vector2 _mouseScreenPosition;


    [Header("Utilities & References")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private CameraController _camController;

    [Header("Debug Settings")]
    [SerializeField] private bool _isDebugActive = false;
    [SerializeField] private GameObject _debugGizmoPrefab;
    private GameObject _debugGizmoObj;


    private bool _isCameraGizmoDrawable = false;

    private Vector3 _cameraOrigin;
    private Vector3 _cameraForwardsDirection;
    [SerializeField] private Color _cameraProjectionGizmoColor = Color.magenta;

    private Vector3 _mouseWorldPosition;
    [SerializeField] private Color _mousePositionGizmoColor = Color.green;

    




    //Monobehaviours
    private void Start()
    {
        CreateDebugGizmo();
    }

    private void Update()
    {
        TrackMousePointer();
    }

    private void OnDrawGizmosSelected()
    {
        if (_isCameraGizmoDrawable)
        {
            Gizmos.color = _cameraProjectionGizmoColor;

            //Make sure we're  looking in the same direction as our camera
            Gizmos.DrawLine(_cameraOrigin, _cameraOrigin + (_cameraForwardsDirection * 200));



            Gizmos.color = _mousePositionGizmoColor;

            //Draw the mouse's relational position vector
            Gizmos.DrawLine(_cameraOrigin, _mouseWorldPosition + (_cameraForwardsDirection * 200));
        }
    }


    //Internal Utils
    private void CreateDebugGizmo()
    {
        if (_debugGizmoPrefab != null)
        {
            _debugGizmoObj = Instantiate(_debugGizmoPrefab);
            _debugGizmoObj.transform.SetParent(transform);
            _debugGizmoObj.transform.position = Vector3.zero;
        }
            
    }

    private void TrackMousePointer()
    {
        //Validate the existence of our tools
        if (_inputReader != null && _camController != null)
        {
            //Get Mouse position
            _mouseScreenPosition = _inputReader.GetMousePositionOnScreen();

            //Ignore invalid mouse positions
            if (_mouseScreenPosition.magnitude < 0)
            {
                LogDebug.Warn("Invalid  mouse position detected. Manipulator standing by for valid mouse position", this);
                
                //Stop drawing gizmos
                _isCameraGizmoDrawable = false;
            }


            else
            {
                BuildDebuggingViewportGizmo();
                /*
                LogDebug.Log(
                    $"Camera World Coords: {_camController.GetCameraPosition()}\n" + 
                    $"Cam FORWARDS Vector [ fromLocal(0,0,1) to World (?,?,?) ]: {_camController.GetForwardCameraPerspectiveVector()}"
                    );
                */

            }

        }

        //Stop drawing gizmos. Our utilities are probably out of date.
        else
            _isCameraGizmoDrawable = false;


    }

    private void BuildDebuggingViewportGizmo()
    {
        //enable gizmo visibility
        _isCameraGizmoDrawable = true;

        //Get Camera Origin (world coords)
        _cameraOrigin = _camController.GetCameraPosition();

        //Get Camera's forwards direction (world coords)
        _cameraForwardsDirection = _camController.GetForwardCameraPerspectiveVector();

        //Calculate the mouse's relational screenToWorld position
        _mouseWorldPosition = _camController.GetWorldPositionFromScreenPoint(_mouseScreenPosition);

    }





    //External Utils




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
