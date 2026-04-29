using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDispatcher : MonoSingleton<InputDispatcher>
{
    public event Action OnUICancel;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;
    }


}
