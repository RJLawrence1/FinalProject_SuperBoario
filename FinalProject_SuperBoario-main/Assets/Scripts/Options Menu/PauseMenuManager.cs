using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject controlsPanel;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Volume Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    [Header("Test Sounds")]
    public AudioSource sfxTestSource;
    public AudioClip sfxTestClip;
    private float sfxTestCooldown = 0.2f;
    private float sfxTestTimer = 0f;

    [Header("Input")]
    public InputActionReference toggleMenuAction;

    public static bool IsMenuOpen { get; private set; } = false;

    void Start()
    {
        ShowMain();
        gameObject.SetActive(false); // Hide menu on start

        // Load saved volumes
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        voiceSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 0.75f);

        SetVolume("MusicVolume", musicSlider.value);
        SetVolume("SFXVolume", sfxSlider.value);
        SetVolume("VoiceVolume", voiceSlider.value);
    }

    void OnEnable()
    {
        toggleMenuAction.action.performed += ctx => ToggleMenu();
        toggleMenuAction.action.Enable();
    }

    void OnDisable()
    {
        toggleMenuAction.action.performed -= ctx => ToggleMenu();
        toggleMenuAction.action.Disable();
    }

    public void ToggleMenu()
    {
        IsMenuOpen = !IsMenuOpen;
        gameObject.SetActive(IsMenuOpen);
        Time.timeScale = IsMenuOpen ? 0f : 1f;
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowControls()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        ToggleMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetMusicVolume(float volume) => SetVolume("MusicVolume", volume);
    public void SetSFXVolume(float volume)
    {
        SetVolume("SFXVolume", volume);

        if (sfxTestSource != null && sfxTestClip != null && Time.unscaledTime >= sfxTestTimer)
        {
            sfxTestSource.PlayOneShot(sfxTestClip);
            sfxTestTimer = Time.unscaledTime + sfxTestCooldown;
        }
    }
    public void SetVoiceVolume(float volume) => SetVolume("VoiceVolume", volume);

    private void SetVolume(string exposedParam, float volume)
    {
        float dB = (volume > 0.99f) ? 0f : Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
        audioMixer.SetFloat(exposedParam, dB);
        PlayerPrefs.SetFloat(exposedParam, volume);
    }
}