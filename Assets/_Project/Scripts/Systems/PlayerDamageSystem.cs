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

                float healthPercent = (float)player.Health / player.MaxHealth;

                if (player.Health <= 0)
                {
                    AudioManager.Instance?.StopHeartbeat();
                    queue.EnqueueImmediate(new GameOverEvent());
                }
                else if (healthPercent <= 0.3f) // poni¿ej 30% HP
                {
                    AudioManager.Instance?.StartHeartbeat(healthPercent);
                }
                else
                {
                    AudioManager.Instance?.StopHeartbeat(); // wyleczony powy¿ej 30%
                }
            }
        }
    }
}