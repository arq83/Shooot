using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour
{
    public static void Show(Vector2 position, Vector2 direction)
    {
        var go = new GameObject("MuzzleFlash");
        go.transform.position = position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0, 0, angle);
        Debug.Log("MuzzleFlash GO created at: " + go.transform.position);
        var flash = go.AddComponent<MuzzleFlash>();
        flash.StartCoroutine(flash.Animate());
    }

    private IEnumerator Animate()
    {
        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateFlashSprite();
        sr.color = new Color(1f, 0.9f, 0.3f, 1f);
        sr.sortingOrder = 10;

        // rozmiar startowy
        transform.localScale = Vector3.one * 2f;

        float duration = 0.15f; // d³u¿ej ¿eby by³o widoczne
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            float scale = Mathf.Lerp(2f, 0.2f, progress);
            transform.localScale = Vector3.one * scale;
            sr.color = new Color(1f, 0.9f, 0.3f, 1f - progress);
            yield return null;
        }

        Destroy(gameObject);
    }

    private Sprite CreateFlashSprite()
    {
        int res = 32;
        var tex = new Texture2D(res, res);
        Vector2 center = new Vector2(res / 2f, res / 2f);

        for (int x = 0; x < res; x++)
            for (int y = 0; y < res; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Max(0, 1f - dist / (res / 2f));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }
}