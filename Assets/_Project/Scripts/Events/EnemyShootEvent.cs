using UnityEngine;

public struct EnemyShootEvent
{
    public Vector2 Position;
    public Vector2 Direction;

    public EnemyShootEvent(Vector2 position, Vector2 direction)
    {
        Position = position;
        Direction = direction;
    }
}