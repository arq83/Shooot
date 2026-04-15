using UnityEngine;
using System.Collections;

public class SpawnIndicator : MonoBehaviour
{
    private SpriteRenderer sr;

    public static void Show(Vector2 position, float duration = 1f)
    {
        var go = new GameObject("SpawnIndicator");
        go.transform.position = position;
        var indicator = go.AddComponent<SpawnIndicator>();
        indicator.StartCoroutine(indicator.Animate(duration));
    }

    void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 0.2f, 0.2f, 0.8f);
        transform.localScale = Vector3.one * 0.3f;
    }

    private IEnumerator Animate(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            // pulsowanie
            float pulse = 1f + Mathf.Sin(progress * Mathf.PI * 6f) * 0.3f;
            transform.localScale = Vector3.one * pulse;

            // fade out na koñcu
            float alpha = progress < 0.7f ? 0.8f : Mathf.Lerp(0.8f, 0f, (progress - 0.7f) / 0.3f);
            sr.color = new Color(1f, 0.2f, 0.2f, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }

    private Sprite CreateCircleSprite()
    {
        int res = 64;
        var tex = new Texture2D(res, res);
        Vector2 center = new Vector2(res / 2f, res / 2f);
        float radius = res / 2f;
        float thickness = 4f;

        for (int x = 0; x < res; x++)
            for (int y = 0; y < res; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                bool isRing = dist > radius - thickness && dist < radius;
                tex.SetPixel(x, y, isRing ? Color.white : Color.clear);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }
}