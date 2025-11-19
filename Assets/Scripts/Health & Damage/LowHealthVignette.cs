using UnityEngine;

public class LowHealthVignette : MonoBehaviour
{
    [SerializeField] private CanvasGroup vignetteGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    void Awake()
    {
        if (vignetteGroup == null)
            vignetteGroup = GetComponent<CanvasGroup>();

        HideVignette(); // Start hidden
    }

    public void ShowVignette()
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(1f));
    }

    public void HideVignette()
    {
        StopAllCoroutines();
        vignetteGroup.alpha = 0f;
    }

    System.Collections.IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = vignetteGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            vignetteGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        vignetteGroup.alpha = targetAlpha;
    }
}