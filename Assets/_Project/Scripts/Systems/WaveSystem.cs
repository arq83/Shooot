using UnityEngine;
using System.Collections.Generic;
using Arekntt.Core;

public class WaveSystem : ISystem
{
    private List<EnemyRuntimeData> enemies;
    private EnemyView prefab;
    private GameState state;

    private float timer = 0f;
    private int currentWave = 0;

    private List<EnemyConfig> configs;

    private List<WaveData> waves = new List<WaveData>
    {
        new WaveData { EnemyCount = 3, SpawnInterval = 0.5f },
        new WaveData { EnemyCount = 5, SpawnInterval = 0.4f },
        new WaveData { EnemyCount = 8, SpawnInterval = 0.3f }
    };

    private int spawnedInWave = 0;
    private ICoroutineRunner coroutineRunner;

    public WaveSystem(EnemyView prefab, List<EnemyRuntimeData> enemies, GameState state, List<EnemyConfig> configs, ICoroutineRunner coroutineRunner)
    {
        this.prefab = prefab;
        this.enemies = enemies;
        this.state = state;
        this.configs = configs;
        this.coroutineRunner = coroutineRunner;
    }

    public void Update()
    {
        if (state.IsGameOver) return;

        if (currentWave >= waves.Count)
        {
            // DODAJ - mo¿esz np. zapêtliæ fale albo...
            // na razie restartuj od fali 0 z trudniejszymi parametrami
            currentWave = 0;
            foreach (var w in waves)
            {
                w.EnemyCount = (int)(w.EnemyCount * 1.5f);
                w.SpawnInterval *= 0.9f;
            }
            return;
        }

        var wave = waves[currentWave];

        timer -= Time.deltaTime;

        if (spawnedInWave < wave.EnemyCount)
        {
            if (timer <= 0f)
            {
                SpawnEnemy();
                spawnedInWave++;
                timer = wave.SpawnInterval;
            }
        }
        else
        {
            // przejœcie do kolejnej fali
            if (enemies.Count == 0)
            {
                currentWave++;
                spawnedInWave = 0;
                Debug.Log("Next wave: " + currentWave);
            }
        }
    }

    private void SpawnEnemy()
    {
        Vector2 pos = new Vector2(Random.Range(-5, 5), Random.Range(-5, 5));
        var config = configs[Random.Range(0, configs.Count)];

        SpawnIndicator.Show(pos, 1f);

        // ZMIANA: RunDelayed zamiast StartCoroutine
        coroutineRunner.RunDelayed(() =>
        {
            var data = new EnemyRuntimeData
            {
                Position = pos,
                Health = config.Health,
                Speed = config.Speed,
                Color = config.Color,
                Scale = config.Scale,

                CanShoot = config.CanShoot,
                ShootRange = config.ShootRange,
                ShootCooldown = config.ShootCooldown
            };

            var view = GameObject.Instantiate(prefab);
            view.transform.position = pos;
            view.Init(data);

            data.View = view;
            enemies.Add(data);
        }, 1f);
    }

}