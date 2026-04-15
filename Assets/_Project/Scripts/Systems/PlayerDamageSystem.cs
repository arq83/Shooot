using UnityEngine;
using Arekntt.Core;

public class PlayerDamageSystem : ISystem
{
    private EventQueue queue;
    private PlayerRuntimeData player;
    private GameState state;

    public PlayerDamageSystem(EventQueue queue, PlayerRuntimeData player, GameState state)
    {
        this.queue = queue;
        this.player = player;
        this.state = state;
    }

    public void Update()
    {
        if (state.IsPaused) return;
        
        foreach (var e in queue.GetEvents())
        {
            if (e is PlayerHitEvent hit)
            {
                if (player.Health <= 0) return;

                player.ApplyDamage(hit.Damage);

                if (player.Health <= 0)
                {
                    queue.EnqueueImmediate(new GameOverEvent());
                }
            }
        }
    }
}