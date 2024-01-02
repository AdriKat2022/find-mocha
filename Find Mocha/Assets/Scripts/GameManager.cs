using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    public string playerName;
    public int levelsNb = 1;

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
    private GameObject mainMenu;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private DialogueUI dialogueUI;
    [SerializeField]
    private GameObject bigNonoPanel;


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
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        inGame = currentSceneIndex != 0;
    }

    private void Start()
    {
        pauseMenuManager = GetComponent<PauseMenuManager>();

        Player = GameObject.Find(playerName);
        if (Player != null)
            playerController = Player.GetComponent<PlayerController>();

        
        DontDestroyOnLoad(gameObject);
        


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

    public void MoveToNextLevel()
    {
        LoadLevelIndex(currentLevelIndex + 1, false);
    }

    private void LoadLevelIndex(int index, bool reload)
    {
        index %= levelsNb;

        ClearBeforeLoading(saveLastStats : !reload && index > 1);
        SceneManager.LoadScene(index);
        
        SetSceneParameters(index);
    }


    private void SetSceneParameters(int index)
    {
        inGame = index != 0;
        currentLevelIndex = index;
        SetCanButtonInteract(index != 0, 7f);
        mainMenu.SetActive(index == 0);
        canPauseGame = index != 0;

        if (index == 0)
            SoundManager.Instance.PlayMusicNow(SoundManager.Instance.mainMenu);
        else
            SoundManager.Instance.PlayMusicNow(SoundManager.Instance.defaultLevel);

        sceneLoadNumber++;
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
