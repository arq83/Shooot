using UnityEngine;

public class EnemyView : MonoBehaviour
{
    public EnemyRuntimeData data;
    private SpriteRenderer sr;
    private float flashTimer = 0f;

    public void Init(EnemyRuntimeData data)
    {
        this.data = data;
        sr = GetComponent<SpriteRenderer>();
        Color brightColor = data.Color * 2f;
        brightColor.a = 1f;
        sr.color = brightColor;
        transform.localScale = Vector3.one * data.Scale;
        //transform.position = data.Position;
    }

    public void Flash()
    {
        flashTimer = 0.1f;
    }

    void Update()
    {
        transform.position = data.Position;
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            sr.color = Color.red * 2f; // jasny flash
        }
        else
        {
            Color brightColor = data.Color * 2f;
            brightColor.a = 1f;
            sr.color = brightColor;
        }
    }
}