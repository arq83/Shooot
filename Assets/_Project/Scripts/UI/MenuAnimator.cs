using UnityEngine;
using System.Collections;

public class MenuAnimator : MonoBehaviour
{
    public static MenuAnimator Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // Fade in ca³ego CanvasGroup
    public void FadeIn(CanvasGroup group, float duration = 0.4f)
    {
        StartCoroutine(FadeRoutine(group, 0f, 1f, duration));
    }

    public void FadeOut(CanvasGroup group, float duration = 0.4f, System.Action onDone = null)
    {
        StartCoroutine(FadeRoutine(group, 1f, 0f, duration, onDone));
    }

    // Slide in z do³u
    public void SlideIn(RectTransform rect, float duration = 0.5f)
    {
        StartCoroutine(SlideRoutine(rect, new Vector2(0, -100f), Vector2.zero, duration));
    }

    // Pulsowanie przycisku
    public void Pulse(RectTransform rect)
    {
        StartCoroutine(PulseRoutine(rect));
    }

    private IEnumerator FadeRoutine(CanvasGroup group, float from, float to, float duration, System.Action onDone = null)
    {
        float t = 0f;
        group.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
        onDone?.Invoke();
    }

    private IEnumerator SlideRoutine(RectTransform rect, Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float ease = 1f - Mathf.Pow(1f - t / duration, 3f); // ease out cubic
            rect.anchoredPosition = Vector2.Lerp(from, to, ease);
            yield return null;
        }
        rect.anchoredPosition = to;
    }

    private IEnumerator PulseRoutine(RectTransform rect)
    {
        Vector3 original = rect.localScale;
        Vector3 big = original * 1.1f;
        float t = 0f;
        float duration = 0.1f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            rect.localScale = Vector3.Lerp(original, big, t / duration);
            yield return null;
        }
        t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            rect.localScale = Vector3.Lerp(big, original, t / duration);
            yield return null;
        }
        rect.localScale = original;
    }
}