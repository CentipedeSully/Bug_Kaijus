using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour, IDebugLoggable
{
    //Declarations
    [SerializeField] private bool _isFreeLookActive = false;
    [SerializeField] private Vector3 _defaultFreeLookPosition;
    [Tooltip("This is what the literalFocusObject is currently following. Empty means we're in freeLook mode")]
    [SerializeField] private GameObject _currentCameraFocus;
    [Tooltip("The vcam is bound to this object at all times. This literal object will get moved and parented to other objects to emulate changes in focus")]
    [SerializeField] private GameObject _literalFocusObject;
    [SerializeField] private CinemachineVirtualCamera _currentCamera;
    [SerializeField] private int _panSpeed;
    [SerializeField] private int _zoomSpeed;
    [SerializeField] private int _orbitSpeed;

    [Header("Debug Utils")]
    [SerializeField] private bool _isDebugActive;
    [SerializeField] private bool _DEBUG_setTargetAsFocus;
    [SerializeField] private GameObject _DEBUG_focusTarget;
    [SerializeField] private bool _DEBUG_enterFreeLook;



    //Monobehaviours
    private void Start()
    {
        EnterFreeLook();
    }

    private void Update()
    {
        if (_isDebugActive) 
            ListenForDebugCommands();
    }

    //Internal Utils
    private void SetupNewLiteralFocus()
    {
        //Create a new object as out literal focus
        _literalFocusObject = Instantiate(new GameObject("Literal Camera Object (Regenerated)"));

        //Set the object to the default position.
        _literalFocusObject.transform.position = _defaultFreeLookPosition;

        //It belongs to the camera controller
        _literalFocusObject.transform.SetParent(this.transform, false);

        //Update the vCam and this object as the new camera's focus
        _currentCamera.LookAt = _literalFocusObject.transform;
        _currentCamera.Follow = _literalFocusObject.transform;

    }


    //External Utils
    public void SetCamera(CinemachineVirtualCamera newCamera)
    {
        if (newCamera != null && newCamera != _currentCamera)
        {
            _currentCamera = newCamera;
        }
    }

    public CinemachineVirtualCamera GetCamera()
    {
        return _currentCamera;
    }

    public void SetCameraFocus(GameObject newSubject)
    {
        if (_literalFocusObject == null)
            SetupNewLiteralFocus();

        if (newSubject != null)
        {
            if (_isFreeLookActive)
            {
                //Exit FreeLook Mode
                _isFreeLookActive = false;
            }

            //Set the newSubject as the literalFocusObject's parent
            _literalFocusObject.transform.SetParent(newSubject.transform, false);
            _literalFocusObject.transform.localPosition = Vector3.zero;

        }
    }

    public void SetOrbitSpeed(int newValue)
    {
        if (_currentCamera != null)
        {
            newValue = Mathf.Max(newValue, 0);
            _orbitSpeed = newValue;
            _currentCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.m_MaxSpeed = _orbitSpeed;
        }
        
    }

    public void SetZoomSpeed(int newValue)
    {
        if (_currentCamera != null)
        {
            newValue = Mathf.Max(newValue, 0);
            _zoomSpeed = newValue;
        }
    }

    public void SetPanSpeed(int newValue)
    {
        if (_currentCamera != null)
        {
            newValue = Mathf.Max(newValue, 0);
            _panSpeed = newValue;
        }
    }

    public void ZoomOnInput(int zoomDirection)
    {
        Vector3 currentOrbitalDistance = _currentCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset;

        //invert the zoom direction for better feel
        float offset = -1 * zoomDirection * _zoomSpeed * Time.deltaTime;
        Vector3 zoomOffset = new(offset, offset, offset);

        Vector3 newOrbitalDistance = currentOrbitalDistance + zoomOffset;

        _currentCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset = newOrbitalDistance;
    }


    public void PanOnInput(Vector2 direction)
    {
        //Detach the focus object from it's parent, if we're not in freelook


        //Move the camera
    }

    public void EnterFreeLook()
    {
        if (!_isFreeLookActive)
        {
            _isFreeLookActive = true;

            //Did our literalFocusObject accidentally get destroyed?
            if (_literalFocusObject == null)
                SetupNewLiteralFocus();

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
