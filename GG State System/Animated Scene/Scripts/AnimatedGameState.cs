using System;
using UnityEngine;
using System.Collections;

public class AnimatedGameState : GameState
{
    [SerializeField] protected GameObject PaneToActivate;
    [SerializeField] protected AnimationTrigger AnimationController;

    private Action ManagerCallback;

    protected override void EnableService(Action callback)
    {
        base.Enable(null);
        PaneToActivate.SetActive(true);
        AnimationController.TriggerAnimations(callback);
    }

    protected override void DisableService(Action callback)
    {
        ManagerCallback = callback;
        AnimationController.TriggerAnimations(AnimationDeactiveCallback);
    }

    private void AnimationDeactiveCallback()
    {
        PaneToActivate.SetActive(false);
        base.Disable(ManagerCallback);
    }
}
