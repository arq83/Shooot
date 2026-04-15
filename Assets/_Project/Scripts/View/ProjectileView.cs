using UnityEngine;

public class ProjectileView : MonoBehaviour
{
    private TrailRenderer trail;

    void Awake()
    {
        trail = gameObject.AddComponent<TrailRenderer>();

        // czas ¿ycia smugi
        trail.time = 0.2f;

        // szerokoœæ — gruba z przodu, cienka z ty³u
        trail.startWidth = 0.15f;
        trail.endWidth = 0f;

        // kolor — ¿ó³ty/bia³y z przodu, przezroczysty z ty³u
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 1f, 0.5f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = gradient;

        // material — u¿yj domyœlnego sprite/unlit
        trail.material = new Material(Shader.Find("Sprites/Default"));

        // sortowanie ¿eby by³ nad t³em
        trail.sortingOrder = 5;
    }

    void OnDisable()
    {
        // czyœæ trail gdy pocisk wraca do poola
        if (trail) trail.Clear();
    }
}