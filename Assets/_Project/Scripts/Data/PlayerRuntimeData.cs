using UnityEngine;

public class PlayerRuntimeData
{
    public Vector2 Position;
    public int Health = 5;
    public int MaxHealth = 100;
    public Vector2 AimDirection = Vector2.up;

    public void ApplyDamage(int dmg)
    {
        Health -= dmg;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
    }

    public void Heal(int amount)
    {
        Health += amount;
        Health = Mathf.Clamp(Health, 0, MaxHealth);
    }
}