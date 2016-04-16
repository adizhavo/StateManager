using UnityEngine;
using System.Collections.Generic;

namespace StateSystem
{
    public class StateBuffer
    {
        protected Queue<StateRequest> bufferedStates = new Queue<StateRequest>();

        public void SaveState(StateRequest state)
        {
            bufferedStates.Enqueue(state);

            #if UNITY_EDITOR
            Debug.Log(string.Format("[State Buffer] State saved: {0}", state.GetStateRequested()));
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
}
