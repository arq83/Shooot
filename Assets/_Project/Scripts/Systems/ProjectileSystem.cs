using UnityEngine;
using System.Collections.Generic;
using Arekntt.Core;

public class ProjectileSystem : ISystem
{
    private EventQueue queue;
    private ProjectileView prefab;

    private List<ProjectileRuntimeData> projectiles;
    private List<EnemyRuntimeData> enemies;

    private float speed = 10f;
    private GameState state;
    private GameObject hitEffectPrefab;
    private ObjectPool<ProjectileView> pool;


    public ProjectileSystem(
        EventQueue queue,
        ProjectileView prefab,
        GameState state,
        List<ProjectileRuntimeData> projectiles,
        List<EnemyRuntimeData> enemies,
        GameObject hitEffectPrefab)
    {
        this.queue = queue;
        this.prefab = prefab;
        this.state = state;
        this.projectiles = projectiles;
        this.enemies = enemies;
        this.hitEffectPrefab = hitEffectPrefab;
        pool = new ObjectPool<ProjectileView>(prefab, 20);
    }

    public void Update()
    {
        if (state.IsPaused) return;
        // spawn
        foreach (var e in queue.GetEvents())
        {
            if (e is SpawnProjectileEvent spawn)
            {
                var data = new ProjectileRuntimeData
                {
                    Position = spawn.Position,
                    Direction = spawn.Direction
                };

                var view = pool.Get();
                view.transform.position = data.Position;

                data.View = view;

                projectiles.Add(data);
            }
        }

        // update + collision
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            var p = projectiles[i];

            p.Position += p.Direction * speed * Time.deltaTime;
            p.View.transform.position = p.Position;

            foreach (var enemy in enemies)
            {
                if (Vector2.Distance(p.Position, enemy.Position) < 0.5f)
                {
                    queue.Enqueue(new HitEvent(enemy, 1, p.Position));

                    GameObject.Instantiate(hitEffectPrefab, enemy.Position, Quaternion.identity);

                    pool.Return(p.View);
                    projectiles.RemoveAt(i);
                    break;
                }
            }
        }
    }
}