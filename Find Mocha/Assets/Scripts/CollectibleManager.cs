using System;
using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    public static int TotalCoinsReferenced { get; private set; }
    public static int TotalCoinsCollected { get; private set; }

    //public static event Action<int> OnCollectibleCollected;

    [SerializeField]
    private TMP_Text coinText;


    private int nLevelCoinsCollected;
    private int nLevelCoinsReferenced;


    private void OnEnable()
    {
        FlagManager.OnPlayerWin += OnLevelWin;
        PlayerController.OnPlayerKnockedOut += OnLevelLose;
        PlayerController.OnPlayerReady += ShowCoinNumber;
    }
    private void OnDisable()
    {
        FlagManager.OnPlayerWin -= OnLevelWin;
        PlayerController.OnPlayerKnockedOut -= OnLevelLose;
        PlayerController.OnPlayerReady -= ShowCoinNumber;
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        ToogleCoinNumber(false);

        nLevelCoinsCollected = 0;
        nLevelCoinsReferenced = 0;
    }

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
            coinText.alpha = 1;
        else
            coinText.alpha = 0;

        UpdateUI();
    }
    public void ShowCoinNumber()
    {
        ToogleCoinNumber(true);
    }
    #endregion

    private void UpdateUI()
    {
        coinText.text = (TotalCoinsCollected+nLevelCoinsCollected).ToString();
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
}
