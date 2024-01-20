using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public GameObject finalPos;
    [SerializeField]
    private Animator mocha;
    [SerializeField]
    private FlagType flagType;
    [SerializeField]
    private float animationLength = 3f;
    [SerializeField]
    private LayerMask playerMask;

    [SerializeField]
    private new ParticleSystem particleSystem;

    private bool hasWon;
    private PlayerController playerController;

    public static event Action OnPlayerWin;

    public static FlagManager Instance;

    private Transform milkSpriteTransform;
    private Animator mochaAnimator;
    private Vector3 basePosition;
    private CameraMovement camScript;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        playerController = PlayerController.Instance;

        milkSpriteTransform = playerController.transform;
        basePosition = milkSpriteTransform.position;

        camScript = Camera.main.transform.parent.GetComponent<CameraMovement>();
        hasWon = false;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (hasWon)
            return;

        int colliderLayer = collider.gameObject.layer;

        if ((playerMask & (1 << colliderLayer)) != 0)
            PlayerWin();
    }

    private void ResetTransform()
    {
        milkSpriteTransform.position = basePosition;
    }

    public void PlayerWin()
    {
        if (hasWon || !playerController.HasWon())
            return;

        GameManager.Instance.SetCanPause(false);

        hasWon = true;

        OnPlayerWin?.Invoke();

        particleSystem.Play();
        SoundManager.Instance.PlaySound(SoundManager.Instance.level_clear);

        switch (flagType)
        {
            case FlagType.Classic:
                StartCoroutine(LevelCompleted());
                break;

            case FlagType.EndGame:
                StartCoroutine(EndGame());
                break;
        }
    }

    private IEnumerator LevelCompleted()
    {
        yield return new WaitForSeconds(animationLength);

        GameManager.Instance.MoveToNextLevel();
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(animationLength);

        playerController.SetPlayerActive(false);
        ResetTransform();
        playerController.StepFinalAnimation();
        mochaAnimator = Instantiate(mocha);
        camScript.SetFinalPos(finalPos);



        yield return new WaitForSeconds(13);

        mochaAnimator.SetTrigger("Final");
        playerController.StepFinalAnimation();

        yield return new WaitForSeconds(.4f);

        GameManager.Instance.EnableWinScreen();
    }

    private enum FlagType
    {
        Classic,
        EndGame
    }
}
