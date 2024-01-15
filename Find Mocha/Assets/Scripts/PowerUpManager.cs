using System.Collections;
using UnityEngine;

public enum PowerUpType
{
    speedBoost,
    jumpBoost,
    invincibilty
}

public class PowerUpManager : MonoBehaviour, ICollectible
{

    [SerializeField]
    private ItemType itemType;

    #region Bonuses granted
    [Header("One time bonuses")]
    [SerializeField]
    private float healthBonus;

    [Header("Temporary bonuses")]
    [SerializeField]
    private PowerUp[] powerUps;
    #endregion Bonuses granted

    [Header("Option")]
    [SerializeField]
    private bool respawns;
    [SerializeField]
    private float respawnTime;


    [Header("Animation properties")]
    [SerializeField]
    private float animationStartPhase;
    [SerializeField]
    private bool randomizeStartPhase;
    [SerializeField]
    private float animationSpeed;
    [SerializeField]
    private float animationDepth;
    [SerializeField]
    private float timeBeforeDestroy = 1f;
    [SerializeField]
    private ParticleSystem particles;

    private Vector2 originalPos;
    private SpriteRenderer spriteRenderer;
    private bool isConsumed;
    private Rainbow rainbow;

    private bool hasRainbow;

    private enum ItemType
    {
        powerUp,
        collection
    }

    private void Start()
    {
        if(randomizeStartPhase)
            animationStartPhase = Random.Range(0f,Mathf.PI*2);

        originalPos = transform.position;
        isConsumed = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        hasRainbow = TryGetComponent(out rainbow);
        StartCoroutine(AnimateCollectible());
    }

    private void Refresh()
    {
        isConsumed = false;
        spriteRenderer.color = Color.white;

        if(hasRainbow)
            rainbow.Activate();
    }

    private void Consume()
    {
        if(hasRainbow)
            rainbow.Deactivate();

        spriteRenderer.color = Color.clear;

        if (respawns)
            StartCoroutine(RefreshAfterTime(respawnTime));
        else
            Destroy(gameObject, timeBeforeDestroy);
    }

    public void Collect(PlayerController script)
    {
        switch(itemType)
        {
            case ItemType.powerUp:


                if (healthBonus != 0)
                    script.Heal(healthBonus);

                PlayerController.Instance.ApplyBonuses(powerUps);

                break;
                

            case ItemType.collection:
                // TODO
                break;
        }
        particles.Play();

        Consume();
    }

    private IEnumerator RefreshAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        Refresh();
    }

    private IEnumerator AnimateCollectible()
    {
        float timer = animationSpeed != 0 ? animationStartPhase/animationSpeed : 0;

        while (true)
        {
            transform.position = originalPos + Vector2.up * Mathf.Sin(timer * animationSpeed) * animationDepth;

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isConsumed)
            return;

        if(collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            isConsumed = true;
            if(hasRainbow)
                rainbow.Deactivate();
            spriteRenderer.color = new Color(0,0,0,0);
            Collect(playerController);
        }
    }
}
