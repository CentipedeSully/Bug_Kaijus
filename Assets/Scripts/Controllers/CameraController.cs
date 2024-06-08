using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour, IDebugLoggable
{
    //Declarations
    [Header("Camera Settings")]
    [SerializeField] private int _cameraMoveSpeed;
    private Vector2 _moveInput;
    //create utils to bound panning-- to Stop player from moving camera beyond gameSpace

    [SerializeField] private int _zoomSpeed;
    private int _zoomInput;

    [SerializeField] private float _maxZoomDistance;
    [SerializeField] private float _minZoomDistance;

    [SerializeField] private int _orbitSpeed;

    [Header("Camera Utilities, States & References")]
    [SerializeField] private bool _isFreeLookActive = true;

    [Tooltip(
        "This is where the camera's focus will land when the camera enters freeLook mode. " +
        "This value changes whenever the camera transitions from Focused to FreeLook")]
    [SerializeField] private Vector3 _defaultFreeLookPosition;

    [Tooltip("This is what the literalFocusObject is currently following. Empty means we're in freeLook mode")]
    [SerializeField] private GameObject _currentCameraFocus;

    [Tooltip("This is our true Follow & LookAt object. It parents to other gameObjects to emulate direct focusing")]
    [SerializeField] private GameObject _literalCamFocusPrefab;
    private GameObject _literalFocusObject;

    [SerializeField] private CinemachineVirtualCamera _mapCamera;


    [Header("Debug Utils")]
    [SerializeField] private bool _isDebugActive;
    [SerializeField] private bool _DEBUG_setTargetAsFocus;
    [SerializeField] private GameObject _DEBUG_focusTarget;
    [SerializeField] private bool _DEBUG_enterFreeLook;



    //Monobehaviours
    private void Start()
    {
        InitializeIntoFreeLookMode();
    }

    private void Update()
    {
        DefaultToFreeLookOnNullCameraFocus();
        MoveCameraOnInput();
        ZoomCameraOnInput();
        UpdateDefaultFreeLookPosition();

        if (_isDebugActive) 
            ListenForDebugCommands();
    }



    //Internal Utils
    private void InitializeIntoFreeLookMode()
    {
        //Hard reset into FreeLook to to start from a clean slate
        _isFreeLookActive = false;
        EnterFreeLook();
    }

    private void SetupNewLiteralFocus()
    {
        if (_literalFocusObject == null)
        {
            //Create a new object as out literal focus
            _literalFocusObject = Instantiate(_literalCamFocusPrefab);

            //Set the object to the default position.
            _literalFocusObject.transform.position = _defaultFreeLookPosition;

            //It belongs to the camera controller
            _literalFocusObject.transform.SetParent(this.transform, false);

            //Update the vCam and this object as the new camera's focus
            _mapCamera.LookAt = _literalFocusObject.transform;
            _mapCamera.Follow = _literalFocusObject.transform;
        }
    }

    private void MoveCameraOnInput()
    {
        if (_moveInput.magnitude > 0)
        {
            //Detach the focus object from it's parent, if we're not in freelook
            EnterFreeLook();

            //create the displacement vector. We're only moving the camera's focus object over the xz plane
            Vector3 displacement = new(_moveInput.x * _cameraMoveSpeed * Time.deltaTime, 0, _moveInput.y * _cameraMoveSpeed * Time.deltaTime);

            //Get the focus's current position
            Vector3 currentFocusPosition = _literalFocusObject.transform.position;

            //Calculate the displacement vector RELATIONAL TO THE PLAYER'S VIEWPORT
            Vector3 relationalDisplacement = _mapCamera.transform.TransformDirection(displacement);

            //Zero the new y displacement
            relationalDisplacement = new(relationalDisplacement.x, 0, relationalDisplacement.z);

            //Move the focus object by the relationalDisplacement vector
            _literalFocusObject.transform.position = currentFocusPosition + relationalDisplacement;
        }
    }

    private void ZoomCameraOnInput()
    {
        if (_zoomInput != 0)
        {
            Vector3 currentOrbitalDistance = _mapCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset;

            //invert the zoom direction for better feel
            float offset = -1 * _zoomInput * _zoomSpeed * Time.deltaTime;
            Vector3 zoomOffset = new(offset, offset, offset);

            Vector3 newOrbitalDistance = currentOrbitalDistance + zoomOffset;

            //Clamp the zooming
            float xDistanceClamped = Mathf.Clamp(newOrbitalDistance.x, _minZoomDistance, _maxZoomDistance);
            float yDistanceClamped = Mathf.Clamp(newOrbitalDistance.y, _minZoomDistance, _maxZoomDistance);
            float zDistanceClamped = Mathf.Clamp(newOrbitalDistance.z, _minZoomDistance, _maxZoomDistance);

            Vector3 newClampedOrbitalDistance = new(xDistanceClamped, yDistanceClamped, zDistanceClamped);


            //Apply the new Distance to the camera
            _mapCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset = newClampedOrbitalDistance;
        }
    }

    private void UpdateDefaultFreeLookPosition()
    {
        //Only update the freelook position if we're NOT in freelook (& our focus object exists)
        if (!_isFreeLookActive && _currentCameraFocus != null)
        {
            //Update the default freeLook position to the last-focused object's position
            _defaultFreeLookPosition = _currentCameraFocus.transform.position;
        }
        
    }

    private void DefaultToFreeLookOnNullCameraFocus()
    {
        //enter freeLook if our camera focus object went missing unexpectedly
        if (!_isFreeLookActive && _currentCameraFocus == null)
            EnterFreeLook();
    }



    //External Utils
    public void SetCameraFocus(GameObject newSubject)
    {
        if (newSubject != null)
        {
            //Create a new literal focus if we don't have one for some reason (accidental deletion?)
            if (_literalFocusObject == null)
                SetupNewLiteralFocus();

            //Exit FreeLook Mode
            if (_isFreeLookActive)
                _isFreeLookActive = false;

            //update the currentFocus Util
            _currentCameraFocus = newSubject;

            //Set the new current focus as the literalFocusObject's parent
            _literalFocusObject.transform.SetParent(_currentCameraFocus.transform, false);
            _literalFocusObject.transform.localPosition = Vector3.zero;

        }
    }



    public void SetOrbitSpeed(int newValue)
    {
        if (_mapCamera != null)
        {
            newValue = Mathf.Max(newValue, 0);
            _orbitSpeed = newValue;
            _mapCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.m_MaxSpeed = _orbitSpeed;
        }
        
    }

    public void SetZoomSpeed(int newValue)
    {
        if (_mapCamera != null)
        {
            newValue = Mathf.Max(newValue, 0);
            _zoomSpeed = newValue;
        }
    }

    public void SetViewMovementSpeed(int newValue)
    {
        if (_mapCamera != null)
        {
            newValue = Mathf.Max(newValue, 0);
            _cameraMoveSpeed = newValue;
        }
    }



    

    public void ReadZoomInput(int zoomDirection)
    {
        _zoomInput = zoomDirection;
    }

    public void ReadMoveInput(Vector2 direction)
    {
        _moveInput = direction;
    }



    public void EnterFreeLook()
    {
        if (!_isFreeLookActive)
        {
            _isFreeLookActive = true;

            //Clear the currentCameraFocus utility
            _currentCameraFocus = null;

            //Did our literalFocusObject accidentally get destroyed?
            if (_literalFocusObject == null)
                SetupNewLiteralFocus();

            //Only take it back (reparent) if it didn't get destroyed
            else
            {
                //Return the camera focus object to this camera controller
                _literalFocusObject.transform.SetParent(transform, true);
            }
            

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

    private void ListenForDebugCommands()
    {
        if (_DEBUG_enterFreeLook)
        {
            _DEBUG_enterFreeLook = false;

            LogDebug.Log("Attempting to enter FreeLook...", this);
            EnterFreeLook();
            LogDebug.Log("Entering FreeLook Complete", this);
        }

        if (_DEBUG_setTargetAsFocus)
        {
            _DEBUG_setTargetAsFocus = false;

            LogDebug.Log($"Attempting to set {_DEBUG_focusTarget} as focus Target...");
            SetCameraFocus(_DEBUG_focusTarget);
            LogDebug.Log("Setting Focus Target Complete", this);
        }
    }
}
