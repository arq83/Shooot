using Arekntt.Core;
public class GameStateSystem : ISystem
{
    private EventQueue queue;
    private GameState state;

    public GameStateSystem(EventQueue queue, GameState state)
    {
        this.queue = queue;
        this.state = state;
    }

    public void Update()
    {
        //if (state.IsPaused) return;
        foreach (var e in queue.GetEvents())
        {
            if (e is GameOverEvent)
                state.IsGameOver = true;

            if (e is TogglePauseEvent && !state.IsGameOver)
                state.IsPaused = !state.IsPaused;
        }
    }
}