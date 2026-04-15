using UnityEngine;

public struct SpawnProjectileEvent
{
    public Vector2 Position;
    public Vector2 Direction;

    public SpawnProjectileEvent(Vector2 position, Vector2 direction)
    {
        Position = position;
        Direction = direction;
    }
}
