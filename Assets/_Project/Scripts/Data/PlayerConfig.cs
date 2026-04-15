using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public float MoveSpeed = 5f;
    public int MaxHealth = 10;
    public Color Color = new Color(0f, 0.8f, 1f);
}