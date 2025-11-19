using UnityEngine;
using UnityEngine.InputSystem;

public class OptionsMenuToggle : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject optionsPanel;

    [Header("Audio")]
    public AudioSource backgroundMusic;
    public AudioSource menuMusic;

    [Header("Input")]
    public InputActionReference toggleMenuAction;

    private bool isVisible = false;
    public static bool IsMenuOpen { get; private set; } = false;

    void Start()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true); // Temporarily activate
            Canvas.ForceUpdateCanvases(); // Force layout rebuild
            optionsPanel.SetActive(false); // Hide again
            isVisible = false;
        }
    }

    void OnEnable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.performed += ctx => ToggleMenu();
            toggleMenuAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.performed -= ctx => ToggleMenu();
            toggleMenuAction.action.Disable();
        }
    }

    private void ToggleMenu()
    {
        isVisible = !isVisible;
        optionsPanel.SetActive(isVisible);
        Time.timeScale = isVisible ? 0f : 1f;

        IsMenuOpen = isVisible;

        // Menu music
        if (menuMusic != null)
        {
            if (isVisible && !menuMusic.isPlaying)
                menuMusic.Play();
            else if (!isVisible && menuMusic.isPlaying)
                menuMusic.Stop();
        }

        // Background music
        if (backgroundMusic != null)
        {
            if (isVisible && backgroundMusic.isPlaying)
                backgroundMusic.Pause();
            else if (!isVisible && !backgroundMusic.isPlaying)
                backgroundMusic.UnPause();
        }
    }
}