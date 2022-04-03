using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public enum UIScreen
    {
        titleScreen,
        inGame,
        settings
    }

    [System.Serializable]
    public class GameVariables
    {
        public bool gamePaused;
        public bool isLoadingLevel;
        public UIScreen activeUIScreen;
        public int titleScreenDepth;
        public Vector2 screenSize;
    }
    [System.Serializable]
    public class GamePublicReferences
    {
        public Transform globalCameraHolderReference;
        public LevelManager currentLevel;
        public AudioMixer mixer;
        public EventSystem eventSystem;
    }
    [System.Serializable]
    public class GameSoundEffects
    {
        public AudioClip[] musicTracks;
        public AudioClip[] generalSfx;
        public AudioClip[] playerSfx;
        public AudioClip[] uiSfx;
    }
    [System.Serializable]
    public class GameSaveData
    {
        // TBD
    }

    // Main class references
    public GameVariables gm_gameVars;
    public GamePublicReferences gm_gameRefs;
    public GameSoundEffects gm_gameSfx;
    public GameSaveData gm_gameSaveData;

    // Audio references
    AudioSource musicSource;
    AudioSource ambienceSource;
    AudioSource sfxSource;

    // UI references
    RectTransform titleScreenPanel;
    RectTransform settingsPanel;
    RectTransform hudPanel;
    TextMeshProUGUI timerText;
    Animator gameOverPopupAnimator;
    TextMeshProUGUI popupButton1Text;
    RectTransform ratArrivalText;
    TextMeshProUGUI popupButtonHeaderText;
    Image blackScreenOverlay;
    RectTransform titleMenu;
    RectTransform levelSelectMenu;
    RectTransform creditsMenu;
    RectTransform quitMenu;
    RectTransform quitButton;
    Slider sfxSlider;
    Slider musicSlider;
    Slider ambSlider;

    // Other references
    NewgroundsUtility ng;
    Transform cam;
    PlayerController ply;

    // Local variables
    bool initialized;

    void Start()
    {
        DontDestroyOnLoad(transform.parent.gameObject);
        GetReferences();
        LoadAudioLevelsFromPlayerPrefs();

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            SceneManager.LoadScene(2);
            PlayMusic(gm_gameSfx.musicTracks[1]);
        }
        else
            LevelLoadedUpdates(SceneManager.GetActiveScene().buildIndex);

        StartCoroutine(FadeFromBlack());
        initialized = true;
    }
    void Update()
    {
        if (!initialized)
            return;

        UpdateUI();

        if (Input.GetKeyDown(KeyCode.F4))
            SetFullscreenMode(!Screen.fullScreen);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gm_gameVars.gamePaused)
            {
                CheckAndPlayClip("GameOverPopup_Default", gameOverPopupAnimator);
                Time.timeScale = 1;
            }
            else
            {
                ShowGameOverPopup(2);
            }

            gm_gameVars.gamePaused = !gm_gameVars.gamePaused;
        }

    }

    public void CheckAndPlayClip(string clipName, Animator anim)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(clipName))
        {
            anim.Play(clipName);
        }
    } // Plays animation clip if it isn't already playing
    public IEnumerator FadeFromBlack()
    {
        blackScreenOverlay.rectTransform.anchoredPosition = Vector2.zero;
        while (blackScreenOverlay.color.a > 0)
        {
            blackScreenOverlay.color = new Color(0, 0, 0, blackScreenOverlay.color.a - 0.075f);
            yield return new WaitForSecondsRealtime(0.025f);
        }
        blackScreenOverlay.rectTransform.anchoredPosition = Vector2.up * 1000;
    }
    public IEnumerator FadeToBlack()
    {
        blackScreenOverlay.rectTransform.anchoredPosition = Vector2.zero;
        while (blackScreenOverlay.color.a < 1)
        {
            blackScreenOverlay.color = new Color(0, 0, 0, blackScreenOverlay.color.a + 0.075f);
            yield return new WaitForSecondsRealtime(0.025f);
        }
    }
    void GetReferences()
    {
        cam = gm_gameRefs.globalCameraHolderReference.GetChild(0);

        // UI references
        blackScreenOverlay = GameObject.Find("BlackScreenOverlay").GetComponent<Image>();
        sfxSlider = GameObject.Find("SFXVolumeSlider").GetComponent<Slider>();
        ambSlider = GameObject.Find("AmbienceVolumeSlider").GetComponent<Slider>();
        musicSlider = GameObject.Find("MusicVolumeSlider").GetComponent<Slider>();
        popupButton1Text = GameObject.Find("PopupButton1Text").GetComponent<TextMeshProUGUI>();
        popupButtonHeaderText = GameObject.Find("PopupButtonHeaderText").GetComponent<TextMeshProUGUI>();
        hudPanel = GameObject.Find("HUDPanel").GetComponent<RectTransform>();
        settingsPanel = GameObject.Find("SettingsPanel").GetComponent<RectTransform>();
        titleScreenPanel = GameObject.Find("TitleScreenPanel").GetComponent<RectTransform>();
        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
        gameOverPopupAnimator = GameObject.Find("GameOverPopup").GetComponent<Animator>();
        titleMenu = GameObject.Find("TopMenuPanel").GetComponent<RectTransform>();
        levelSelectMenu = GameObject.Find("LevelSelectPanel").GetComponent<RectTransform>();
        creditsMenu = GameObject.Find("CreditsPanel").GetComponent<RectTransform>();
        quitMenu = GameObject.Find("QuitPanel").GetComponent<RectTransform>();
        ratArrivalText = GameObject.Find("RatsArrivedWarning").GetComponent<RectTransform>();
        quitButton = GameObject.Find("QuitButton").GetComponent<RectTransform>();
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            quitButton.anchoredPosition = new Vector2(0, -282.5f);

        // Audio references
        musicSource = GameObject.Find("GameMusicSource").GetComponent<AudioSource>();
        sfxSource = GameObject.Find("GameSFXSource").GetComponent<AudioSource>();
        ambienceSource = GameObject.Find("GameAmbienceSource").GetComponent<AudioSource>();

        ng = FindObjectOfType<NewgroundsUtility>();
    } // Obtain UI + GameObject references. Called by Start() and probably nowhere else
    public void InitializePlayer() { } // Readies / unfreezes player gameobject in-game
    public void SetPausedState(bool paused) { } // Pauses / unpauses game and performs necessary UI stuff
    public void UnlockMedal(int id)
    {
        ng.UnlockMedal(id);
    } // Newgrounds API, self-explanatory
    public void LoadLevel(int buildIndex)
    {
        if (gm_gameVars.isLoadingLevel)
            return;

        gm_gameVars.isLoadingLevel = true;
        StartCoroutine(LoadLevelCoroutine(buildIndex));
    }
    IEnumerator LoadLevelCoroutine(int buildIndex)
    {
        yield return FadeToBlack();
        Time.timeScale = 1;
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        while (!asyncLoadLevel.isDone)
            yield return null;
        LevelLoadedUpdates(buildIndex);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return FadeFromBlack();
        gm_gameVars.isLoadingLevel = false;
    }

    void LevelLoadedUpdates(int buildIndex)
    {
        // Deselect buttons
        gm_gameRefs.eventSystem.SetSelectedGameObject(null);
        gm_gameVars.gamePaused = false;

        if (buildIndex == 2)
        {
            gm_gameVars.activeUIScreen = UIScreen.titleScreen;
            PlayMusic(gm_gameSfx.musicTracks[1]);
        }
        else
        {
            if (musicSource.clip != gm_gameSfx.musicTracks[0])
                musicSource.Stop();

            gameOverPopupAnimator.Play("GameOverPopup_Default", 0, 0);
            gm_gameRefs.currentLevel = FindObjectOfType<LevelManager>();
            ply = FindObjectOfType<PlayerController>();
            gm_gameVars.activeUIScreen = UIScreen.inGame;
        }
    }

    void LoadAudioLevelsFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("AMB_VOLUME"))
        {
            musicSlider.value = PlayerPrefs.GetInt("MUS_VOLUME") / 4;
            sfxSlider.value = PlayerPrefs.GetInt("SFX_VOLUME") / 4;
            ambSlider.value = PlayerPrefs.GetInt("AMB_VOLUME") / 4;
        }

        UpdateMusicVolume();
        UpdateSFXVolume(false);
        UpdateAmbienceVolume();

    } // Sets audio levels to match stored values in PlayerPrefs
    public void PlaySFX(AudioClip sfx)
    {
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayMusic(AudioClip track)
    {
        if (musicSource.clip == track && musicSource.isPlaying)
            return;
        musicSource.clip = track;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    void ReadSaveData()
    {
        string prefix = @"idbfs/" + Application.productName;

        if (Application.platform == RuntimePlatform.WindowsPlayer)
            prefix = Application.persistentDataPath;

        if (!File.Exists(prefix + @"/savedata.json"))
        {
            gm_gameSaveData = new GameSaveData();
            return;
        }
        string json = File.ReadAllText(prefix + @"/savedata.json");
        gm_gameSaveData = JsonUtility.FromJson<GameSaveData>(json);
    }
    void SetFullscreenMode(bool isFullscreen)
    {
        int width = (int)gm_gameVars.screenSize.x;
        int height = (int)gm_gameVars.screenSize.y;
        if (isFullscreen)
        {
            width = Screen.currentResolution.width;
            height = Screen.currentResolution.height;
        }

        Screen.SetResolution(width, height, isFullscreen);
    } // Should work fine without AspectRatioController
    public void ScreenShake()
    {
        StartCoroutine(ScreenShakeCoroutine(5));
    }
    public void ScreenShake(float intensity)
    {
        StartCoroutine(ScreenShakeCoroutine(intensity));
    }
    IEnumerator ScreenShakeCoroutine(float intensity)
    {
        for (int i = 0; i < 10; i++)
        {
            cam.localPosition = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)) * intensity;
            intensity /= 1.25f;
            yield return new WaitForFixedUpdate();
        }
        cam.localPosition = Vector2.zero;
    }

    void UpdateUI()
    {
        switch (gm_gameVars.activeUIScreen)
        {
            case UIScreen.titleScreen:
                hudPanel.anchoredPosition = Vector2.up * 1000;
                settingsPanel.anchoredPosition = Vector2.up * 1000;
                titleScreenPanel.anchoredPosition = Vector2.zero;

                Vector2 right = Vector2.right * 2000;
                titleMenu.anchoredPosition = right;
                levelSelectMenu.anchoredPosition = right;
                creditsMenu.anchoredPosition = right;
                quitMenu.anchoredPosition = right;

                switch (gm_gameVars.titleScreenDepth)
                {
                    case 0:
                        titleMenu.anchoredPosition = Vector2.zero;
                        break;
                    case 1:
                        levelSelectMenu.anchoredPosition = Vector2.zero;
                        break;
                    case 2:
                        gm_gameVars.activeUIScreen = UIScreen.settings;
                        break;
                    case 3:
                        creditsMenu.anchoredPosition = Vector2.zero;
                        break;
                    case 4:
                        quitMenu.anchoredPosition = Vector2.zero;
                        break;
                }

                break;

            case UIScreen.settings:
                settingsPanel.anchoredPosition = Vector2.zero;
                hudPanel.anchoredPosition = Vector2.up * 1000;
                titleScreenPanel.anchoredPosition = Vector2.up * 1000;
                break;

            case UIScreen.inGame:
                hudPanel.anchoredPosition = Vector2.zero;
                settingsPanel.anchoredPosition = Vector2.up * 1000;
                titleScreenPanel.anchoredPosition = Vector2.up * 1000;

                if (gm_gameRefs.currentLevel != null)
                {
                    float timer = gm_gameRefs.currentLevel.runtime;
                    int minutes = Mathf.FloorToInt(timer / 60F);
                    int seconds = Mathf.FloorToInt(timer - minutes * 60);
                    int milliseconds = Mathf.FloorToInt(((timer - (minutes * 60) - seconds)) * 100);
                    string niceTime = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);


                    float delta = timer / gm_gameRefs.currentLevel.startingTime;
                    float r = Mathf.MoveTowards(1, 0, delta);
                    float g = Mathf.MoveTowards(0, 1, delta);
                    float b = 0;

                    timerText.color = new Color(r, g, b, 1);
                    timerText.text = niceTime;

                    if (timer == 0f && !ply.p_states.dead)
                    {
                        ratArrivalText.anchoredPosition = Vector2.down * 64;
                    }
                    else
                        ratArrivalText.anchoredPosition = Vector2.up*250;
                }
                else
                    timerText.text = "";
                break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetTitleScreenDepth(int depth)
    {
        gm_gameVars.titleScreenDepth = depth;
    }

    public void SetActiveUIScreen(int index)
    {
        UIScreen screen = default;
        switch (index)
        {
            case 0:
                screen = UIScreen.inGame;
                break;
            case 1:
                screen = UIScreen.settings;
                break;
            case 2:
                screen = UIScreen.titleScreen;
                break;
        }
        gm_gameVars.activeUIScreen = screen;
    }

    public void ShowGameOverPopup(int screen) // 0 = victory, 1 = game over, 2 = pause
    {
        Time.timeScale = 0;
        if (screen == 0)
        {
            popupButtonHeaderText.text = "Victory";
            popupButton1Text.text = "Continue";
        }
        else if (screen == 1)
        {
            popupButtonHeaderText.text = "Game Over";
            popupButton1Text.text = "Retry";
        }
        else if (screen == 2)
        {
            popupButtonHeaderText.text = "Paused";
            popupButton1Text.text = "Retry";
        }

        CheckAndPlayClip("GameOverPopup_Appear", gameOverPopupAnimator);
    }

    public void RetryOrContinue()
    {
        if (gm_gameRefs.currentLevel.points < gm_gameRefs.currentLevel.requiredPoints)
            LoadLevel(SceneManager.GetActiveScene().buildIndex);
        else
            LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void QuitToTitle()
    {
        LoadLevel(2);
    }

    public void UpdateAmbienceVolume()
    {
        if (ambSlider == null)
            return;

        int volume = (int)ambSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("AMB_VOLUME", volume);
        gm_gameRefs.mixer.SetFloat("AmbienceVolume", volume);
    }
    public void UpdateMusicVolume()
    {
        if (musicSlider == null)
            return;

        int volume = (int)musicSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("MUS_VOLUME", volume);
        gm_gameRefs.mixer.SetFloat("MusicVolume", volume);
    }
    public void UpdateSFXVolume(bool calledFromSlider)
    {
        if (sfxSlider == null)
            return;

        int volume = (int)sfxSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("SFX_VOLUME", volume);
        gm_gameRefs.mixer.SetFloat("SFXVolume", volume);

        if (calledFromSlider)
        {
            int rand = Random.Range(0, 3);
            PlaySFX(gm_gameSfx.uiSfx[rand]);
        }
    }
    void WriteSaveData()
    {
        // Reason for idbfs prefix: https://itch.io/t/140214/persistent-data-in-updatable-webgl-games (don't question it)
        string prefix = @"idbfs/" + Application.productName;

        if (Application.platform == RuntimePlatform.WindowsPlayer)
            prefix = Application.persistentDataPath;
        else if (!Directory.Exists(prefix))
            Directory.CreateDirectory(prefix);

        string json = JsonUtility.ToJson(gm_gameSaveData);
        File.WriteAllText(prefix + @"/savedata.json", json);
    }
}