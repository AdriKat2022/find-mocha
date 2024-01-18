using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{

    #region Static fields
    public static CollectibleManager Instance { get; private set; }

    public static int TotalCoinsReferenced { get; private set; }
    public static int TotalCoinsCollected { get; private set; }

    #endregion

    [SerializeField]
    private TMP_Text coinTextCount;
    [SerializeField]
    private TMP_Text coinText;


    private int nLevelCoinsCollected;
    private int nLevelCoinsReferenced;


    #region Event Subscribtions

    private void OnEnable()
    {
        FlagManager.OnPlayerWin += OnLevelWin;
        PlayerController.OnPlayerKnockedOut += OnLevelLose;
        PlayerController.OnPlayerReady += ShowCoinNumber;
        GameManager.OnSceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        FlagManager.OnPlayerWin -= OnLevelWin;
        PlayerController.OnPlayerKnockedOut -= OnLevelLose;
        PlayerController.OnPlayerReady -= ShowCoinNumber;
        GameManager.OnSceneLoaded -= OnSceneLoad;
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

    #region Event functions

    private void OnSceneLoad(bool isMainMenu)
    {
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
    }
    private void OnLevelLose()
    {
        nLevelCoinsCollected = 0;
        nLevelCoinsReferenced = 0;
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
        if (show)
        {
            coinText.alpha = 1;
            coinTextCount.alpha = 1;
        }
        else
        {
            coinText.alpha = 0;
            coinTextCount.alpha = 0;
        }

        UpdateUI();
    }
    public void ShowCoinNumber()
    {
        ToogleCoinNumber(true);
    }
    #endregion

    private void UpdateUI()
    {
        coinTextCount.text = (TotalCoinsCollected+nLevelCoinsCollected).ToString();
    }

}
