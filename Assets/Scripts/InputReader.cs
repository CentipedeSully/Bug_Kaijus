using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class InputReader : MonoBehaviour, IDebugLoggable
{
    //Declarations
    [SerializeField] private float _inputCooldown = .1f;

    private bool _isSelectInputReady = true;
    private bool _isActionInputReady = true;
    private bool _isAuxInputReady = true;
    private bool _isBackInputReady = true;

    [SerializeField] private bool _showDebug = false;

    [Header("Selection Events")]
    [Space(5)]
    public UnityEvent OnSelectInput;
    public UnityEvent OnActionInput;
    public UnityEvent OnAuxInput;
    public UnityEvent OnBackInput;

    [Header("Manipulation Events")]
    [Space(5)]
    public UnityEvent<Vector2> OnDirectionalInput;
    public UnityEvent<int> OnZoomInput;
    public UnityEvent<int> OnCycleInput;




    //Monobehaviors



    //Internal
    private void CooldownSelectInput()
    {
        Invoke("ReadySelectInput", _inputCooldown);
    }

    private void CooldownActionInput()
    {
        Invoke("ReadyActionInput", _inputCooldown);
    }

    private void CooldownAuxInput()
    {
        Invoke("ReadyAuxInput", _inputCooldown);
    }

    private void CooldownBackInput()
    {
        Invoke("ReadyBackInput", _inputCooldown);
    }


    private void ReadySelectInput()
    {
        _isSelectInputReady = true;
    }

    private void ReadyActionInput()
    {
        _isActionInputReady = true;
    }

    private void ReadyAuxInput()
    {
        _isAuxInputReady = true;
    }

    private void ReadyBackInput()
    {
        _isBackInputReady = true;
    }



    //External
    public void DetectSelectionInput(InputAction.CallbackContext context)
    {
        if (_isSelectInputReady && context.phase == InputActionPhase.Performed)
        {
            CooldownSelectInput();
            OnSelectInput.Invoke();

            if(_showDebug)
                LogDebug.Log("Selection Input Detected",this);
        }
    }

    public void DetectActionInput(InputAction.CallbackContext context)
    {
        if (_isActionInputReady && context.phase == InputActionPhase.Performed)
        {
            CooldownActionInput();
            OnActionInput.Invoke();

            if (_showDebug)
                LogDebug.Log("Action Input Detected",this);
        }
    }

    public void DetectAuxillaryInput(InputAction.CallbackContext context)
    {
        if (_isAuxInputReady && context.phase == InputActionPhase.Performed)
        {
            CooldownAuxInput();
            OnAuxInput.Invoke();

            if (_showDebug)
                LogDebug.Log("Aux Input Detected", this);
        }
    }

    public void DetectBackInput(InputAction.CallbackContext context)
    {
        if (_isBackInputReady && context.phase == InputActionPhase.Performed)
        {
            CooldownBackInput();
            OnBackInput.Invoke();

            if (_showDebug)
                LogDebug.Log("Back Input Detected", this);
        }
    }


    public void DetectDirectionalInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.action.IsPressed())
        {
            Vector2 directionalData = context.ReadValue<Vector2>();
            OnDirectionalInput.Invoke(directionalData);

            if (_showDebug)
                LogDebug.Log($"Detected Directional Input: {directionalData}",this);
        }
    }

    public void DetectZoomInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.action.IsPressed())
        {
            //Mouse scroll is interpreted as the y value of a Vector2
            float rawZoomInput = context.ReadValue<Vector2>().y;

            //normalize the zoom input
            if (rawZoomInput > 0)
                rawZoomInput = 1;
            else if (rawZoomInput < 0)
                rawZoomInput = -1;

            int zoomInput = (int)rawZoomInput;

            OnZoomInput.Invoke((int)zoomInput);

            if (_showDebug)
                LogDebug.Log($"Detected Zoom Input: {zoomInput}", this);
        }
    }

    public void DetectCycleInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.action.IsPressed())
        {
            float cycleInput = context.ReadValue<float>();
            OnCycleInput.Invoke((int)cycleInput);

            if (_showDebug)
                LogDebug.Log($"Detected Cycle Input: {(int)cycleInput}", this);
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
