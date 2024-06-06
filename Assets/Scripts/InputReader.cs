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

    [Header("Input Events")]
    [Space(5)]
    public UnityEvent OnSelectInput;
    public UnityEvent OnActionInput;
    public UnityEvent OnAuxInput;
    public UnityEvent OnBackInput;




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
    public void ReadSelectionInput(InputAction.CallbackContext context)
    {
        if (_isSelectInputReady && context.phase == InputActionPhase.Performed)
        {
            CooldownSelectInput();
            OnSelectInput.Invoke();

            if(_showDebug)
                LogDebug.Log("Selection Input Detected",this);
        }
    }

    public void ReadActionInput(InputAction.CallbackContext context)
    {
        if (_isActionInputReady && context.phase == InputActionPhase.Performed)
        {
            CooldownActionInput();
            OnActionInput.Invoke();

            if (_showDebug)
                LogDebug.Log("Action Input Detected",this);
        }
    }

    public void ReadAuxillaryInput(InputAction.CallbackContext context)
    {
        if (_isAuxInputReady && context.phase == InputActionPhase.Performed)
        {
            CooldownAuxInput();
            OnAuxInput.Invoke();

            if (_showDebug)
                LogDebug.Log("Aux Input Detected", this);
        }
    }

    public void ReadBackInput(InputAction.CallbackContext context)
    {
        if (_isBackInputReady && context.phase == InputActionPhase.Performed)
        {
            CooldownBackInput();
            OnBackInput.Invoke();

            if (_showDebug)
                LogDebug.Log("Back Input Detected", this);
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
