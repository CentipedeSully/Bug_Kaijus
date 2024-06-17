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
        if (IsAbilityInProgress())
        {
            LogDebug.Log($"Ability {this.name} interrupted!",this);
            EndAbility();
        }
    }

    public override void PerformAbility()
    {
        EnterAbility();
        LogDebug.Log("Melee Atk Triggered", this);
        EndAbility();
    }



    //Debugging

}
