using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulatorController : MonoBehaviour, IDebugLoggable
{
    //Delcarations
    [Header("Manipulator Settings")]
    [Tooltip("A visualizer will be drawn at the contact point between the player's hovering mouse and the closest terrainLayer object")]
    [SerializeField] private LayerMask _terrainLayers;
    [SerializeField] [Min(0)] private float _maxCastDistance = 400;


    [Header("States, Utilities & References")]
    [SerializeField] private Vector2 _mouseScreenPosition;
    [SerializeField] private Vector3 _terrainContactPosition;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private CameraController _camController;

    [Header("Debug Settings")]
    [SerializeField] private bool _isDebugActive = false;
    [SerializeField] private GameObject _terrainVisualizerPrefab;
    private GameObject _terrainVisualizerObj;


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
        DrawMousePointerGizmos();
    }


    //Internal Utils
    private void CreateDebugGizmo()
    {
        if (_terrainVisualizerPrefab != null)
        {
            _terrainVisualizerObj = Instantiate(_terrainVisualizerPrefab);
            _terrainVisualizerObj.transform.SetParent(transform);
            _terrainVisualizerObj.transform.position = Vector3.zero;
        }
            
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

                //Get Camera Origin (world coords)
                _cameraOrigin = _camController.GetCameraPosition();

                //Get Camera's forwards direction (world coords)
                _cameraForwardsDirection = _camController.GetForwardCameraPerspectiveVector();

                //Calculate the mouse's relational screenToWorld position
                _mouseWorldPosition = _camController.GetWorldPositionFromScreenPoint(_mouseScreenPosition);

                //Get mouse contact point with any TerrainLayer objects
                _terrainContactPosition = CalculateClosestTerrainContact();

                //Move terrain visualizer to contact point
                _terrainVisualizerObj.transform.position = _terrainContactPosition;
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

    private Vector3 CalculateClosestTerrainContact()
    {
        Vector3 mouseDirection = (_mouseWorldPosition - _cameraOrigin).normalized;
        RaycastHit[] hits = Physics.RaycastAll(_cameraOrigin, mouseDirection, _maxCastDistance, _terrainLayers);

        if (hits.Length > 0)
        {
            //LogDebug.Log($"Closest Object Hit: {hits[hits.Length -1].transform.gameObject.name}, Location: {hits[hits.Length - 1].point}",this);
            return hits[hits.Length - 1].point;
        }
        else return Vector3.zero;
        
        

    }

    private void DrawMousePointerGizmos()
    {
        if (_isCameraGizmoDrawable)
        {
            Gizmos.color = _cameraProjectionGizmoColor;

            //Make sure we're  looking in the same direction as our camera
            Gizmos.DrawLine(_cameraOrigin, _cameraOrigin + (_cameraForwardsDirection * 200));



            Gizmos.color = _mousePositionGizmoColor;

            //Draw the mouse's relational position vector
            //Gizmos.DrawLine(_cameraOrigin, _mouseWorldPosition + (_cameraForwardsDirection * 200));
            Vector3 mouseDirection = (_mouseWorldPosition - _cameraOrigin).normalized;
            Gizmos.DrawLine(_cameraOrigin, _cameraOrigin + (mouseDirection * _maxCastDistance));
        }
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
