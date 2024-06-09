using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulatorController : MonoBehaviour
{
    //Delcarations
    [SerializeField] private Vector2 _mouseScreenPosition;


    [Header("Utilities & References")]
    [SerializeField] private InputReader _inputReader;

    [Header("Debug Settings")]
    [SerializeField] private bool _isDebugActive = false;
    [SerializeField] private GameObject _debugGizmoPrefab;
    private GameObject _debugGizmoObj;

    




    //Monobehaviours
    private void Start()
    {
        CreateDebugGizmo();
    }

    private void Update()
    {
        if (_inputReader != null)
            _mouseScreenPosition = _inputReader.GetMousePositionOnScreen();
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

    private void CalculateGroundPointLocationFromMouse()
    {

    }



    //External Utils




    //Debug



}
