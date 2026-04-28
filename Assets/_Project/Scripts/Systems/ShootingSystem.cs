using UnityEngine;
using UnityEngine.EventSystems;
using Arekntt.Core;

public class ShootingSystem : ISystem
{
    private EventQueue queue;
    private PlayerRuntimeData player;
    private GameState state;


    public ShootingSystem(EventQueue queue, PlayerRuntimeData player, GameState state)
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
            if (e is ShootEvent)
            {
                // strza³ do góry (na razie na sztywno)
                //Vector2 direction = Vector2.up;
                Debug.Log("Shoot dir: " + player.AimDirection);
                Vector2 direction = player.AimDirection;// z inputu

                queue.EnqueueImmediate(new SpawnProjectileEvent(player.Position, direction));
                Debug.Log("AudioManager instance: " + (AudioManager.Instance == null ? "NULL" : "OK"));
                Debug.Log("MuzzleFlash Show called at: " + player.Position);
                MuzzleFlash.Show(player.Position, direction);
                //AudioManager.Instance?.PlayShoot();
                //AudioManager.Instance.PlayShootQuantized();
                //AudioManager.Instance.PlayShootGroove();
                AudioManager.Instance.PlayShootRhythmic();
                // triggeruj nutê basu
                //MarkovBassSequencer.Instance?.TriggerFromShot();

                Debug.Log("Spawn projectile event");
            }
        }
    }
}