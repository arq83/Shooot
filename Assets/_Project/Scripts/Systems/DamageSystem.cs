using System.Collections.Generic;
using UnityEngine;
using Arekntt.Core;

public class DamageSystem : ISystem
{
    private EventQueue queue;
    private List<EnemyRuntimeData> enemies;
    private GameState state;
    private List<PowerUpRuntimeData> powerUps;
    private PowerUpView powerUpPrefab;

    public DamageSystem(EventQueue queue, List<EnemyRuntimeData> enemies, GameState state, List<PowerUpRuntimeData> powerUps, PowerUpView powerUpPrefab)
    {
        this.queue = queue;
        this.enemies = enemies;
        this.state = state;
        this.powerUps = powerUps;
        this.powerUpPrefab = powerUpPrefab;
    }

    public void Update()
    {
        if (state.IsPaused) return;
        foreach (var e in queue.GetEvents())
        {
            if (e is HitEvent hit)
            {
                hit.Enemy.Health -= hit.Damage;
                hit.Enemy.View.Flash();
                AudioManager.Instance?.PlayHit();

                // knockback
                Vector2 knockDir = (hit.Enemy.Position - hit.KnockbackSource).normalized;
                hit.Enemy.KnockbackVelocity = knockDir * 12f;

                if (hit.Enemy.Health <= 0)
                {
                    AudioManager.Instance?.PlayDie();
                    AudioManager.Instance?.PlayKillConfirm();
                    state.Score += 10;
                    HighscoreSystem.Submit(state.Score);
                    GameObject.Destroy(hit.Enemy.View.gameObject);
                    if (Random.value < 0.3f)
                    {
                        var powerData = new PowerUpRuntimeData
                        {
                            Position = hit.Enemy.Position
                        };

                        var view = GameObject.Instantiate(powerUpPrefab);
                        view.Init(powerData);

                        powerData.View = view;
                        powerUps.Add(powerData);
                    }
                    enemies.Remove(hit.Enemy);
                }
            }
        }
    }
}