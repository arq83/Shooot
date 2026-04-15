using UnityEngine;
using UnityEngine.InputSystem;
using Arekntt.Core;

public class PlayerInputSystem : ISystem
{
    private EventQueue queue;
    private GameState state;
    private PlayerRuntimeData player;

    public PlayerInputSystem(EventQueue queue, GameState state, PlayerRuntimeData player)
    {
        this.queue = queue;
        this.state = state;
        this.player = player;
    }

    public void Update()
    {
        if (state.IsPaused) return;
        if (state.IsGameOver) return;

        Debug.Log("InputSystem dzia³a");

        Vector2 move = Vector2.zero;

        // 1. NAJPIERW zbieramy input
        if (Keyboard.current.wKey.isPressed) move.y += 1;
        if (Keyboard.current.sKey.isPressed) move.y -= 1;
        if (Keyboard.current.aKey.isPressed) move.x -= 1;
        if (Keyboard.current.dKey.isPressed) move.x += 1;

        Debug.Log("Move: " + move);

        // 2. POTEM u¿ywamy move
        if (move != Vector2.zero)
        {
            player.AimDirection = move.normalized;
            Debug.Log("Aim set to: " + player.AimDirection);

            queue.Enqueue(new MoveEvent(move.normalized));
        }

        // strza³
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Shoot dir: " + player.AimDirection);
            queue.Enqueue(new ShootEvent());
        }

        // pause
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            queue.Enqueue(new TogglePauseEvent());
        }

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        Vector3 aimDir = (mouseWorld - (Vector3)player.Position);
        aimDir.z = 0;

        if (aimDir != Vector3.zero)
        {
            player.AimDirection = aimDir.normalized;
        }
    }
}
