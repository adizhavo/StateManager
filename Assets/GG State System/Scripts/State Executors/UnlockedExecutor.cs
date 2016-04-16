using System;

namespace StateSystem
{
    public class UnclokedExecutor : IStateExecutor
    {
        protected StateCommander commander;
        protected GameState cachedReqState;

        public UnclokedExecutor(StateCommander commander)
        {
            this.commander = commander;
        }

        public void ProcessRequestedMainState(GameState reqGameState)
        {
            ChangeExecutor();
            ExecuteMainRequest(reqGameState);
        }

        public void ProcessRequestedChildState(GameState reqChildState)
        {
            ChangeExecutor();
            ExecuteChildRequest(reqChildState);
        }

        public void KillChildGameState(GameState reqChildState)
        {
            ChangeExecutor();
            reqChildState.Disable(ExecutionerCleanup);
        }
        // Changes states of the Executor to store incoming requests
        protected virtual void ChangeExecutor()
        {
            commander.SetExecutor(new LockedExecutor(commander));
        }

        private void ExecuteMainRequest(GameState reqGameState)
        {
            cachedReqState = reqGameState;
            ClearChildStates();
        }

        private void ClearChildStates()
        {
            // First clear all the active child states
            if (commander.ChildStateHistory.HasSavedStates())
            { 
                ClearCurrentStateInHistory(commander.ChildStateHistory, ClearChildStates);
            }
            // Clear the current active main state
            else if (commander.MainStateHistory.HasSavedStates())
            {
                ClearCurrentStateInHistory(commander.MainStateHistory, ActivateCachedAsMainState);
            }
            else
            {
                ActivateCachedAsMainState();
            }
        }

        private void ClearCurrentStateInHistory(StatetHistory history, Action Callback)
        {
            GameState current = history.GetCurrentState();
            history.RemoveCurrentState();
            current.Disable(Callback);
        }

        private void ActivateCachedAsMainState()
        {
            commander.OverallStateHistory.Save(cachedReqState);
            commander.MainStateHistory.Save(cachedReqState);
            cachedReqState.Enable(ExecutionerCleanup);
        }

        private void ExecuteChildRequest(GameState reqChildState)
        {
            commander.ChildStateHistory.Save(reqChildState);
            commander.OverallStateHistory.Save(reqChildState);
            reqChildState.Enable(ExecutionerCleanup);
        }

        public void ExecutionerCleanup()
        {
            // change the Command state to process requests
            commander.SetExecutor(this);
            commander.CheckBufferLoad();
        }
    }
}