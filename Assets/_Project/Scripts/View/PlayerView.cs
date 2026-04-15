using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private PlayerRuntimeData data;

    public void Init(PlayerRuntimeData data, PlayerConfig config)
    {
        this.data = data;
        var sr = GetComponent<SpriteRenderer>();
        sr.color = config.Color;
    }

    void Update()
    {
        transform.position = data.Position;

        Vector2 dir = data.AimDirection;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}
