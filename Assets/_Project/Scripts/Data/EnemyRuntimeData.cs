using UnityEngine;

public class EnemyRuntimeData
{
    public Vector2 Position;
    public int Health;
    public EnemyView View;
    public float Speed;
    public Color Color;
    public float Scale;

    public float AttackCooldown = 1.5f;
    public float AttackTimer = 0f;

    public bool CanShoot = false;        
    public float ShootRange = 5f;        
    public float ShootCooldown = 2f;     
    public float ShootTimer = 0f;        

    public Vector2 KnockbackVelocity;
}