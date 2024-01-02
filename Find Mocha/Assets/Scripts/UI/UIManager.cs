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
    private GameObject hpBar;
    [SerializeField]
    private Animator gameOverMenuAnimator;
    [SerializeField]
    private GameObject screenAura;
    [SerializeField]
    private Image screenAuraImg;


    private readonly float waitTimeBeforeAnimating = 1f;

    private float maxHp;
    private float currentHp;
    private bool isLowHP;
    private float hpChange;
    private float animationTimer;

    private float _time;
    private bool isBlinking;

    private bool inGame;

    private HPBarAnimationState currentHPBarAnimationState;

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
    }

    private void Start()
    {

        currentHPBarAnimationState = HPBarAnimationState.None;

        _time = Time.time;
        hpChange = 0;

        IEnumerator animation_CR = AnimateHealthBar();
        StartCoroutine(animation_CR);
    }

    private void Update()
    {
        CheckHPChange();
    }

    public void SetHPAuraActive(bool active)
    {
        screenAura.SetActive(active);
        if(screenAuraImg != null)
            screenAuraImg.color = lowHPColor;
    }


    #region Health Animation
    private void FetchAndUpdateHP()
    {
        UpdateCurrentHP(PlayerController.Instance.GetPlayerStats());
    }
    private void UpdateCurrentHP(PlayerStats stats)
    {
        lowHpThreshold = PlayerController.Instance.LowHpThreshold;

        maxHp = stats.maxHp;

        hpChange = stats.hp - currentHp;

        currentHp = stats.hp;

        isLowHP = currentHp / maxHp <= lowHpThreshold;

        inGame = GameManager.Instance.InGame;
        
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

        if (hpChange < -hpDeltaDetect)
        {
            currentHPBarAnimationState = HPBarAnimationState.Hurting;
            StartCoroutine(BlinkHealthBar());
            hpChange = 0;
        }
        else if(hpChange > hpDeltaDetect)
        {
            currentHPBarAnimationState = HPBarAnimationState.Healing;
            StartCoroutine(BlinkHealthBar());
            hpChange = 0;
        }
    }

    private IEnumerator AnimateHealthBar()
    {
        while (true)
        {
            if (Time.time - _time < waitTimeBeforeAnimating)
            {
                if(maxHp != 0)
                {
                    hpFillBg.fillAmount = currentHp / maxHp;
                    hpFill.fillAmount = currentHp / maxHp;
                }
                yield return null;
                continue;
            }

            //Debug.Log("low hp: " + isLowHP);
            //Debug.Log("ingame: " + inGame);

            switch (currentHPBarAnimationState)
            {
                case HPBarAnimationState.Hurting:

                    hpFill.fillAmount = currentHp/maxHp;
                    hpFillBg.color = hurtBg;

                    if (animationTimer > animationHpDelay) {
                        hpFillBg.fillAmount = Mathf.Lerp(hpFillBg.fillAmount, currentHp/maxHp, Time.deltaTime * animationHpSpeed);
                    }
                    else
                    {
                        animationTimer += Time.deltaTime;
                    }

                    break;

                case HPBarAnimationState.Healing:

                    hpFillBg.fillAmount = currentHp / maxHp;
                    hpFillBg.color = healBg;

                    if (animationTimer > animationHpDelay)
                    {
                        hpFill.fillAmount = Mathf.Lerp(hpFill.fillAmount, currentHp / maxHp, Time.deltaTime * animationHpSpeed);
                    }
                    else
                    {
                        animationTimer += Time.deltaTime;
                    }

                    break;
            }

            /*timer += Time.deltaTime;

            //Debug.Log(hpBarPos + healthBarBounceDepth * Mathf.Sin(timer * healthBarBounceSpeed) * Vector2.up);

            hpBarRectTransform.anchoredPosition = hpBarPos + healthBarBounceDepth * Mathf.Sin(timer * healthBarBounceSpeed) * Vector2.up + hpBarRectTransform.anchoredPosition.x * Vector2.right;*/

            yield return null;
        }
    }

    private IEnumerator BlinkHealthBar()
    {
        animationTimer = 0f;

        if (isBlinking)
            yield break;

        isBlinking = true;

        bool flip = false;

        Color color = currentHPBarAnimationState == HPBarAnimationState.Hurting ? blinkHurtColor : blinkHealColor;

        while(animationTimer < animationHpDelay)
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
    }
    private void DeactivateInvincibilityVisuals()
    {
        StartRainbow(hpBarOverlayRainbow, false);
    }
}
