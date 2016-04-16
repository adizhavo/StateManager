public interface IStateExecutor
{
    void ProcessRequestedMainState(GameState reqGameState);

    void ProcessRequestedChildState(GameState reqGameState);

    void KillChildGameState(GameState reqGameState);
}