using UnityEngine;

public class PowerUpView : MonoBehaviour
{
    public PowerUpRuntimeData data;
    private SpriteRenderer sr;
    private float pulseTimer = 0f;

    public void Init(PowerUpRuntimeData data)
    {
        this.data = data;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position = data.Position;

        // pulsowanie rozmiaru
        pulseTimer += Time.deltaTime * 3f;
        float scale = 0.4f + Mathf.Sin(pulseTimer) * 0.1f;
        transform.localScale = Vector3.one * scale;

        // pulsowanie koloru — jaœniej/ciemniej
        float brightness = 1f + Mathf.Sin(pulseTimer * 2f) * 0.3f;
        sr.color = new Color(brightness, 0f, brightness * 0.8f, 1f);
    }
}