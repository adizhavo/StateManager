using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateManager : MonoBehaviour
{
    private static StateManager instance;

    public static StateManager Instance
    {
        get
        { 
            return instance;
        }
    }

    [SerializeField] protected StateRequest StartState;
    [SerializeField] protected StateMapper mapper;
    public StateMapper Mapper
    {
        get
        { 
            return mapper;
        }
    }
    [SerializeField] protected StateCommander CurrentCommander;
    protected StateCommander UnlockedState;
    protected StateCommander LockedState;

    protected virtual void Awake()
    {
        instance = this;
        InitializeCommander();
        SwitchMainState(StartState);
    }

    protected virtual void InitializeCommander()
    {
        UnlockedState = new UnclokedStateCommander();
        UnlockedState.InitializeCommander(this);
        LockedState = new LockedStateCommander();
        LockedState.InitializeCommander(this);
        CurrentCommander = UnlockedState;
    }

    public void RegisterStates(GameState gameState)
    {
        CurrentCommander.RegisterStates(gameState);
    }

    public void SwitchMainState(StateRequestContainer container)
    {
        SwitchMainState(container.GetStateRequest());
    }

    public void StartChildState(StateRequestContainer paralReqState)
    {
        StartChildState(paralReqState.GetStateRequest());
    }

    public void KillChildState(StateRequestContainer paralReqState)
    {
        KillChildState(paralReqState.GetStateRequest());
    }

    public void SwitchMainState(StateRequest reqState)
    {
        ChangeManagerInternalState();
        CurrentCommander.SwitchMainState(reqState);
    }

    public void StartChildState(StateRequest paralReqState)
    {
        ChangeManagerInternalState();
        CurrentCommander.SwitchMainState(paralReqState);
    }

    public void KillChildState(StateRequest paralReqState)
    {
        ChangeManagerInternalState();
        CurrentCommander.KillChildState(paralReqState);
    }

    public virtual void StateManagerProcessCleanup()
    {
        ChangeManagerInternalState();
        CurrentCommander.CheckBufferLoad();
    }

    public virtual void ChangeManagerInternalState()
    {
        CurrentCommander = (CurrentCommander.GetType().Equals(typeof(UnclokedStateCommander))) ? LockedState : UnlockedState;
    }
}

public abstract class StateCommander
{
    protected StateManager manager;
    protected StatetHistory overallStateHistory;
    protected StatetHistory mainStateHistory;
    protected StatetHistory childStateHistory;

    protected StateBuffer mainStateBuffer;
    protected StateBuffer childStateBuffer;

    protected object lockObject = new object();

    public void InitializeCommander(StateManager manager)
    {
        this.manager = manager;
        overallStateHistory = new StatetHistory();
        mainStateHistory = new StatetHistory();
        childStateHistory = new StatetHistory();
    }

    public void RegisterStates(GameState gameState)
    {
        manager.Mapper.RegisterState(gameState);
    }

    public void SwitchMainState(StateRequest reqState)
    {
        CheckReceivedSwitchRequest(reqState, ProcessRequestedMainState);
    }

    public void StartChildState(StateRequest reqState)
    {
        CheckReceivedSwitchRequest(reqState, ProcessRequestedChildState);
    }

    public void KillChildState(StateRequest paralReqState)
    {
        CheckReceivedSwitchRequest(paralReqState, KillChildGameState);
    }

    protected StateRequest cachedRequest;

    protected void CheckReceivedSwitchRequest(StateRequest reqState, Action<GameState> processReqState)
    {
        // Thread Lock
        lock (lockObject)
        {
            cachedRequest = reqState;
            GameState reqGameState = manager.Mapper.GetStateWithEnum(reqState.GetStateRequested());
            if (reqGameState == null)
            {
                Debug.LogError("[State Manager] State requested is not registered! Request Type : " + reqState.GetStateRequested().ToString());
                manager.ChangeManagerInternalState();
            }
            else
            {
                processReqState(reqGameState);
            }
        }
    }

    protected abstract void ProcessRequestedMainState(GameState reqGameState);

    protected abstract void ProcessRequestedChildState(GameState reqGameState);

    protected abstract void KillChildGameState(GameState reqGameState);

    public abstract void CheckBufferLoad();
}

public class UnclokedStateCommander : StateCommander
{
    private GameState reqCachedReqState;

    protected override void KillChildGameState(GameState reqChildState)
    {
        reqChildState.Disable(manager.StateManagerProcessCleanup);
    }

    protected override void ProcessRequestedMainState(GameState reqGameState)
    {
        ExecuteTheRequest(reqGameState);
        if (mainStateHistory != null)
            mainStateHistory.Save(reqGameState);
    }

    protected override void ProcessRequestedChildState(GameState reqGameState)
    {
        ExecuteChildState(reqGameState);
        if (childStateHistory != null)
            childStateHistory.Save(reqGameState);
    }

    private void ExecuteTheRequest(GameState reqGameState)
    {
        //
        // Here Can be process a saved state from cache or file ( Derin DB idea )
        //
        reqCachedReqState = reqGameState;
        overallStateHistory.Save(reqGameState);
        ClearChildStates();
    }

    private void ClearChildStates()
    {
        if (childStateHistory.HasSavedStates())
        {
            ClearCurrentStateInHistory(childStateHistory, ClearChildStates);
        }
        else
        {
            ClearCurrentStateInHistory(mainStateHistory, ActivateCachedAsMainState);
        }
    }

    private void ClearCurrentStateInHistory(StatetHistory history, Action Callback)
    {
        GameState current = history.GetCurrentState();
        current.Disable(Callback);
        mainStateHistory.RemoveCurrentState();
    }

    private void ActivatePreviousParallel()
    {
        GameState stateToSave = (childStateHistory.HasPreviousState()) ? childStateHistory.GetPreviousState() : mainStateHistory.GetCurrentState();
        childStateHistory.RemoveCurrentState();
        overallStateHistory.Save(stateToSave);
        manager.StateManagerProcessCleanup();
    }

    private void ExecuteChildState(GameState reqChildState)
    {
        //
        // Here Can be process a saved state from cache or file (Derind DB idea)
        //
        overallStateHistory.Save(reqChildState);
        reqChildState.Enable(manager.StateManagerProcessCleanup);
    }

    private void ActivateCachedAsMainState()
    {
        reqCachedReqState.Enable(manager.StateManagerProcessCleanup);
    }

    public override void CheckBufferLoad()
    {
        if (childStateBuffer.HasBufferData())
        {
            StartChildState(childStateBuffer.DequeueNextState());
        }
        else if (mainStateBuffer.HasBufferData())
        {
            SwitchMainState(mainStateBuffer.DequeueNextState());
        }
    }
}

public class LockedStateCommander : StateCommander
{

    protected override void ProcessRequestedMainState(GameState reqGameState)
    {
        SaveToBuffer(mainStateBuffer);
    }

    protected override void ProcessRequestedChildState(GameState reqGameState)
    {
        SaveToBuffer(childStateBuffer);
    }

    protected override void KillChildGameState(GameState requestedparallelGameState)
    {
        Debug.Log("[Command Buffer] You cannot kill, wait for the manager to be active");
    }

    public override void CheckBufferLoad()
    {
        Debug.Log("[Command Buffer] You cannot access the buffer, wait for the manager to be active");
    }

    private void SaveToBuffer(StateBuffer stateBuffer)
    {
        if (stateBuffer != null)
            stateBuffer.SaveState(cachedRequest);
    }
}

[System.Serializable]
public class StateMapper
{
    [SerializeField] protected List<GameState> savedStates = new List<GameState>();

    public void RegisterState(GameState gameState)
    {
        if (!savedStates.Contains(gameState))
            savedStates.Add(gameState);
    }

    public GameState GetStateWithEnum(States GameStateType)
    {
        for (int i = 0; i < savedStates.Count; i++)
        {
            if (GameStateType.Equals(savedStates[i].GetGameStateType()))
            {
                return savedStates[i];
            }
        }

        return null;
    }
}

[System.Serializable]
public class StatetHistory
{
    [SerializeField] protected List<GameState> savedHistoryStates = new List<GameState>();

    public void Save(GameState stateToSave)
    {
        savedHistoryStates.Add(stateToSave);
    }

    public GameState GetPreviousState()
    {
        if (savedHistoryStates.Count > 1)
        {
            return savedHistoryStates[savedHistoryStates.Count - 2];
        }
        return null;
    }

    public void RemovePrevioustState()
    {
        if (savedHistoryStates.Count > 1)
        {
            savedHistoryStates.RemoveAt(savedHistoryStates.Count - 2);
        }
    }

    public GameState GetCurrentState()
    {
        if (savedHistoryStates.Count > 0)
        {
            return savedHistoryStates[savedHistoryStates.Count - 1];
        }
        return null;
    }

    public void RemoveCurrentState()
    {
        if (savedHistoryStates.Count > 0)
        {
            savedHistoryStates.RemoveAt(savedHistoryStates.Count - 1);
        }
    }

    public virtual bool HasSavedStates()
    {
        return savedHistoryStates.Count > 0;
    }

    public virtual bool HasPreviousState()
    {
        return savedHistoryStates.Count > 1;
    }

    public void PrintAllSavedHistory()
    {
        for (int i = 0; i < savedHistoryStates.Count; i++)
        {
            Debug.Log("[State History] state no: " + (i + 1) + " is: " + savedHistoryStates[i].GetGameStateType().ToString());
        }
    }
}

[System.Serializable]
public class ActiveStateHistory : StatetHistory
{
    public override bool HasPreviousState()
    {
        CleanDisabledStates();
        return base.HasPreviousState();
    }

    public override bool HasSavedStates()
    {
        CleanDisabledStates();
        return base.HasPreviousState();
    }

    protected virtual void CleanDisabledStates()
    {
        for (int i = 0; i < savedHistoryStates.Count; i++)
        {
            if (savedHistoryStates[i].IsGameStateDisabled())
            {
                savedHistoryStates.RemoveAt(i);
            }
        }
    }
}

[System.Serializable]
public class StateBuffer
{
    [SerializeField] protected Queue<StateRequest> bufferedStates = new Queue<StateRequest>();

    public void SaveState(StateRequest state)
    {
        bufferedStates.Enqueue(state);

        #if UNITY_EDITOR
        Debug.Log("[State Buffer] State saved: " + state.GetStateRequested().ToString());
        #endif
    }

    public StateRequest DequeueNextState()
    {
        return bufferedStates.Dequeue();
    }

    public bool HasBufferData()
    {
        return bufferedStates.Count > 0;
    }
}

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

    protected abstract void EnableService(Action ManagerControl);

    protected abstract void DisableService(Action ManagerControl);
}