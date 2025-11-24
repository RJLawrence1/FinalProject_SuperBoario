using UnityEngine;
using UnityEngine.UI;

public class HeartDisplay : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    private Image heartImage;

    void Awake()
    {
        heartImage = GetComponent<Image>();
    }

    public void SetHeart(float value)
    {
        if (value >= 1f)
            heartImage.sprite = fullHeart;
        else if (value > 0f)
            heartImage.sprite = halfHeart;
        else
            heartImage.sprite = emptyHeart;
    }
}
