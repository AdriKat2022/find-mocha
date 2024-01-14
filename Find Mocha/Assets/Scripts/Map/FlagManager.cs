using System.Collections;
using System;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public float animationLength = 3f;
    public LayerMask playerMask;

    [SerializeField] private new ParticleSystem particleSystem;

    private bool hasWon;


    public static event Action OnPlayerWin;



    private void Start()
    {

        hasWon = false;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (hasWon)
            return;

        int colliderLayer = collider.gameObject.layer;

        if ((playerMask & (1 << colliderLayer)) != 0)
            StartCoroutine(LevelCompleted(collider.gameObject.GetComponent<PlayerController>()));
    }

    IEnumerator LevelCompleted(PlayerController playerController)
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
