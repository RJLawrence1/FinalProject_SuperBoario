using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Sound Clips")]
    public AudioClip uiClick;
    public AudioClip npcInteract;
    public AudioClip footstepsGrass;
    public AudioClip footstepsConcrete;
    public AudioClip jump;
    public AudioClip skid;
    public AudioClip waterSplash;
    public AudioClip yellHurt;
    public AudioClip yellDeath;
    public AudioClip respawn;
    public AudioClip checkpointSound;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // General sounds
    public void PlayUIClick() => PlayClip(uiClick);
    public void PlayNPCInteract() => PlayClip(npcInteract);
    public void PlayFootstepGrass() => PlayClip(footstepsGrass);
    public void PlayFootstepConcrete() => PlayClip(footstepsConcrete);
    public void PlayJump() => PlayClip(jump);
    public void PlaySkid() => PlayClip(skid);
    public void PlayWaterSplash() => PlayClip(waterSplash);
    public void PlayYellHurt() => PlayClip(yellHurt);
    public void PlayYellDeath() => PlayClip(yellDeath);
    public void PlayRespawn() => PlayClip(respawn);
    public void PlayCheckpointTouch() => PlayClip(checkpointSound);
}