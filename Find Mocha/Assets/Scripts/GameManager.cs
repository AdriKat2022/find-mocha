using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    public string playerName;

    [SerializeField]
    private int nStoryLevels = 1;
    [SerializeField]
    private int nBonusLevels = 1;
    private int currentLevelIndex;
    private int sceneLoadNumber = 0;


    static public GameManager Instance;
    static public GameObject Player;

    private PlayerController playerController;

    private PlayerStats playerLastStats;
    private bool keepLastStats;

    private bool canButtonInteract;
    private bool canPauseGame;
    private bool inHitStop;
    private bool inGame;

    private PauseMenuManager pauseMenuManager;
    [SerializeField]
    private MainMenuManager mainMenuManager;
    [SerializeField]
    private WinScreenManager winScreen;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private DialogueUI dialogueUI;
    [SerializeField]
    private GameObject bigNonoPanel;


    public void EnableWinScreen()
    {
        winScreen.gameObject.SetActive(true);
    }

    #region Events

    public static event Action<bool> OnSceneLoaded; // True = main menu ; False = in game
    public static event Action OnTrueReset;

    #endregion

    #region Giveaway variables

    public DialogueUI DialogueUI => dialogueUI;
    public bool CanPauseGame => canPauseGame;
    public bool CanButtonInteract => canButtonInteract;
    public int CurrentSceneIndex => currentLevelIndex;
    public int SceneLoadNumber => sceneLoadNumber;
    public bool InGame => inGame;
    #endregion

    private int currentSceneIndex => SceneManager.GetActiveScene().buildIndex;


    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inGame = currentSceneIndex != 0;

        SetCanButtonInteract(true);
    }

    private void Start()
    {
        winScreen.gameObject.SetActive(false);
        pauseMenuManager = GetComponent<PauseMenuManager>();

        Player = GameObject.Find(playerName);
        if (Player != null)
            playerController = Player.GetComponent<PlayerController>();

        currentLevelIndex = currentSceneIndex;

        SetSceneParameters(currentSceneIndex);

        inHitStop = false;
    }

    public void SetCanPause(bool value) => canPauseGame = value;

    public void SetCanButtonInteract(bool active, float time = 0f)
    {
        canButtonInteract = active;
        bigNonoPanel.SetActive(!active);

        if (time > 0f && !active)
            StartCoroutine(UnblockAfter(time));
    }

    public void MoveToScene(int index)
    {
        print("DEBUG METHOD : Moving to scene " + index);
        SceneManager.LoadScene(index);
    }

    public void StartBonusLevel(int number)
    {
        LoadLevelIndex(nStoryLevels + number, false, true);
    }

    public void MoveToNextLevel()
    {
        LoadLevelIndex(currentLevelIndex + 1, false);
    }

    private void LoadLevelIndex(int index, bool reload, bool disableSave = false)
    {
        index %= nStoryLevels + nBonusLevels + 1;

        ClearBeforeLoading(saveLastStats: !reload && (index > 1) && !disableSave);
        SceneManager.LoadScene(index);
        
        SetSceneParameters(index);
    }

    private void SetSceneParameters(int index)
    {
        if (index == 0)
        {
            // Main menu
            inGame = false;
            canPauseGame = false;
            mainMenuManager.StartMenu();
            SoundManager.Instance.PlayMusicNow(SoundManager.Instance.mainMenu);
        }
        else
        {
            // In game
            inGame = true;
            canPauseGame = true;
            mainMenuManager.HideMenu();
            SoundManager.Instance.PlayMusicNow(SoundManager.Instance.defaultLevel);
            print("game is launched");
        }

        currentLevelIndex = index;

        sceneLoadNumber++;

        OnSceneLoaded?.Invoke(index == 0);
    }

    private void PlayAssociatedMusic()
    {
        // TODO

        // Check if music is not already the good one (in that case don't change it)
    }

    public void ReloadLevel() {
        LoadLevelIndex(SceneManager.GetActiveScene().buildIndex, true);
    }

    private IEnumerator UnblockAfter(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SetCanButtonInteract(true);
    }

    public void ToMainMenu()
    {
        // Complete reset
        OnTrueReset?.Invoke();
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        SceneManager.MoveGameObjectToScene(SoundManager.Instance.gameObject, SceneManager.GetActiveScene());
        LoadLevelIndex(0, false);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    public void CreateHitStop(float duration)
    {
        if(inHitStop) return;

        canPauseGame = false;
        inHitStop = true;
        StartCoroutine(HitStop(duration));
    }

    private IEnumerator HitStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;

        canPauseGame = true;
        inHitStop = false;
    }

    private void ClearBeforeLoading(bool saveLastStats)
    {
        uiManager.ActivateHPBar(false);
        uiManager.SetHPAuraActive(false);
        if (saveLastStats)
        {
            SavePlayerStats();
            keepLastStats = true;
        }

        RemovePauseScreen();
    }

    private void SavePlayerStats() => playerLastStats = playerController.GetPlayerStats();
    
    private void LoadPlayerStats() => playerController.LoadPlayerStats(playerLastStats); // Deprecated

    public bool CheckPlayerStats(PlayerController pc, out PlayerStats stats)
    {

        playerController = pc;

        stats = default;
        if(!keepLastStats)
            return false;

        keepLastStats = false; // Reset this for next time
        stats = playerLastStats;
        return true;
    }

    private void RemovePauseScreen()
    {
        pauseMenuManager.UnpauseGame();
    }
}
