using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IAbilityBehavior
{
    void RebuildAbility();
    void InitializeManipulator(ManipulatorController playerManipulator);


    string GetAbililtyName();
    string GetUiName();
    string GetUiDescription();


    bool IsObjectInRange(GameObject targetObject);
    void SetTargetLocation(Vector3 position);
    void ShowFieldOfEffect(Vector3 position);
    void HideFieldOfEffectRange();


    float GetCooldownDuration();
    void SetCooldownDuration(float newValue);


    bool IsAbilityReady();
    bool IsAbilityInProgress();
    bool IsAbilityCoolingDown();


    void TriggerAbility();
    void InterruptAbility();
}

public abstract class Ability : MonoBehaviour, IDebugLoggable, IAbilityBehavior
{
    //Declarations
    [Header("Settings")]
    [SerializeField] private string _abilityName = "Undefined Ability";
    [SerializeField] private string _uiName = "Undefined Ability";
    [SerializeField] private string _uiDescription = "Undefined Description";
    [SerializeField] private float _cooldownDuration;
    [SerializeField] private float _abilityRange;


    [Header("States")]
    [SerializeField] private bool _isReady = true;
    [SerializeField] private bool _isInProgress = false;
    [SerializeField] private bool _isCoolingDown = false;
    [SerializeField] private bool _isShowingVisualizer = false;
    [SerializeField] private Vector3 _targetLocation;

    [Header("References")]
    [SerializeField] private GameObject _abilityAreaVisualizerObject;
    [SerializeField] private ManipulatorController _manipulator;



    //Monobehaviours
    private void Update()
    {

        UpdateAbilityAreaVisulizerPosition();
    }


    //Internals
    private void UpdateAbilityAreaVisulizerPosition()
    {
        if (_isShowingVisualizer)
            SetTargetLocation(_manipulator.GetCurrentHoverContactPoint());
    } //For Abilities that require additional aiming

    protected virtual void EnterAbility()
    {
        _isReady = false;
        _isInProgress = true;
    }

    protected virtual void EndAbility()
    {
        _isInProgress = false;
        _isCoolingDown = true;
        Invoke(nameof(ReadyAbility), _cooldownDuration);
    }

    protected virtual void ReadyAbility()
    {
        _isCoolingDown = false;
        _isReady = true;
    }

    protected abstract void PerformConcreteAbilityLogic();

    protected abstract void PerformConcreteInterruption();




    //Externals
    public void InitializeManipulator(ManipulatorController playerManipulator)
    {
        _manipulator = playerManipulator;
    }

    public virtual void RebuildAbility() //use this to update the ability's value if an entity's stats change
    {
        throw new System.NotImplementedException();
    }


    public string GetAbililtyName()
    {
        return _abilityName;
    }
    public string GetUiName()
    {
        return _uiName;
    }
    public string GetUiDescription()
    {
        return _uiDescription;
    }



    public virtual void SetTargetLocation(Vector3 position)
    {
        //Convert the vector into local space
        _targetLocation = transform.InverseTransformVector(position);

        //Set the visualizer Object to anywhere within range
        if (Mathf.Abs(position.magnitude) <= _abilityRange)
            _abilityAreaVisualizerObject.transform.position = _targetLocation;

        //Or bound the visualizer within range
        else
        {
            Vector3 boundVector = _targetLocation.normalized * _abilityRange;
            _abilityAreaVisualizerObject.transform.position = boundVector;
        }
    }
    public abstract bool IsObjectInRange(GameObject targetObject);
    public virtual void ShowFieldOfEffect(Vector3 position)
    {
        if (!_isShowingVisualizer)
        {
            _isShowingVisualizer = true;
            _abilityAreaVisualizerObject.SetActive(true);
        }
    }
    public virtual void HideFieldOfEffectRange()
    {
        if (_isShowingVisualizer)
        {
            _isShowingVisualizer = false;
            _abilityAreaVisualizerObject.SetActive(false);
        }
    }



    public bool IsAbilityReady()
    {
        return _isReady;
    }
    public bool IsAbilityInProgress()
    {
        return _isInProgress;
    }
    public bool IsAbilityCoolingDown()
    {
        return _isCoolingDown;
    }



    public float GetCooldownDuration()
    {
        return _cooldownDuration;
    }
    public void SetCooldownDuration(float newValue)
    {
        _cooldownDuration = Mathf.Max(newValue, .1f);
    }



    public void TriggerAbility()
    {
        if (_isReady)
            PerformConcreteAbilityLogic();
    }

    public void InterruptAbility()
    {
        if (_isInProgress)
            PerformConcreteInterruption();
    }




    //Debugging
    public int LoggableID()
    {
        return GetInstanceID();
    }

    public string LoggableName()
    {
        return gameObject.name;
    }

}

public class AbilityManager : MonoBehaviour, IDebugLoggable
{
    //Declarations
    private ManipulatorController _manipulator;
    [SerializeField] private List<GameObject> _abilityObjectPrefabs;



    //Monobehaviours



    //Internals



    //Externals
    public GameObject CreateNewAbilityObject(string abilityName, Transform newParent)
    {
        foreach (GameObject abilityObject in _abilityObjectPrefabs)
        {
            if (abilityObject.GetComponent<IAbilityBehavior>().GetAbililtyName() == abilityName)
            {
                GameObject newAbilityObject = Instantiate(abilityObject, newParent, false);
                newAbilityObject.GetComponent<IAbilityBehavior>().InitializeManipulator(_manipulator);
            }
                
        }

        LogDebug.Error($"Ability '{abilityName}' doesn't exist. returning Null", this);
        return null;
    }





    //Debugging
    public int LoggableID()
    {
        return GetInstanceID();
    }

    public string LoggableName()
    {
        return gameObject.name;
    }


}

