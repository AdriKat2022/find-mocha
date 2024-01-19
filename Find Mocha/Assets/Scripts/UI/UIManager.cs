using System.Collections;
using UnityEngine;
using UnityEngine.UI;



public class UIManager : MonoBehaviour
{
    [Header("Gameover settings")]
    [SerializeField]
    private GameObject gameOverScreen;
    [SerializeField]
    private float waitTimeBeforeGameOverScreen = 2f;
    [SerializeField]
    private float waitTimeBeforeLoadingScene = .8f;

    [Header("Low HP settings")]
    //[SerializeField] [Range(0f, 1f)]
    private float lowHpThreshold;
    [SerializeField]
    private Color lowHPColor;

    [Header("Progressive life bar animation")]

    [SerializeField]
    private float hpDeltaDetect;
    [SerializeField]
    private float animationHpDelay;
    [SerializeField]
    private float animationHpSpeed;
    [SerializeField]
    private Color healBg;
    [SerializeField]
    private Color hurtBg;

    [Header("Blink animation")]

    [SerializeField]
    private float blinkLength;
    [SerializeField]
    private Color blinkHurtColor;
    [SerializeField]
    private Color blinkHealColor;
    [SerializeField]
    private Color defaultBlinkColor;
    [SerializeField]

    [Header("Idle bar animation")]

    private float healthBarBounceSpeed;
    [SerializeField]
    private float healthBarBounceDepth;
    [SerializeField]

    [Header("References")]

    private Image hpBarOverlay;
    [SerializeField]
    private Image hpFill;
    [SerializeField]
    private Image hpFillBg;
    [SerializeField]
    private RectTransform hpBarRectTransform;
    [SerializeField]
    private Animator hpBarAnimator;
    [SerializeField]
    private Rainbow hpBarOverlayRainbow;
    [SerializeField]
    private Rainbow hpBarFillRainbow;
    [SerializeField]
    private GameObject hpBar;
    [SerializeField]
    private Animator gameOverMenuAnimator;
    [SerializeField]
    private GameObject screenAura;
    [SerializeField]
    private Image screenAuraImg;


    private readonly float waitTimeBeforeAnimating = 1f;

    private float currentHp;
    private bool isLowHP;
    private float hpChange;
    private float timeSinceLastUpdate;

    private float _time;
    private bool isBlinking;

    private bool inGame;

    private HPBarAnimationState currentHPBarAnimationState;

    private float currentDisplayedHPNormalized;
    private float currentHPNormalized;
    private float hurtBarAmount;

    private enum HPBarAnimationState
    {
        None,
        Healing,
        Hurting
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (GameManager.Instance != null && PlayerController.Instance != null)
            FetchAndUpdateHP();

        if (screenAuraImg != null)
            screenAuraImg.color = lowHPColor;
    }

#endif

    private void OnEnable()
    {
        SetHPAuraActive(false);
        SoundManager.Instance?.StopLoopSound(force: true);

        PlayerController.OnPlayerInvincibility += ActivateInvincibilityVisuals;
        PlayerController.OnPlayerInvincibilityEnd += DeactivateInvincibilityVisuals;
        PlayerController.OnPlayerKnockedOut += PrepareGameOver;
        FlagManager.OnPlayerWin += PrepareWin;
        PlayerController.OnPlayerReady += PrepareStart;
        PlayerController.OnPlayerChangeHP += UpdateCurrentHP;
    }
    private void OnDisable()
    {
        SetHPAuraActive(false);
        
        PlayerController.OnPlayerInvincibility -= ActivateInvincibilityVisuals;
        PlayerController.OnPlayerInvincibilityEnd -= DeactivateInvincibilityVisuals;
        PlayerController.OnPlayerKnockedOut -= PrepareGameOver;
        FlagManager.OnPlayerWin -= PrepareWin;
        PlayerController.OnPlayerReady -= PrepareStart;
        PlayerController.OnPlayerChangeHP -= UpdateCurrentHP;
    }

    private void Awake()
    {
        screenAura.TryGetComponent(out screenAuraImg);
        screenAuraImg.color = lowHPColor;
        inGame = false;
        lowHpThreshold = 0;
        isBlinking = false;
        currentDisplayedHPNormalized = 1;
    }

    private void Start()
    {
        currentHPBarAnimationState = HPBarAnimationState.None;

        _time = Time.time;
        hpChange = 0;


        if(GameManager.Instance != null && GameManager.Instance.InGame) {
            ActivateHPBar();
        }

        StartCoroutine(AnimateHealthBar());
    }

    public void SetHPAuraActive(bool active)
    {
        screenAura.SetActive(active);
        if(screenAuraImg != null)
            screenAuraImg.color = lowHPColor;
    }


    #region Health Bar Animation
    private void FetchAndUpdateHP()
    {
        UpdateCurrentHP(PlayerController.Instance.GetPlayerStats());
    }
    private void UpdateCurrentHP(PlayerStats stats)
    {
        if(PlayerController.Instance != null)
            lowHpThreshold = PlayerController.Instance.LowHpThreshold;

        hpChange = stats.hp - currentHp;

        currentHp = stats.hp;

        currentHPNormalized = currentHp / stats.maxHp;

        isLowHP = currentHPNormalized <= lowHpThreshold;

        inGame = GameManager.Instance.InGame;
        
        CheckHPChange();

        if(isLowHP && inGame)
        {
            SetHPAuraActive(true);
            if(currentHp != 0)
                SoundManager.Instance.PlayLoopSound(SoundManager.Instance.low_hp_sound);
        }
        else
        {
            SetHPAuraActive(false);
            SoundManager.Instance.StopLoopSound(force: true);
        }
    }
    public void ActivateHPBar(bool activate = true)
    {
        hpBar.SetActive(activate);
    }

    private void CheckHPChange()
    {
        if (Time.time - _time < waitTimeBeforeAnimating)
        {
            hpChange = 0;
            return;
        }

        if(Mathf.Abs(hpChange) > hpDeltaDetect)
        {
            SetCorrectHpBarAnimationState();
            StartCoroutine(BlinkHealthBar());
            hpChange = 0;
        }
    }

    private void SetCorrectHpBarAnimationState()
    {
        if(hpChange > 0)
        {
            if(currentHPNormalized > currentDisplayedHPNormalized || hurtBarAmount < currentHPNormalized)
                currentHPBarAnimationState = HPBarAnimationState.Healing;
            else
                currentHPBarAnimationState = HPBarAnimationState.Hurting;
        }
        else
        {
            if(currentHPNormalized > currentDisplayedHPNormalized)
                currentHPBarAnimationState = HPBarAnimationState.Healing;
            else
                currentHPBarAnimationState = HPBarAnimationState.Hurting;
        }
    }

    private IEnumerator AnimateHealthBar()
    {
        hurtBarAmount = 1;

        while (true)
        {
            if (Time.time - _time < waitTimeBeforeAnimating)
            {
                hpFillBg.fillAmount = currentHPNormalized;
                hpFill.fillAmount = currentHPNormalized;

                yield return null;
                continue;
            }

            switch (currentHPBarAnimationState)
            {
                case HPBarAnimationState.Hurting:


                    if (timeSinceLastUpdate > animationHpDelay) {
                        hpFillBg.fillAmount = Mathf.Lerp(hpFillBg.fillAmount, currentHPNormalized, Time.deltaTime * animationHpSpeed);
                        hurtBarAmount = hpFillBg.fillAmount;
                    }
                    else
                    {
                        hpFillBg.fillAmount = hurtBarAmount;
                        timeSinceLastUpdate += Time.deltaTime;
                    }

                    hpFill.fillAmount = currentHPNormalized;
                    hpFillBg.color = hurtBg;

                    
                    break;

                case HPBarAnimationState.Healing:

                    hpFillBg.fillAmount = currentHPNormalized;
                    hpFillBg.color = healBg;

                    if (timeSinceLastUpdate > animationHpDelay)
                    {
                        hpFill.fillAmount = Mathf.Lerp(hpFill.fillAmount, currentHPNormalized, Time.deltaTime * animationHpSpeed);
                    }
                    else
                    {
                        timeSinceLastUpdate += Time.deltaTime;
                    }

                    hurtBarAmount = hpFill.fillAmount;

                    break;
            }

            currentDisplayedHPNormalized = hpFill.fillAmount;

            yield return null;
        }
    }

    private IEnumerator BlinkHealthBar()
    {
        timeSinceLastUpdate = 0f;

        if (isBlinking)
            yield break;

        isBlinking = true;

        bool flip = false;

        Color color = currentHPBarAnimationState == HPBarAnimationState.Hurting ? blinkHurtColor : blinkHealColor;

        while(timeSinceLastUpdate < animationHpDelay)
        {
            flip = !flip;


            hpBarOverlay.color = flip ? color : defaultBlinkColor;

            yield return new WaitForSeconds(blinkLength);
        }

        hpBarOverlay.color = defaultBlinkColor;

        isBlinking = false;
    }

    #endregion

    #region Start

    private void PrepareStart()
    {
        GameManager.Instance.SetCanButtonInteract(true);

        if (GameManager.Instance.InGame)
            ActivateHPBar();

        hpBarAnimator.SetBool("hasWon", false);
        hpBarAnimator.SetBool("hasLost", false);
        FetchAndUpdateHP();
    }

    #endregion

    #region Win

    private void PrepareWin()
    {
        hpBarAnimator.SetBool("hasWon", true);
    }

    #endregion

    #region GameOver

    private void PrepareGameOver()
    {
        GameManager.Instance.SetCanPause(false);
        hpBarAnimator.SetBool("hasLost", true);
        StartCoroutine(KnockAnimation());
    }

    private IEnumerator KnockAnimation()
    {
        SoundManager.Instance.StopMusic();
        SoundManager.Instance.StopLoopSound();

        yield return new WaitForSeconds(waitTimeBeforeGameOverScreen);

        gameOverScreen.SetActive(true);

        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOverSound);
        SoundManager.Instance.PlayMusic(SoundManager.Instance.gameOver, 4f);


        yield return null;
    }

    private void AnimateGameOverMenu()
    {
        GameManager.Instance.SetCanButtonInteract(false);
        gameOverMenuAnimator.SetTrigger("closeWindow");
    }

    public void Retry()
    {
        AnimateGameOverMenu();
        StartCoroutine(WaitAndReload(waitTimeBeforeLoadingScene));
    }

    public void GiveUp(){
        AnimateGameOverMenu();
        StartCoroutine(WaitAndLoadMainMenu(waitTimeBeforeLoadingScene));
    }

    private IEnumerator WaitAndLoadMainMenu(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        GameManager.Instance.ToMainMenu();
        ResetGameOverScreen();

    }
    private IEnumerator WaitAndReload(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        GameManager.Instance.ReloadLevel();
        ResetGameOverScreen();
    }

    private void ResetGameOverScreen(bool activate = false)
    {
        gameOverMenuAnimator.SetTrigger("reset");
        gameOverScreen.SetActive(activate);
    }
    #endregion

    private void StartRainbow(Rainbow rbw, bool activate = true)
    {
        if(activate)
            rbw.Activate();
        else
            rbw.Deactivate();
    }

    private void ActivateInvincibilityVisuals()
    {
        StartRainbow(hpBarOverlayRainbow);
        StartRainbow(hpBarFillRainbow);
    }
    private void DeactivateInvincibilityVisuals()
    {
        StartRainbow(hpBarOverlayRainbow, false);
        StartRainbow(hpBarFillRainbow, false);
    }
}
