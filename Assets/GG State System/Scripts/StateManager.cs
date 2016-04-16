using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StateSystem
{
    public class StateManager : MonoBehaviour
    {
        private static StateManager instance;

        public static StateManager Instance{ get { return instance; } }

        #region PublicManagerMethods

        public void RegisterStates(GameState gameState)
        {
            Commander.Mapper.RegisterState(gameState);
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
            Commander.SwitchMainState(reqState);
        }

        public void StartChildState(StateRequest paralReqState)
        {
            Commander.StartChildState(paralReqState);
        }

        public void KillChildState(StateRequest paralReqState)
        {
            Commander.KillChildState(paralReqState);
        }

        #endregion

        [SerializeField] protected StateRequest StartState;
        [SerializeField] protected StateCommander Commander;

        protected virtual void Awake()
        {
            instance = this;
            InitializeCommander();
            GetAllGameStates();
            SwitchMainState(StartState);
        }

        protected virtual void InitializeCommander()
        {
            IStateExecutor initialStateExecutor = new UnclokedExecutor(Commander);
            Commander.SetExecutor(initialStateExecutor);
        }

        protected void GetAllGameStates()
        {
            GameState[] allStates = Resources.FindObjectsOfTypeAll<GameState>();
            foreach (GameState st in allStates)
            {
                RegisterStates(st);
            }
        }
    }
}