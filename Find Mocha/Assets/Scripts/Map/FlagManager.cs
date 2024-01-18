using System.Collections;
using System;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public float animationLength = 3f;
    public LayerMask playerMask;

    [SerializeField] private new ParticleSystem particleSystem;

    private bool hasWon;
    private PlayerController playerController;

    public static event Action OnPlayerWin;

    public static FlagManager Instance;


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
        hasWon = false;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (hasWon)
            return;

        int colliderLayer = collider.gameObject.layer;

        if ((playerMask & (1 << colliderLayer)) != 0)
            StartCoroutine(LevelCompleted());
    }

    public void PlayerWin()
    {
        StartCoroutine(LevelCompleted());
    }

    IEnumerator LevelCompleted()
    {
        hasWon = true;

        if (!playerController.HasWon())
        {
            hasWon = false;
            yield break;
        }

        OnPlayerWin?.Invoke();

        particleSystem.Play();
        SoundManager.Instance.PlaySound(SoundManager.Instance.level_clear);

        yield return new WaitForSeconds(animationLength);

        GameManager.Instance.MoveToNextLevel();
    }
}
