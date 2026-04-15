using UnityEngine;
using System.Collections.Generic;

public class MenuBackground : MonoBehaviour
{
    [System.Serializable]
    public class Particle
    {
        public RectTransform rect;
        public Vector2 velocity;
        public float rotationSpeed;
    }

    private List<Particle> particles = new();
    private RectTransform canvasRect;

    private Color[] colors = new Color[]
    {
        new Color(1f, 1f, 1f, 0.03f),
        new Color(1f, 1f, 1f, 0.05f),
        new Color(1f, 1f, 1f, 0.02f),
    };

    void Start()
    {
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        SpawnParticles(12);
    }

    void SpawnParticles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // stwórz obiekt
            var go = new GameObject("BgParticle_" + i);
            go.transform.SetParent(transform, false);

            // dodaj Image
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = colors[Random.Range(0, colors.Length)];

            // rozmiar losowy
            var rect = go.GetComponent<RectTransform>();
            float size = Random.Range(40f, 150f);
            rect.sizeDelta = new Vector2(size, size);

            // pozycja losowa w granicach canvasu
            float hw = canvasRect.rect.width / 2f;
            float hh = canvasRect.rect.height / 2f;
            rect.anchoredPosition = new Vector2(
                Random.Range(-hw, hw),
                Random.Range(-hh, hh)
            );

            // rotacja losowa
            rect.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            var p = new Particle
            {
                rect = rect,
                velocity = new Vector2(
                    Random.Range(-20f, 20f),
                    Random.Range(10f, 40f)  // g³ównie w górê
                ),
                rotationSpeed = Random.Range(-30f, 30f)
            };

            particles.Add(p);
        }
    }

    void Update()
    {
        float hw = canvasRect.rect.width / 2f;
        float hh = canvasRect.rect.height / 2f;

        foreach (var p in particles)
        {
            // ruch
            p.rect.anchoredPosition += p.velocity * Time.deltaTime;

            // rotacja
            p.rect.Rotate(0, 0, p.rotationSpeed * Time.deltaTime);

            // wrap — jeœli wyjdzie za ekran wróæ z drugiej strony
            var pos = p.rect.anchoredPosition;

            if (pos.y > hh + 100f) pos.y = -hh - 100f;
            if (pos.y < -hh - 100f) pos.y = hh + 100f;
            if (pos.x > hw + 100f) pos.x = -hw - 100f;
            if (pos.x < -hw - 100f) pos.x = hw + 100f;

            p.rect.anchoredPosition = pos;
        }
    }
}
