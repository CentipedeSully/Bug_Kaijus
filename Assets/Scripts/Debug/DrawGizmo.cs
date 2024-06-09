using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGizmo : MonoBehaviour
{
    //Declarations
    [SerializeField] private bool _cubeGizmo = false;
    [SerializeField] private bool _sphereGizmo = false;
    [SerializeField] private bool _lineGizmo = false;
    [SerializeField] private Color _gizmoColor = Color.blue;
    [SerializeField] private float _gizmoSize = .2f;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private Vector3 _lineDirection = Vector3.up;



    //Monobehaviors
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _gizmoColor;
        DrawCubeGizmo();
        DrawSphereGizmo();
        DrawLineGizmo();
    }


    //Internals
    private void DrawSphereGizmo()
    {
        if (_sphereGizmo)
            Gizmos.DrawWireSphere(transform.position + _offset, _gizmoSize);
    }

    private void DrawCubeGizmo()
    {
        if (_cubeGizmo)
            Gizmos.DrawWireCube(transform.position + _offset, Vector3.one * _gizmoSize);
    }

    private void DrawLineGizmo()
    {
        if (_lineGizmo)
        {
            Vector3 drawOrigin = transform.position + _offset;
            Vector3 drawDirection = _lineDirection * _gizmoSize;

            Gizmos.DrawLine(drawOrigin, drawOrigin + drawDirection);
        }
            
    }


    //Externals



    //Debugging



}
