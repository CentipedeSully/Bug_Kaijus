using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMelee : Ability
{
    //Declarations





    //Monobehaviours




    //Internals




    //Externals
    public override void InterruptAbility()
    {

    }

    public override void StartAbility()
    {
        EnterAbility();
        LogDebug.Log("Melee Atk Triggered", this);
        EndAbility();
    }



    //Debugging

}
