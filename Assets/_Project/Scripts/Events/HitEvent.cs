using UnityEngine;

public struct HitEvent
{
    public EnemyRuntimeData Enemy;
    public int Damage;
    public Vector2 KnockbackSource;

    public HitEvent(EnemyRuntimeData enemy, int damage, Vector2 knockbackSource)
    {
        Enemy = enemy;
        Damage = damage;
        KnockbackSource = knockbackSource;
    }
}