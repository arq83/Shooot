using UnityEngine;

public struct MoveEvent
{
    public Vector2 Direction;

    public MoveEvent(Vector2 direction)
    {
        Direction = direction;
    }
}
