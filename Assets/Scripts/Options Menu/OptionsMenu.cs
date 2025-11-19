using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Volume Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    [Header("Display Settings")]
    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;

    [Header("Test Sounds")]
    public AudioClip sfxTestClip;
    public AudioSource sfxTestSource;
    private float sfxTestCooldown = 0.2f; // seconds between test sounds
    private float sfxTestTimer = 0f;


    private Resolution[] resolutions;

    void Awake()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentIndex = 0;
        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void Start()
    {
        // Load saved volume values
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        float voiceVol = PlayerPrefs.GetFloat("VoiceVolume", 0.75f);

        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;
        voiceSlider.value = voiceVol;

        SetVolume("MusicVolume", musicVol);
        SetVolume("SFXVolume", sfxVol);
        SetVolume("VoiceVolume", voiceVol);
    }

    public void SetMusicVolume(float volume)
    {
        SetVolume("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume("SFXVolume", volume);
        if (sfxTestSource != null && sfxTestClip != null)
        {
            if (Time.unscaledTime >= sfxTestTimer)
            {
                sfxTestSource.PlayOneShot(sfxTestClip);
                sfxTestTimer = Time.unscaledTime + sfxTestCooldown;
            }
        }
    }

    public void SetVoiceVolume(float volume)
    {
        SetVolume("VoiceVolume", volume);
    }

    private void SetVolume(string exposedParam, float volume)
    {
        float dB = (volume > 0.99f) ? 0f : Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
        audioMixer.SetFloat(exposedParam, dB);
        PlayerPrefs.SetFloat(exposedParam, volume);
        Debug.Log($"{exposedParam} set to {dB} dB");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}