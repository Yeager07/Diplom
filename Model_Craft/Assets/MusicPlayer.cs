using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Audio;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    public GameObject musicUIPrefab;

    public AudioClip[] builtInTracks;

    public AudioMixer masterMixer;

    private AudioSource audioSource;
    private Slider volumeSlider;
    private Button playPauseButton;
    private Button nextButton;
    private Button prevButton;
    private TMP_Text trackNameText;
    private TMP_Text volumePercentText;

    private int currentTrackIndex = 0;
    private bool isPlaying = true;
    private bool stopRequested = true;

    private const string VOLUME_KEY = "MusicVolume";
    private const string TRACK_INDEX_KEY = "MusicTrackIndex";
    private const string IS_PLAYING_KEY = "MusicIsPlaying";

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if(audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = false;
        audioSource.playOnAwake = false;

        LoadSettings();

        /*float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 0.5f);
        SetVolume(savedVolume);*/

        if(builtInTracks.Length > 0)
        {
            if(currentTrackIndex >= builtInTracks.Length)
            currentTrackIndex = 0;
            
            PlayTrack(currentTrackIndex);
            
            if(!isPlaying)
            audioSource.Pause();
        }
    }

    public void ShowMusicUI()
    {
        FindContainerAndCreateUI();
    }

    private void FindContainerAndCreateUI()
    {
        Player player = FindFirstObjectByType<Player>();
        Transform containerTransform = null;

        if (player != null && player.typeGame != "MainMenu")
        {
            Transform pauseMenu = player.transform.Find("UI/PauseMenu");
            
            if(pauseMenu != null && pauseMenu.gameObject.activeInHierarchy)
            {
                containerTransform = pauseMenu.Find("Background").Find("MusicAnchor");
                
                if(containerTransform == null)
                {
                    GameObject go = new GameObject("MusicAnchor");
                    go.transform.SetParent(pauseMenu, false);
                    containerTransform = go.transform;
                }
            }
        
            else
            {
                Debug.LogWarning("PauseMenu not active or not found. Music UI will not be created.");
                return;
            }
        }
        
        else
        {
            GameObject containerGO = GameObject.FindGameObjectWithTag("MusicUIContainer");
            
            if(containerGO != null && containerGO.activeInHierarchy)
            containerTransform = containerGO.transform;
            
            else
            Debug.LogWarning("Active MusicUIContainer with tag 'MusicUIContainer' not found in MainMenu.");
        }

        if(containerTransform == null)
        {
            Debug.LogWarning("No valid active container for Music UI.");
            return;
        }

        foreach(Transform child in containerTransform)
        Destroy(child.gameObject);

        if(musicUIPrefab == null)
        {
            Debug.LogError("Music UI Prefab not assigned in MusicPlayer!");
            return;
        }

        GameObject uiInstance = Instantiate(musicUIPrefab, containerTransform);
        uiInstance.transform.localPosition = Vector3.zero;

        volumeSlider = uiInstance.GetComponentInChildren<Slider>();
        playPauseButton = FindButton(uiInstance, "PausePlayButton");
        nextButton = FindButton(uiInstance, "NextTreckButton");
        prevButton = FindButton(uiInstance, "PrevTreckButton");
        trackNameText = uiInstance.GetComponentInChildren<TMP_Text>();
        volumePercentText = FindText(uiInstance, "VolumePercent");

        if(volumeSlider != null)
        {
            volumeSlider.value = GetLinearVolume();
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }

        if(playPauseButton != null)
        {
            playPauseButton.onClick.RemoveAllListeners();
            playPauseButton.onClick.AddListener(TogglePlayPause);
        }

        if(nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextTrack);
        }

        if(prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(PrevTrack);
        }


        UpdateUI();
        UpdateVolumePercent(GetLinearVolume());
    }

    private float LinearToDecibels(float linear)
    {
        return Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
    }

    // Преобразование из децибел в линейное значение
    private float DecibelsToLinear(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }

    // Установка громкости (линейное значение)
    private void SetVolume(float linearVolume)
    {
        float db = LinearToDecibels(linearVolume);
        
        if(masterMixer != null)
        masterMixer.SetFloat("MasterVolume", db);
        
        else
        audioSource.volume = linearVolume;
    }

    // Получение текущей линейной громкости из микшера
    private float GetLinearVolume()
    {
        if(masterMixer != null)
        {
            masterMixer.GetFloat("MasterVolume", out float db);
            return DecibelsToLinear(db);
        }
        
        else
        return audioSource.volume;
    }
    private Button FindButton(GameObject root, string buttonName)
    {
        Transform t = root.transform.Find(buttonName);
        return t != null ? t.GetComponent<Button>() : null;
    }

    private TMP_Text FindText(GameObject root, string textName)
    {
        Transform t = root.transform.Find(textName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    private void LoadSettings()
    {
        float savedLinear = PlayerPrefs.GetFloat(VOLUME_KEY, 0.5f);
        SetVolume(savedLinear);
        currentTrackIndex = PlayerPrefs.GetInt(TRACK_INDEX_KEY, 0);
        isPlaying = PlayerPrefs.GetInt(IS_PLAYING_KEY, 1) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, audioSource.volume);
        PlayerPrefs.SetInt(TRACK_INDEX_KEY, currentTrackIndex);
        PlayerPrefs.SetInt(IS_PLAYING_KEY, isPlaying ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeVolume(float value)
    {
        SetVolume(value);
        UpdateVolumePercent(value);
        SaveSettings();
        Debug.Log("Volume changed to: " + value); // для проверки
    }

    private void UpdateVolumePercent(float value)
    {
        if(volumePercentText != null)
        volumePercentText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void TogglePlayPause()
    {
        if(builtInTracks.Length == 0)
        return;

        if(audioSource.isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;
        }
        
        else if(audioSource.clip != null)
        {
            audioSource.UnPause();
            isPlaying = true;
        }
        
        SaveSettings();
        UpdateUI();
    }

    public void NextTrack()
    {
        if(builtInTracks.Length == 0)
        return;
        
        stopRequested = true;
        currentTrackIndex = (currentTrackIndex + 1) % builtInTracks.Length;
        PlayTrack(currentTrackIndex);
        isPlaying = true;
        SaveSettings();
        UpdateUI();
    }

    public void PrevTrack()
    {
        if(builtInTracks.Length == 0)
        return;
        
        stopRequested = true;
        currentTrackIndex = (currentTrackIndex - 1 + builtInTracks.Length) % builtInTracks.Length;
        PlayTrack(currentTrackIndex);
        isPlaying = true;
        SaveSettings();
        UpdateUI();
    }

    private void PlayTrack(int index)
    {
        if(index < 0 || index >= builtInTracks.Length)
        return;
        
        audioSource.clip = builtInTracks[index];
        audioSource.Play();
        StopAllCoroutines();
        StartCoroutine(WaitForTrackEnd());
    }

    private IEnumerator WaitForTrackEnd()
    {
        while(audioSource.isPlaying)
        yield return null;
        
        if(!stopRequested)
        NextTrack();
        stopRequested = false;
    }

    private void UpdateUI()
    {
        if(trackNameText != null && builtInTracks.Length > 0 && currentTrackIndex < builtInTracks.Length)
        trackNameText.text = builtInTracks[currentTrackIndex].name;

        if(playPauseButton != null)
        {
            TMP_Text btnText = playPauseButton.GetComponentInChildren<TMP_Text>();
            
            if(btnText != null)
            btnText.text = isPlaying ? "Pause" : "Play";
        }
    }
}