using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFocusDebugGizmo : MonoBehaviour
{
    //Declarations
    [SerializeField] private Color _gizmoColor = Color.blue;
    [SerializeField] private float _gizmoSize = .2f;


    //Monobehaviors
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _gizmoColor;

        Gizmos.DrawWireSphere(transform.position, _gizmoSize);
    }


    //Internals



    //Externals



    //Debugging



}
