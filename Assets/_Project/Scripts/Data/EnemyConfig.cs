using UnityEngine;

[CreateAssetMenu(menuName = "Game/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    public float Speed;
    public int Health;
    public Color Color;
    public float Scale = 1f;

    public bool CanShoot = false;        // czy strzelaj¹cy typ
    public float ShootRange = 5f;        // dystans z którego strzela
    public float ShootCooldown = 2f;     // co ile sekund strzela
}