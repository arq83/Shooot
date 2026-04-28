using UnityEngine;
using System.Collections.Generic;
using Arekntt.Core;

public class PowerUpSystem : ISystem
{
    private List<PowerUpRuntimeData> powerUps;
    private PlayerRuntimeData player;

    public PowerUpSystem(List<PowerUpRuntimeData> powerUps, PlayerRuntimeData player)
    {
        this.powerUps = powerUps;
        this.player = player;
    }

    public void Update()
    {
        for (int i = powerUps.Count - 1; i >= 0; i--)
        {
            var p = powerUps[i];

            if (Vector2.Distance(player.Position, p.Position) < 0.5f)
            {
                player.Heal(1);
                AudioManager.Instance?.PlayPickup();

                // sprawdŸ czy HP wróci³o powy¿ej progu
                float healthPercent = (float)player.Health / player.MaxHealth;
                if (healthPercent > 0.3f)
                    AudioManager.Instance?.StopHeartbeat();
                else
                    AudioManager.Instance?.StartHeartbeat(healthPercent); // odœwie¿ tempo

                GameObject.Destroy(p.View.gameObject);
                powerUps.RemoveAt(i);

                Debug.Log("Picked HP!");
            }
        }
    }
}
