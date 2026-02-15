using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;

public enum SoundType
{
    BUTTON_SELECT,
    BUTTON_CLICK,
    BUTTON_DENIED,
    ATTACK,
    FOOTSTEP,
    ORB_COLLECT,
    PLAYER_DEATH,
    DASH,
    JUMP
}

// [RequireComponent(typeof(AudioSource)), ExecuteInEditMode]

public class AudioManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    public static AudioManager instance;
    [SerializeField] public AudioSource SFX_UIAudioSource;
    [SerializeField] private AudioMixer mixer;


    [Header("Music")]
    public AudioSource musicAudioSource; // Changed from [SerializeField] private to public
    [SerializeField] private AudioClip mainMenuMusicClip; // assign in inspector
    [SerializeField] private AudioClip level1MusicClip; // assign in inspector
    [SerializeField] private AudioClip level2MusicClip; // assign in inspector
    [SerializeField] private AudioClip creditsMusicClip; // assign in inspector
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float musicFadeDuration = 1f;

    [Header("Diegetic")]
    public AudioSource SFX_DiegeticAudioSource;
    [SerializeField] private float diegeticFadeDuration = 1f;

    [Header("Ambience")]
    [SerializeField] private AudioSource SFX_Diegetic_AmbienceAudioSource; // assign in inspector
    [SerializeField] private AudioClip caveAmbienceClip; // assign in inspector
    [SerializeField] private AudioClip spaceAmbienceClip; // New: Level2 ambience
    [SerializeField] private AudioClip mainMenuAmbienceClip; // assign in inspector
    [SerializeField] private float ambienceVolume = 0.5f;
    [SerializeField] private float ambienceFadeDuration = 1f;

    // these variables act as handles to the currently running coroutines, not the coroutines themselves
    private Coroutine musicFadeCoroutine;
    private Coroutine diegeticFadeCoroutine;
    private Coroutine ambienceFadeCoroutine;

    // Awake() can be called multiple times (multiple instances), so we need to ensure that everything gets reset inside
    private void Awake()
    {
        // 7 lines below ensure there is only one AudioManager instance
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // this condition ensures the AudioManager gets transferr
        // ed to the DontDestroyOnLoad scene only while in play mode
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }

        mixer.SetFloat("Music_Internal", -60f);
        mixer.SetFloat("SFX_Diegetic", -60f);

        // 2 lines below ensure the OnSceneLoaded method is called when a scene is loaded
        // SceneManager.sceneLoaded represents a Unity event that is triggered when a scene is loaded (notification system for all the scenes)
        // -= unsubscribes from the event, += subscribes to the event
        // += adds the method to the list of methods to be called when the event is triggered and also pushes variables Scene and LoadSceneMode to the method
        // -= is called first to ensure there are no duplicate subscriptions, because Awake() can be called multiple times
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }



    private void Start()
    {
        // audioSource = GetComponent<AudioSource>();

        // this line retrieves the saved SFX volume from PlayerPrefs, or defaults to 1 if there is no saved volume
        float savedSFX = PlayerPrefs.GetFloat("SFX_Internal", 1f);

        // this line converts the linear volume value of savedSFX to decibel value  
        // Log10(x) * 20f is the standard formula to convert linear volume to decibels (savedSFX 1.0 = 0db which is the max volume, savedSFX 0.5 = -6db, savedSFX 0.01 = -40db, etc.)
        // Clamp ensures the value is between 0.01 and 1, meaning if it is 0, it will be set to 0.01, if it is above 1, it will be set to 1, otherwise it stays the same
        // Clamp is needed because Log10(0) is minus infinity, and that would break the game
        float dB = Mathf.Log10(Mathf.Clamp(savedSFX, 0.0001f, 1f)) * 20f;

        // these 3 lines set the volume of the SFX, SFX_Diegetic, and SFX_Diegetic_Ambience groups in the audio mixer to the value of dB
        mixer.SetFloat("SFX_Internal", dB);
        mixer.SetFloat("SFX_Diegetic", dB);
        mixer.SetFloat("SFX_Diegetic_Ambience", dB);
    }

    // this method is called by other scripts to play short sounds
    public static void PlaySound(SoundType sound, AudioSource audioSource, float volume = 1f)
    {
        // this condition prevents errors if AudioManager is not yet initialized
        if (instance == null)
        {
            Debug.LogWarning("AudioManager instance is null!");
            return;
        }

        // this condition prevents errors if the audio source is not set
        if (audioSource == null)
        {
            Debug.LogWarning($"AudioSource is null for sound {sound}!");
            return;
        }

        // this condition prevents errors if the sound index is out of range 
        int soundIndex = (int)sound;
        if (soundIndex >= instance.soundList.Length)
        {
            Debug.LogWarning($"Sound {sound} (index {soundIndex}) is out of range! SoundList has {instance.soundList.Length} elements.");
            return;
        }

        // this condition prevents errors if there are no clips for the sound
        var clips = instance.soundList[soundIndex].Sounds;
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"No clips found for sound {sound}!");
            return;
        }

        // this line picks a random clip from the array of clips for the sound
        var clip = clips[UnityEngine.Random.Range(0, clips.Length)];

        // this line plays the clip through the provided audio source
        audioSource.PlayOneShot(clip, volume);
    }

    // this method's sole purpose is to keep the soundList in sync with the SoundType enum (editor only)
    private void OnEnable()
    {
    #if UNITY_EDITOR
        // enum name auto-fill
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    #endif
    }

    public void PrepareForSceneLoad()
    {
        if (diegeticFadeCoroutine != null)
        {
            StopCoroutine(diegeticFadeCoroutine);
            diegeticFadeCoroutine = null;
        }

        diegeticFadeCoroutine = StartCoroutine(FadeOutDiegetic());
    }

    // this method is called when a scene is loaded (sceneLoaded event in Awake())
    // the sceneLoaded event requires a method with a signature of (Scene, LoadSceneMode)
    // the Scene variable represents the scene that was loaded
    // the LoadSceneMode variable represents how the scene was loaded (single, additive, etc.)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[AudioManager] Scene loaded: {scene.name}");

        // same code as in Start() to restore SFX volume when scene loads (in case it was muted during death)
        // float savedSFX = PlayerPrefs.GetFloat("SFX", 1f);
        // float dB = Mathf.Log10(Mathf.Clamp(savedSFX, 0.0001f, 1f)) * 20f;

        UnmuteDiegetic();
        diegeticFadeCoroutine = StartCoroutine(FadeInDiegetic());

        // this condition checks if there is a music fade coroutine currently running, and if so, it kills it
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }
        
        // this coroutine does the same for the diegetic fade coroutine
        if (diegeticFadeCoroutine != null)
        {
            StopCoroutine(diegeticFadeCoroutine);
            diegeticFadeCoroutine = null;
        }

        // this coroutine does the same for the ambience fade coroutine
        if (ambienceFadeCoroutine != null)
        {
            StopCoroutine(ambienceFadeCoroutine);
            ambienceFadeCoroutine = null;
        }


        StartCoroutine(SetSceneAudioFaded(scene));
    }

    // private IEnumerator DelayedFadeToSceneAudio(Scene scene
    private IEnumerator SetSceneAudioFaded(Scene scene)
    {
        bool isMainMenu = scene.name == "MainMenu";
        bool isLevel1 = scene.name == "Level1";
        bool isLevel2 = scene.name == "Level2";
        bool isCredits = scene.name == "Credits";

        // Only start music if NOT in tutorial mode
        if (isMainMenu && mainMenuMusicClip != null)
        {
            yield return StartCoroutine(FadeInMusic(mainMenuMusicClip));
        }
        else if (isLevel2 && level2MusicClip != null)
        {
            yield return StartCoroutine(FadeInMusic(level2MusicClip));
        }
        else if (isCredits && creditsMusicClip != null)
        {
            yield return StartCoroutine(FadeInMusic(creditsMusicClip));
        }

        // Ambience continues normally
        if (isMainMenu && mainMenuAmbienceClip != null)
        {
            SetAmbienceClip(mainMenuAmbienceClip);
        }
        else if (isLevel1 && caveAmbienceClip != null)
        {
            SetAmbienceClip(caveAmbienceClip);
        }
        else if (isLevel2 && spaceAmbienceClip != null)
        {
            SetAmbienceClip(spaceAmbienceClip);
        }
    }

    private IEnumerator FadeInMusic(AudioClip targetClip)
    {
        if (musicAudioSource.clip == targetClip && musicAudioSource.isPlaying) yield break;

        // this condition checks if there is any music currently playing, and if so, it fades it out
        if (musicAudioSource.isPlaying)
        {
            yield return StartCoroutine(FadeAudio("Music_Internal", -60f, musicFadeDuration));
        }

        musicAudioSource.clip = targetClip;
        musicAudioSource.loop = true;
        musicAudioSource.Play();

        // Fade in new music using linear values
        float savedMusicVolume = PlayerPrefs.GetFloat("Music_Internal", 1f);
        float targetDB = Mathf.Log10(Mathf.Clamp(savedMusicVolume, 0.001f, 1f)) * 20f;
        
        yield return StartCoroutine(FadeAudio("Music_Internal", targetDB, musicFadeDuration));
    }

    private IEnumerator FadeOutMusic()
    {
        yield return StartCoroutine(FadeAudio("Music_Internal", -60f, musicFadeDuration));
        musicAudioSource.Stop();
    }

    // private IEnumerator PauseMusicCoroutine()
    // {
    //     if (musicFadeCoroutine != null)
    //         StopCoroutine(musicFadeCoroutine);

    //     musicFadeCoroutine = StartCoroutine(FadeAudio("Music_Pause", -60f, musicFadeDuration));

    //     musicAudioSource.Pause();
    // }

    private IEnumerator FadeOutDiegetic()
    {
        yield return StartCoroutine(FadeAudio("SFX_Diegetic", -60f, diegeticFadeDuration));
        Debug.Log("stopping diegetic");
    }

    private IEnumerator FadeInDiegetic()
    {
        float savedSFX = PlayerPrefs.GetFloat("SFX_Diegetic", 1f);
        float targetDB = Mathf.Log10(Mathf.Clamp(savedSFX, 0.001f, 1f)) * 20f;
        
        yield return StartCoroutine(FadeAudio("SFX_Diegetic", targetDB, diegeticFadeDuration));
    }

    private void SetAmbienceClip(AudioClip targetClip)
    {
        if (SFX_Diegetic_AmbienceAudioSource.clip == targetClip && SFX_Diegetic_AmbienceAudioSource.isPlaying) return;

        SFX_Diegetic_AmbienceAudioSource.clip = targetClip;
        SFX_Diegetic_AmbienceAudioSource.loop = true;
        SFX_Diegetic_AmbienceAudioSource.Play();
    }

    private IEnumerator FadeInAmbience(AudioClip targetClip)
    {
        if (SFX_Diegetic_AmbienceAudioSource.isPlaying)
            yield return StartCoroutine(FadeAudio("SFX_Diegetic_Ambience", -60f, ambienceFadeDuration));

        // Switch to new clip without stopping
        SFX_Diegetic_AmbienceAudioSource.clip = targetClip;
        SFX_Diegetic_AmbienceAudioSource.loop = true;
        SFX_Diegetic_AmbienceAudioSource.Play();

        float targetDB = Mathf.Log10(Mathf.Clamp(ambienceVolume, 0.0001f, 1f)) * 20f;
        yield return StartCoroutine(FadeAudio("SFX_Diegetic_Ambience", targetDB, ambienceFadeDuration));
    }

    private IEnumerator FadeOutAmbience()
    {
        if (!SFX_Diegetic_AmbienceAudioSource.isPlaying) yield break;

        yield return StartCoroutine(FadeAudio("SFX_Diegetic_Ambience", -60f, ambienceFadeDuration));
        
        // Don't stop - just set volume to 0 and keep playing
        SFX_Diegetic_AmbienceAudioSource.volume = 0f;
    }

    // this method is a generic fade logic for any audio source and mixer parameter
    private IEnumerator FadeAudio(string mixerParam, float targetDB, float fadeDuration)
    {
        mixer.GetFloat(mixerParam, out float startDB);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            mixer.SetFloat(mixerParam, Mathf.Lerp(startDB, targetDB, t));
            yield return null;
        }

        mixer.SetFloat(mixerParam, targetDB);
    }

    // this method enforces music fade out coroutine by killing any running coroutines first
    // also, we cannot call FadeOutMusic() coroutine directly in other scripts (e.g. PlayerDeath.cs) because it would create a new instance of it, so we wrap it in public StopMusic()
    public void StopMusic()
    {
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);
        
        musicFadeCoroutine = StartCoroutine(FadeOutMusic());
    }

    IEnumerator StopMusicCoroutine()
    {
        yield return FadeAudio("Music_Internal", -60f, musicFadeDuration);
        musicAudioSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);
        
        musicFadeCoroutine = StartCoroutine(FadeAudio("Music_Pause", -60f, musicFadeDuration));
    }

    public void ResumeMusic()
    {
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);


        //mixer.SetFloat("Music", -30f);

        float savedMusicVolume = PlayerPrefs.GetFloat("Music_Pause", 1f);
        float targetDB = Mathf.Log10(Mathf.Clamp(savedMusicVolume, 0.0001f, 1f)) * 20f;
        
        musicFadeCoroutine = StartCoroutine(FadeAudio("Music_Pause", targetDB, musicFadeDuration));
    }

    public void MuteDiegetic()
    {
        if (diegeticFadeCoroutine != null)
            StopCoroutine(diegeticFadeCoroutine);

        diegeticFadeCoroutine = StartCoroutine(FadeAudio("SFX_Pause", -60f, diegeticFadeDuration));
    }

    public void UnmuteDiegetic()
    {
        if (diegeticFadeCoroutine != null)
            StopCoroutine(diegeticFadeCoroutine);

        float savedSFX = PlayerPrefs.GetFloat("SFX_Pause", 1f);
        float targetDB = Mathf.Log10(Mathf.Clamp(savedSFX, 0.001f, 1f)) * 20f;

        diegeticFadeCoroutine = StartCoroutine(FadeAudio("SFX_Pause", targetDB, diegeticFadeDuration));
    }
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name; 
    [SerializeField] private AudioClip[] sounds;
}