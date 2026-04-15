using Arekntt.Core;
public class BoundsSystem : ISystem
{
    private PlayerRuntimeData player;
    private float minX, maxX, minY, maxY;

    public BoundsSystem(PlayerRuntimeData player, float minX = -8f, float maxX = 8f, float minY = -4.5f, float maxY = 4.5f)
    {
        this.player = player;
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }

    public void Update()
    {
        player.Position = new UnityEngine.Vector2(
            UnityEngine.Mathf.Clamp(player.Position.x, minX, maxX),
            UnityEngine.Mathf.Clamp(player.Position.y, minY, maxY)
        );
    }
}