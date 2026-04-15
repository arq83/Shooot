using UnityEngine;

[CreateAssetMenu(menuName = "Game/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    public float Speed;
    public int Health;
    public Color Color;
    public float Scale = 1f;
}