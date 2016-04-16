using UnityEngine;
using System.Collections.Generic;

namespace StateSystem
{
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
}
