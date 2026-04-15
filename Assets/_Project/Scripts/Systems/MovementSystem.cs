using UnityEngine;
using Arekntt.Core;

public class MovementSystem : ISystem
{
    private PlayerRuntimeData data;
    private EventQueue queue;
    private GameState state;
    private float speed;

    //private float speed = 5f;

    public MovementSystem(PlayerRuntimeData data, EventQueue queue, GameState state, float speed)
    {
        this.data = data;
        this.queue = queue;
        this.state = state;
        this.speed = speed;
    }

    public void Update()
    {
        if (state.IsPaused) return;
        if (state.IsGameOver) return;

        foreach (var e in queue.GetEvents())
        {
            if (e is MoveEvent move)
            {
                data.Position += move.Direction * speed * Time.deltaTime;
            }
            
        }
    }
}