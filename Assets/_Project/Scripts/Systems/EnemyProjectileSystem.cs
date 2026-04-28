using UnityEngine;
using System.Collections.Generic;
using Arekntt.Core;

public class EnemyProjectileSystem : ISystem
{
    private EventQueue queue;
    private ProjectileView prefab;
    private List<EnemyProjectileRuntimeData> projectiles;
    private PlayerRuntimeData player;
    private GameState state;
    private ObjectPool<ProjectileView> pool;

    private float speed = 6f; // wolniej ni¿ gracz

    public EnemyProjectileSystem(
        EventQueue queue,
        ProjectileView prefab,
        PlayerRuntimeData player,
        GameState state)
    {
        this.queue = queue;
        this.prefab = prefab;
        this.player = player;
        this.state = state;
        projectiles = new List<EnemyProjectileRuntimeData>();
        pool = new ObjectPool<ProjectileView>(prefab, 10);
    }

    public void Update()
    {
        if (state.IsPaused || state.IsGameOver) return;

        // spawn z eventu
        foreach (var e in queue.GetEvents())
        {
            if (e is EnemyShootEvent shoot)
            {
                var data = new EnemyProjectileRuntimeData
                {
                    Position = shoot.Position,
                    Direction = shoot.Direction
                };

                var view = pool.Get();
                // inny kolor ¿eby gracz odró¿ni³ wrogie pociski
                var sr = view.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.red;

                view.transform.position = data.Position;
                data.View = view;
                projectiles.Add(data);

                // dŸwiêk whizz gdy pocisk jest blisko gracza
                AudioManager.Instance?.PlayBulletWhizz();
            }
        }

        // update + kolizja z graczem
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            var p = projectiles[i];
            p.Position += p.Direction * speed * Time.deltaTime;
            p.View.transform.position = p.Position;

            // zniszcz gdy za daleko
            if (Vector2.Distance(p.Position, Vector2.zero) > 30f)
            {
                pool.Return(p.View);
                projectiles.RemoveAt(i);
                continue;
            }

            // trafienie gracza
            if (Vector2.Distance(p.Position, player.Position) < 0.4f)
            {
                queue.Enqueue(new PlayerHitEvent(1));
                AudioManager.Instance?.PlayHit();

                pool.Return(p.View);
                projectiles.RemoveAt(i);
            }
        }
    }
}