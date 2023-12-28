using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    public string playerName;
    public int levelsNb = 1;

    private int currentLevelIndex;


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
    public bool InGame => inGame;
    #endregion

    private int currentSceneIndex => SceneManager.GetActiveScene().buildIndex;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


    }

    private void Start()
    {
        pauseMenuManager = GetComponent<PauseMenuManager>();

        Player = GameObject.Find(playerName);
        if (Player != null)
            playerController = Player.GetComponent<PlayerController>();

        
        DontDestroyOnLoad(gameObject);
        

        currentLevelIndex = currentSceneIndex;

        if (currentSceneIndex == 0)
        {
            mainMenu.SetActive(true);
            canPauseGame = false;
            SetCanButtonInteract(false, 7f);
            inGame = false;
        }
        else
        {
            mainMenu.SetActive(false);
            canPauseGame = true;
            SetCanButtonInteract(true);
            inGame = true;
        }

        uiManager.ActivateHPBar(currentSceneIndex != 0);

        inHitStop = false;
    }


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
        //if(index > 1 && !reload)
        //{
        //    LoadPlayerStats();
        //}
        inGame = index != 0;
        currentLevelIndex = index;
        SetCanButtonInteract(index != 0, 7f);
        mainMenu.SetActive(index == 0);
        canPauseGame = index != 0;
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
        //Debug.Log("HitStop");
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        //Debug.Log("HitResume");
        Time.timeScale = 1f;

        canPauseGame = true;
        inHitStop = false;
    }

    private void ClearBeforeLoading(bool saveLastStats)
    {
        uiManager.ActivateHPBar(false);
        if (saveLastStats)
        {
            SavePlayerStats();
            keepLastStats = true;
        }

        RemovePauseScreen();
        SoundManager.Instance.StopMusic();
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
