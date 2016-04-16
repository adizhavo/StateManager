using System;
using UnityEngine;
using System.Collections;

public class SampleUIGameState : GameState
{
    [SerializeField] private GameObject Panel;

    protected override void EnableService(Action callback)
    {
        Panel.SetActive(true);
        base.Enable(callback);
    }

    protected override void DisableService(Action callback)
    {
        Panel.SetActive(false);
        base.Disable(callback);
    }
}
