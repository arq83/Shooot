using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Arekntt.Core;

public class EnemyMovementSystem : ISystem
{
    private List<EnemyRuntimeData> enemies;
    private PlayerRuntimeData player;
    private EventQueue queue;
    private GameState state;

    //private float speed = 2f;

    public EnemyMovementSystem(List<EnemyRuntimeData> enemies, PlayerRuntimeData player, EventQueue queue, GameState state)
    {
        this.enemies = enemies;
        this.player = player;
        this.queue = queue;
        this.state = state;
    }

    public void Update()
    {
        if (state.IsPaused) return;
        if (state.IsGameOver) return;

        foreach (var enemy in enemies)
        {

            if (enemy.KnockbackVelocity.magnitude > 0.01f)
            {
                enemy.Position += enemy.KnockbackVelocity * Time.deltaTime;
                enemy.KnockbackVelocity = Vector2.Lerp(enemy.KnockbackVelocity, Vector2.zero, Time.deltaTime * 5f);
            }
            else
            {
                Vector2 moveDir = (player.Position - enemy.Position).normalized;
                enemy.Position += moveDir * enemy.Speed * Time.deltaTime;
            }

            // cooldown
            enemy.AttackTimer -= Time.deltaTime;

            if (Vector2.Distance(enemy.Position, player.Position) < 0.5f)
            {
                if (enemy.AttackTimer <= 0f)
                {
                    queue.Enqueue(new PlayerHitEvent(1));
                    enemy.AttackTimer = enemy.AttackCooldown;
                }
            }
        }

        for (int i = 0; i < enemies.Count; i++)
            for (int j = i + 1; j < enemies.Count; j++)
            {
                // ZMIANA: minDist zale¿y od skali obu wrogów
                float minDist = (enemies[i].Scale + enemies[j].Scale) * 0.5f;

                Vector2 diff = enemies[i].Position - enemies[j].Position;
                float dist = diff.magnitude;

                if (dist < minDist && dist > 0.001f)
                {
                    Vector2 push = diff.normalized * (minDist - dist) * 0.5f;
                    enemies[i].Position += push;
                    enemies[j].Position -= push;
                }
            }
    }
}