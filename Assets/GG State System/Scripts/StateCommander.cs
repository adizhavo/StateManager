using System;
using UnityEngine;
using System.Collections.Generic;

namespace StateSystem
{
    [System.Serializable]
    public class StateCommander
    {
        #region PUBLIC_GETTERS

        public StateMapper Mapper{ get { return mapper; } }

        public StatetHistory OverallStateHistory{ get { return overallStateHistory; } }

        public StatetHistory MainStateHistory{ get { return mainStateHistory; } }

        public StatetHistory ChildStateHistory{ get { return childStateHistory; } }

        public StateBuffer MainStateBuffer{ get { return mainStateBuffer; } }

        public StateBuffer ChildStateBuffer{ get { return childStateBuffer; } }

        public StateRequest CachedPrevRequest{ get { return cachedPrevRequest; } }

        #endregion

        #region Protected_Fields

        protected StateMapper mapper;

        protected StatetHistory overallStateHistory;
        protected StatetHistory mainStateHistory;
        protected StatetHistory childStateHistory;

        protected StateBuffer mainStateBuffer;
        protected StateBuffer childStateBuffer;

        protected StateRequest cachedPrevRequest;

        protected IStateExecutor currentExecutor;
        protected object lockObject = new object();

        #endregion

        #region PublicCommanderMethods

        public StateCommander()
        {
            mapper = new StateMapper();

            overallStateHistory = new StatetHistory();
            mainStateHistory = new StatetHistory();
            childStateHistory = new ActiveStateHistory();

            mainStateBuffer = new StateBuffer();
            childStateBuffer = new StateBuffer();
        }

        public void SwitchMainState(StateRequest reqState)
        {
            CheckReceivedSwitchRequest(reqState, currentExecutor.ProcessRequestedMainState);
        }

        public void StartChildState(StateRequest reqState)
        {
            CheckReceivedSwitchRequest(reqState, currentExecutor.ProcessRequestedChildState);
        }

        public void KillChildState(StateRequest paralReqState)
        {
            CheckReceivedSwitchRequest(paralReqState, currentExecutor.KillChildGameState);
        }

        public void SetExecutor(IStateExecutor executor)
        {
            if (executor != null)
            {
                currentExecutor = executor;
            }
        }

        public virtual void CheckBufferLoad()
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

        #endregion

        protected void CheckReceivedSwitchRequest(StateRequest reqState, Action<GameState> processReqState)
        {
            // Thread Lock
            lock (lockObject)
            {
                GameState reqGameState = Mapper.GetStateWithEnum(reqState.GetStateRequested());
                if (reqGameState == null)
                {
                    Debug.LogError(string.Format("[State Manager] State requested is not registered! Request Type : {0}", reqState.GetStateRequested()));
                }
                else
                {
                    cachedPrevRequest = reqState;
                    processReqState(reqGameState);
                }
            }
        }
    }
}