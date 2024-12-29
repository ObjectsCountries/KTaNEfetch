using wawa.Modules;
using wawa.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class needytemplatescript : ModdedModule
{
    public KMSelectable thing;

    protected override void Awake()
    {
        Get<KMNeedyModule>().Set(
            onNeedyActivation: () => { OnNeedyActivation(); },
            onNeedyDeactivation: () => { OnNeedyDeactivation(); },
            onTimerExpired: () => { OnTimerExpired(); });
    }

    protected bool Solve()
    {
        GetComponent<KMNeedyModule>().OnPass();
        return false;
    }

    private void OnNeedyActivation()
    {

    }

    private void OnNeedyDeactivation()
    {

    }

    private void OnTimerExpired()
    {
        Strike("Time Up!");
    }
}
