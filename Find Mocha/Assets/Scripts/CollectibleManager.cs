using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{

    #region Static fields
    public static CollectibleManager Instance { get; private set; }

    public static int TotalCoinsReferenced { get; private set; }
    public static int TotalCoinsCollected { get; private set; }
    public static int TotalDepressions { get; private set; }
    public static float TotalTimeSeconds { get; private set; }

    #endregion

    [SerializeField]
    private TMP_Text coinTextCount;
    [SerializeField]
    private GameObject coinDisplayer;


    private int nLevelCoinsCollected;
    private int nLevelCoinsReferenced;
    private bool isStopWatchRunning = false;

    #region Event Subscribtions

    private void OnEnable()
    {
        FlagManager.OnPlayerWin += OnLevelWin;
        PlayerController.OnPlayerKnockedOut += OnLevelLose;
        PlayerController.OnPlayerReady += OnLevelStart;
        GameManager.OnSceneLoaded += OnSceneLoad;
        GameManager.OnTrueReset += TrueReset;
    }
    private void OnDisable()
    {
        FlagManager.OnPlayerWin -= OnLevelWin;
        PlayerController.OnPlayerKnockedOut -= OnLevelLose;
        PlayerController.OnPlayerReady -= OnLevelStart;
        GameManager.OnSceneLoaded -= OnSceneLoad;
        GameManager.OnTrueReset -= TrueReset;
    }

    #endregion

    private void Awake()
    {
        ToogleCoinNumber(false);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


        nLevelCoinsCollected = 0;
        nLevelCoinsReferenced = 0;
    }

    private void Update()
    {
        if (isStopWatchRunning)
            TotalTimeSeconds += Time.deltaTime;
    }

    #region Event functions

    private void OnSceneLoad(bool isMainMenu)
    {
        nLevelCoinsCollected = 0;
        nLevelCoinsReferenced = 0;

        if (isMainMenu)
            ToogleCoinNumber(false);
        else
            ToogleCoinNumber(true);
    }
    private void OnLevelWin()
    {
        TotalCoinsReferenced += nLevelCoinsReferenced;
        TotalCoinsCollected += nLevelCoinsCollected;

        nLevelCoinsCollected = 0;
        nLevelCoinsReferenced = 0;

        ToogleStopwatch(false);
    }
    private void OnLevelLose()
    {
        nLevelCoinsCollected = 0;
        nLevelCoinsReferenced = 0;
        TotalDepressions++;
        ToogleStopwatch(false);
    }
    private void OnLevelStart()
    {
        ShowCoinNumber();
        ToogleStopwatch(true);
    }
    
    #endregion

    #region Public commands
    public void RegisterCollectibleForLevel()
    {
        nLevelCoinsReferenced++;
        UpdateUI();
    }
    public void AddCoin(int value)
    {
        nLevelCoinsCollected += value;
        UpdateUI();
    }
    public void ToogleCoinNumber(bool show)
    {
        coinDisplayer.SetActive(show);

        UpdateUI();
    }
    public void ShowCoinNumber()
    {
        ToogleCoinNumber(true);
    }
    #endregion

    private void TrueReset()
    {
        nLevelCoinsCollected = 0;
        nLevelCoinsReferenced = 0;
        TotalCoinsCollected = 0;
        TotalCoinsReferenced = 0;
        TotalDepressions = 0;
        TotalTimeSeconds = 0;
    }

    private void UpdateUI()
    {
        coinTextCount.text = (TotalCoinsCollected+nLevelCoinsCollected).ToString();
    }

    private void ToogleStopwatch(bool toggle)
    {
        isStopWatchRunning = toggle;
    }
}
