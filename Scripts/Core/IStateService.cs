public interface IStateService
{
    void OnEnter(GameStateDataSO state);
    void OnUpdate(GameStateDataSO state);
    void OnExit(GameStateDataSO state);
}
