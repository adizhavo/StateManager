using System;
using UnityEngine;
using StateSystem;

public abstract class GameState : MonoBehaviour
{
    [SerializeField] protected States GameStateType;

    private enum State
    {
        Enabled,
        Disabled
    };

    private State CurrentState = State.Disabled;

    public virtual void RegisterToManager()
    {
        StateManager.Instance.RegisterStates(this);
    }

    internal void Enable(Action callback)
    {
        EnableService(callback);
        ChangeInternalGameState();
        #if UNITY_EDITOR
        Debug.Log("[Game State] State Active " + GetGameStateType());
        #endif
    }

    internal void Disable(Action callback)
    {
        DisableService(callback);
        ChangeInternalGameState();
        #if UNITY_EDITOR
        Debug.Log("[Game State] State Deactivate " + GetGameStateType());
        #endif
    }

    internal void ChangeInternalGameState()
    {
        CurrentState = (CurrentState.Equals(State.Disabled)) ? State.Enabled : State.Disabled;
    }

    public bool IsGameStateDisabled()
    {
        return CurrentState.Equals(State.Disabled);
    }

    public virtual States GetGameStateType()
    {
        return GameStateType;
    }

    protected abstract void EnableService(Action CommanderControl);

    protected abstract void DisableService(Action CommanderControl);
}