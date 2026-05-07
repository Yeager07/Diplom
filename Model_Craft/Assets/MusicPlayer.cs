using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Audio;  
using SFB;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    public GameObject musicUIPrefab_Full;
    public GameObject musicUIPrefab_Simple;
    public GameObject trackCardPrefab;

    public AudioClip[] builtInTracks;
    public AudioMixer masterMixer;
    public string userMusicFolder = "UserMusic";

    // Приватные компоненты
    private AudioSource audioSource;
    private Slider volumeSlider;
    private Button playPauseButton;
    private Button nextButton;
    private Button prevButton;
    private TMP_Text trackNameText;
    private TMP_Text volumePercentText;
    private Transform trackListContent;
    private Button uploadButton;
    private Button deleteButton;

    // Списки треков
    private List<string> userTrackPaths = new List<string>();
    private List<string> combinedTrackNames = new List<string>();
    private List<string> combinedTrackSources = new List<string>();
    private List<GameObject> trackCards = new List<GameObject>();
    private int currentCombinedIndex = 0;
    private bool isPlaying = true;
    private bool stopRequested = true;
    private bool isLoadingTrack = false;

    // Константы сохранения
    private const string VOLUME_KEY = "MusicVolume";
    private const string COMBINED_INDEX_KEY = "MusicCombinedIndex";
    private const string IS_PLAYING_KEY = "MusicIsPlaying";

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if(audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        LoadSettings();

        ScanAndLoadUserTracks();
        BuildCombinedTrackList();

        if(combinedTrackSources.Count > 0)
        {
            if(currentCombinedIndex >= combinedTrackSources.Count)
            currentCombinedIndex = 0;
            
            OnTrackSelected(currentCombinedIndex);
        }
    }

    private void ScanAndLoadUserTracks()
    {
        string path = Path.Combine(Application.persistentDataPath, userMusicFolder);
        
        if(!Directory.Exists(path))
        Directory.CreateDirectory(path);

        var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        userTrackPaths.Clear();
        
        foreach(var file in files)
        {
            string ext = Path.GetExtension(file).ToLower();
            
            if(ext == ".mp3" || ext == ".ogg" || ext == ".wav")
            userTrackPaths.Add(file);
        }
    }

    private void BuildCombinedTrackList()
    {
        combinedTrackNames.Clear();
        combinedTrackSources.Clear();

        for(int i = 0; i < builtInTracks.Length; i++)
        {
            combinedTrackNames.Add(builtInTracks[i].name);
            combinedTrackSources.Add("builtin:" + i);
        }

        foreach(var path in userTrackPaths)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            combinedTrackNames.Add(name);
            combinedTrackSources.Add(path);
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
        bool isFullUI = false;

        if(player != null && player.typeGame != "MainMenu")
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
            {
                containerTransform = containerGO.transform;
                isFullUI = true;
            }
            
            else
            {
                Debug.LogWarning("Active MusicUIContainer with tag 'MusicUIContainer' not found in MainMenu.");
                return;
            }
        }

        if(containerTransform == null)
        {
            Debug.LogWarning("No valid active container for Music UI.");
            return;
        }

        foreach(Transform child in containerTransform)
        Destroy(child.gameObject);

        GameObject uiInstance = null;

        if(isFullUI && musicUIPrefab_Full != null)
        uiInstance = Instantiate(musicUIPrefab_Full, containerTransform);
        
        else if(!isFullUI && musicUIPrefab_Simple != null)
        uiInstance = Instantiate(musicUIPrefab_Simple, containerTransform);
        
        else
        {
            Debug.LogError("Music UI Prefab is not assigned for the current context!");
            return;
        }
        
        uiInstance.transform.localPosition = Vector3.zero;

        volumeSlider = uiInstance.GetComponentInChildren<Slider>();
        playPauseButton = FindButton(uiInstance, "PausePlayButton");
        nextButton = FindButton(uiInstance, "NextTreckButton");
        prevButton = FindButton(uiInstance, "PrevTreckButton");
        trackNameText = uiInstance.GetComponentInChildren<TMP_Text>();
        volumePercentText = FindText(uiInstance, "VolumePercent");

        if(!isFullUI)
        trackNameText = uiInstance.GetComponentInChildren<TMP_Text>();

        if(isFullUI)
        {
            trackListContent = uiInstance.transform.Find("TrackListScrollView/Viewport/Content");
            uploadButton = FindButton(uiInstance, "DownloadTreckButton");
            
            if(uploadButton != null)
            uploadButton.onClick.AddListener(UploadTrack);
        }

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

        if(isFullUI && trackListContent != null && trackCardPrefab != null)
        {
            BuildTrackList();
            HighlightCurrentTrack();
        }


        UpdateUI();
        UpdateVolumePercent(GetLinearVolume());
    }

    private void BuildTrackList()
    {
        // Очищаем старые карточки
        foreach(var card in trackCards)
        {
            if(card != null)
            Destroy(card);
        }
        
        trackCards.Clear();

        for(int i = 0; i < combinedTrackNames.Count; i++)
        {
            int index = i;
            GameObject card = Instantiate(trackCardPrefab, trackListContent);
            trackCards.Add(card);

            Button playBtn = card.GetComponentInChildren<Button>();
            TMP_Text nameText = playBtn?.GetComponentInChildren<TMP_Text>();
            
            if(nameText != null)
            nameText.text = combinedTrackNames[index];
            
            if(playBtn != null)
            playBtn.onClick.AddListener(() => OnTrackSelected(index));

            Button deleteBtn = card.transform.Find("DeleteButton")?.GetComponent<Button>();
            
            if(deleteBtn != null)
            {
                bool isBuiltin = combinedTrackSources[index].StartsWith("builtin:");
                deleteBtn.gameObject.SetActive(!isBuiltin);
                
                if(!isBuiltin)
                deleteBtn.onClick.AddListener(() => DeleteUserTrackByIndex(index));
            }
        }
    }

    private void HighlightCurrentTrack()
    {
        if(trackCards == null)
        return;
        
        for(int i = 0; i < trackCards.Count; i++)
        {
            Button btn = trackCards[i].GetComponentInChildren<Button>();
            
            if(btn != null)
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = (i == currentCombinedIndex) ? Color.green : Color.white;
                btn.colors = colors;
            }
        }
    }

    public void OnTrackSelected(int index)
    {
        if(isLoadingTrack)
        return;
        
        if(index < 0 || index >= combinedTrackSources.Count)
        return;
        
        currentCombinedIndex = index;
        string source = combinedTrackSources[index];

        if(source.StartsWith("builtin:"))
        {
            int builtinIdx = int.Parse(source.Substring(8));
            PlayBuiltinTrack(builtinIdx);
        }
        
        else
        StartCoroutine(LoadAndPlayUserTrack(source));
        
     
        SaveSettings();
        UpdateUI();
        HighlightCurrentTrack();
    }

    private void PlayBuiltinTrack(int idx)
    {
        if(idx < 0 || idx >= builtInTracks.Length)
        return;
        
        audioSource.clip = builtInTracks[idx];
        audioSource.Play();
        stopRequested = false;
        isPlaying = true;
    }

    private IEnumerator LoadAndPlayUserTrack(string filePath)
    {
        isLoadingTrack = true;
        
        using(UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, GetAudioTypeFromExtension(filePath)))
        {
            yield return uwr.SendWebRequest();
        
            if(uwr.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                clip.name = Path.GetFileNameWithoutExtension(filePath);
                
                if(audioSource.clip != null && !IsBuiltinClip(audioSource.clip))
                Destroy(audioSource.clip);
                
                audioSource.clip = clip;
                audioSource.Play();
                stopRequested = false;
                isPlaying = true;
            }
            
            else
            Debug.LogError("Failed to load user track: " + uwr.error);
        }
        
        isLoadingTrack = false;
        UpdateUI();
    }

    private bool IsBuiltinClip(AudioClip clip)
    {
        foreach(var builtin in builtInTracks)
        {
            if(builtin == clip)
            return true;
        }
        
        return false;
    }

    private AudioType GetAudioTypeFromExtension(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        
        if(ext == ".mp3")
        return AudioType.MPEG;
        
        if(ext == ".ogg")
        return AudioType.OGGVORBIS;
        
        if(ext == ".wav")
        return AudioType.WAV;
        
        return AudioType.UNKNOWN;
    }

    public void UploadTrack()
    {
        if(uploadButton == null)
        return;

        var extensions = new[] { new ExtensionFilter("Audio Files", "mp3", "ogg", "wav") };
        StandaloneFileBrowser.OpenFilePanelAsync("Выберите аудиофайл", "", extensions, false, (string[] paths) =>
        {
            if(paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                string destDir = Path.Combine(Application.persistentDataPath, userMusicFolder);
                
                if(!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
                
                string destFile = Path.Combine(destDir, Path.GetFileName(paths[0]));
                File.Copy(paths[0], destFile, true);
                
                ScanAndLoadUserTracks();
                BuildCombinedTrackList();
                
                if(trackListContent != null)
                BuildTrackList();

                if(audioSource.clip == null && combinedTrackSources.Count > 0)
                OnTrackSelected(0);
                
                else
                HighlightCurrentTrack();
            }
        });
    }
    
    private void DeleteUserTrackByIndex(int index)
    {
        string source = combinedTrackSources[index];
        
        if(source.StartsWith("builtin:"))
        return;

        if(File.Exists(source))
        File.Delete(source);

        if(currentCombinedIndex == index)
        {
            audioSource.Stop();
            int newIndex = (index == 0) ? 1 : 0;
          
            if(newIndex >= 0 && newIndex < combinedTrackSources.Count)
            OnTrackSelected(newIndex);
            
            else if(builtInTracks.Length > 0)
            PlayBuiltinTrack(0);
            
            else
            audioSource.clip = null;
        }

        ScanAndLoadUserTracks();
        BuildCombinedTrackList();
        BuildTrackList();

        if(currentCombinedIndex >= combinedTrackSources.Count)
        currentCombinedIndex = combinedTrackSources.Count - 1;
        
        HighlightCurrentTrack();
        SaveSettings();
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
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 0.5f);
        SetVolume(savedVolume);
        currentCombinedIndex = PlayerPrefs.GetInt(COMBINED_INDEX_KEY, 0);
        isPlaying = PlayerPrefs.GetInt(IS_PLAYING_KEY, 1) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, GetLinearVolume());
        PlayerPrefs.SetInt(COMBINED_INDEX_KEY, currentCombinedIndex);
        PlayerPrefs.SetInt(IS_PLAYING_KEY, isPlaying ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeVolume(float value)
    {
        SetVolume(value);
        UpdateVolumePercent(value);
        SaveSettings();
    }

    private void UpdateVolumePercent(float value)
    {
        if(volumePercentText != null)
        volumePercentText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void TogglePlayPause()
    {
        if(combinedTrackSources.Count == 0)
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
        if(combinedTrackSources.Count == 0)
        return;
        
        stopRequested = true;
        int next = (currentCombinedIndex + 1) % combinedTrackSources.Count;
        OnTrackSelected(next);
        isPlaying = true;
    }

    public void PrevTrack()
    {
        if(combinedTrackSources.Count == 0)
        return;
        
        stopRequested = true;
        int prev = (currentCombinedIndex - 1 + combinedTrackSources.Count) % combinedTrackSources.Count;
        OnTrackSelected(prev);
        isPlaying = true;
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
        if(trackNameText != null && currentCombinedIndex >= 0 && currentCombinedIndex < combinedTrackNames.Count)
        trackNameText.text = combinedTrackNames[currentCombinedIndex];

        if(playPauseButton != null)
        {
            TMP_Text btnText = playPauseButton.GetComponentInChildren<TMP_Text>();
            
            if(btnText != null)
            btnText.text = isPlaying ? "Pause" : "Play";
        }
    }

    private void OnDestroy()
    {
        if(trackCards != null)
        {
            foreach(var card in trackCards)
            {
                if(card != null)
                Destroy(card);
            }
            
            trackCards.Clear();
        }
        
        if(audioSource != null && audioSource.clip != null)
        {
            if(!IsBuiltinClip(audioSource.clip))
            Destroy(audioSource.clip);
        }
    }
}