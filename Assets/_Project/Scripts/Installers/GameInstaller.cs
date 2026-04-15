using UnityEngine;
using System.Collections.Generic;
using Arekntt.Core;

public class GameInstaller : MonoBehaviour, ICoroutineRunner // DODAJ interfejs
{
    public PlayerView playerPrefab;
    public ProjectileView projectilePrefab;
    public EnemyView enemyPrefab;
    private GameLoop gameLoop;
    private List<EnemyRuntimeData> enemies = new();
    private List<ProjectileRuntimeData> projectiles = new();
    public GameUIController ui;
    public GameObject hitEffectPrefab;
    private List<PowerUpRuntimeData> powerUps = new();
    public PowerUpView powerUpPrefab;
    public PlayerConfig playerConfig;
    public List<EnemyConfig> enemyConfigs;

    // USUÑ Instance - nie potrzebujemy go ju¿

    // DODAJ implementacjê interfejsu:
    public void RunDelayed(System.Action action, float delay)
    {
        StartCoroutine(DelayRoutine(action, delay));
    }

    private System.Collections.IEnumerator DelayRoutine(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    void Start()
    {
        var queue = new EventQueue();
        var save = SaveSystem.Load();
        var playerData = new PlayerRuntimeData();
        playerData.Health = playerConfig.MaxHealth;
        playerData.MaxHealth = playerConfig.MaxHealth;
        var player = Instantiate(playerPrefab);
        player.Init(playerData, playerConfig);
        var gameState = new GameState();
        ui.Init(playerData, gameState);

        var systems = new List<ISystem>
        {
            new PlayerInputSystem(queue, gameState, playerData),
            new MovementSystem(playerData, queue, gameState, playerConfig.MoveSpeed),
            new BoundsSystem(playerData),
            new WaveSystem(enemyPrefab, enemies, gameState, enemyConfigs, this), // this implementuje ICoroutineRunner
            new EnemyMovementSystem(enemies, playerData, queue, gameState),
            new ShootingSystem(queue, playerData, gameState),
            new ProjectileSystem(queue, projectilePrefab, gameState, projectiles, enemies, hitEffectPrefab),
            new DamageSystem(queue, enemies, gameState, powerUps, powerUpPrefab),
            new PowerUpSystem(powerUps, playerData),
            new PlayerDamageSystem(queue, playerData, gameState),
            new GameStateSystem(queue, gameState)
        };

        gameLoop = new GameLoop(systems, queue);
    }

    void Update()
    {
        if (gameLoop == null)
        {
            Debug.LogError("GameLoop is NULL!");
            return;
        }
        gameLoop.Update();
    }
}