using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SpeedDisplay : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite emptySpeed;
    public Sprite greenSpeed;
    public Sprite yellowSpeed;
    public Sprite orangeSpeed;
    public Sprite redSpeed;

    [Header("Speed Thresholds (normalized 0–1)")]
    public float greenThreshold = 0.05f;
    public float yellowThreshold = 0.25f;
    public float orangeThreshold = 0.5f;
    public float redThreshold = 0.75f;

    [Header("Max Speed Reference")]
    public float maxSpeed = 10f;

    [Header("UI Prompt")]
    public TMP_Text chargePrompt;
    public float flashSpeed = 0.5f;

    [Header("Player References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInputHandler inputHandler;

    private Image speedImage;
    private Coroutine flashRoutine;

    void Awake()
    {
        speedImage = GetComponent<Image>();
        if (chargePrompt != null)
            chargePrompt.gameObject.SetActive(false);
    }

    public void SetSpeedVisual(float currentSpeed)
    {
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);

        // --- Sprite logic ---
        if (normalizedSpeed < greenThreshold)
            speedImage.sprite = emptySpeed;
        else if (normalizedSpeed < yellowThreshold)
            speedImage.sprite = greenSpeed;
        else if (normalizedSpeed < orangeThreshold)
            speedImage.sprite = yellowSpeed;
        else if (normalizedSpeed < redThreshold)
            speedImage.sprite = orangeSpeed;
        else
            speedImage.sprite = redSpeed;

        // --- Prompt logic ---
        bool isCharging = normalizedSpeed >= redThreshold
                          && inputHandler != null
                          && inputHandler.ChargeHeld;

        if (chargePrompt != null)
        {
            if (isCharging && !chargePrompt.gameObject.activeSelf)
            {
                chargePrompt.gameObject.SetActive(true);
                if (flashRoutine == null)
                    flashRoutine = StartCoroutine(FlashPrompt());
            }
            else if (!isCharging && chargePrompt.gameObject.activeSelf)
            {
                chargePrompt.gameObject.SetActive(false);
                if (flashRoutine != null)
                {
                    StopCoroutine(flashRoutine);
                    flashRoutine = null;
                }
            }
        }
    }

    private IEnumerator FlashPrompt()
    {
        while (true)
        {
            chargePrompt.enabled = !chargePrompt.enabled;
            yield return new WaitForSeconds(flashSpeed);
        }
    }
}