using UnityEngine;
using System.Collections.Generic;

namespace StateSystem
{
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
            return base.HasSavedStates();
        }

        protected virtual void CleanDisabledStates()
        {
            for (int i = 0; i < savedHistoryStates.Count; i++)
            {
                if (savedHistoryStates[i].IsGameStateDisabled())
                {
                    savedHistoryStates.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}