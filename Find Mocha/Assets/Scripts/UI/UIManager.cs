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

    [Header("HP Bar Animations")]

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
    [SerializeField]
    private float blinkLength;
    [SerializeField]
    private Color blinkHurtColor;
    [SerializeField]
    private Color blinkHealColor;
    [SerializeField]
    private Color defaultBlinkColor;
    [SerializeField]
    private float healthBarBounceSpeed;
    [SerializeField]
    private float healthBarBounceDepth;
    [SerializeField]
    private Image hpBarOverlay;
    [SerializeField]
    private Image hpFill;
    [SerializeField]
    private Image hpFillBg;



    [SerializeField]
    private GameObject hpBar;
    [SerializeField]
    private RectTransform hpBarRectTransform;
    [SerializeField]
    private Animator hpBarAnimator;
    [SerializeField]
    private Rainbow hpBarOverlayRainbow;
    [SerializeField]
    private Animator gameOverMenuAnimator;


    private float waitTimeBeforeAnimating = .5f;

    private Vector2 hpBarPos;

    private float maxHp;
    private float currentHp;
    private float hpChange;
    private float animationTimer;

    private float _time;

    private AnimationState currentAnimationState;

    private enum AnimationState
    {
        None,
        Healing,
        Hurting
    }


    private void OnEnable()
    {
        PlayerController.OnPlayerInvincibility += ActivateInvincibilityVisuals;
        PlayerController.OnPlayerInvincibilityEnd += DeactivateInvincibilityVisuals;
        PlayerController.OnPlayerKnockedOut += PrepareGameOver;
        FlagManager.OnPlayerWin += PrepareWin;
        PlayerController.OnPlayerReady += PrepareStart;
        PlayerController.OnPlayerChangeHP += UpdateCurrentHP;
    }
    private void OnDisable()
    {
        PlayerController.OnPlayerInvincibility -= ActivateInvincibilityVisuals;
        PlayerController.OnPlayerInvincibilityEnd -= DeactivateInvincibilityVisuals;
        PlayerController.OnPlayerKnockedOut -= PrepareGameOver;
        FlagManager.OnPlayerWin -= PrepareWin;
        PlayerController.OnPlayerReady -= PrepareStart;
        PlayerController.OnPlayerChangeHP -= UpdateCurrentHP;
    }


    private void Start()
    {
        hpBarPos = hpBarRectTransform.anchoredPosition;

        currentAnimationState = AnimationState.None;

        _time = Time.time;
        hpChange = 0;

        IEnumerator animation_CR = AnimateHealthBar();
        StartCoroutine(animation_CR);
    }

    private void Update()
    {
        CheckHPChange();
    }

    #region HealthBar Animation

    public void ActivateHPBar(bool activate = true)
    {
        hpBar.SetActive(activate);
    }

    private void FetchAndUpdateHP()
    {
        UpdateCurrentHP(PlayerController.Instance.GetPlayerStats());
    }

    private void UpdateCurrentHP(PlayerStats stats)
    {
        maxHp = stats.maxHp;

        hpChange = stats.hp - currentHp;

        currentHp = stats.hp;
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
            currentAnimationState = AnimationState.Hurting;
            animationTimer = 0f;
            StartCoroutine(BlinkHealthBar());
            hpChange = 0;
        }
        else if(hpChange > hpDeltaDetect)
        {
            currentAnimationState = AnimationState.Healing;
            animationTimer = 0f;
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

            switch (currentAnimationState)
            {
                case AnimationState.Hurting:

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

                case AnimationState.Healing:

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
        bool flip = false;

        Color color = currentAnimationState == AnimationState.Hurting ? blinkHurtColor : blinkHealColor;

        while(animationTimer < animationHpDelay)
        {
            flip = !flip;


            hpBarOverlay.color = flip ? color : defaultBlinkColor;

            yield return new WaitForSeconds(blinkLength);
        }

        hpBarOverlay.color = defaultBlinkColor;
    }

    #endregion

    #region Start

    private void PrepareStart()
    {
        GameManager.Instance.SetCanButtonInteract(true);

        if(GameManager.Instance.CurrentSceneIndex != 0)
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
        hpBarAnimator.SetBool("hasLost", true);
        StartCoroutine(KnockAnimation());
    }

    private IEnumerator KnockAnimation()
    {
        yield return new WaitForSeconds(waitTimeBeforeGameOverScreen);

        gameOverScreen.SetActive(true);

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
