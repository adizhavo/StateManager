using UnityEngine;

namespace StateSystem
{
    public class LockedExecutor : IStateExecutor
    {
        protected StateCommander commander;

        public LockedExecutor(StateCommander commander)
        {
            this.commander = commander;
        }

        public void ProcessRequestedMainState(GameState reqGameState)
        {
            Debug.Log("[Buffer] Main " + reqGameState);
            SaveToBuffer(commander.MainStateBuffer);
        }

        public void ProcessRequestedChildState(GameState reqGameState)
        {
            Debug.Log("[Buffer] Child " + reqGameState);
            SaveToBuffer(commander.ChildStateBuffer);
        }

        public void KillChildGameState(GameState requestedparallelGameState)
        {
            Debug.Log("[Command Buffer] You cannot kill, wait for the manager to be active");
        }

        private void SaveToBuffer(StateBuffer stateBuffer)
        {
            stateBuffer.SaveState(commander.CachedPrevRequest);
        }
    }
}
