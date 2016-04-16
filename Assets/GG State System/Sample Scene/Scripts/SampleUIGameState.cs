using System;
using UnityEngine;
using System.Collections;

public class SampleUIGameState : GameState
{
    [SerializeField] private GameObject Panel;

    protected override void EnableService(Action CommanderControl)
    {
        Panel.SetActive(true);
        CommanderControl();
    }

    protected override void DisableService(Action CommanderControl)
    {
        Panel.SetActive(false);
        CommanderControl();;
    }
}
