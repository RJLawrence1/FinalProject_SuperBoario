using UnityEngine;
using UnityEngine.UI;

public class ScreenOverlayController : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideOverlay(); // Start hidden
    }

    public void ShowDeathOverlay()
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(1f)); // Fade in to full opacity
    }

    public void HideOverlay()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0f;
    }

    System.Collections.IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}