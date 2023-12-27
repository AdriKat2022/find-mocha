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

    [Header("Animation properties")]
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
        originalPos = transform.position;
        isConsumed = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        hasRainbow = TryGetComponent(out rainbow);
        StartCoroutine(AnimateCollectible());
    }

    public void Collect(PlayerController script)
    {
        switch(itemType)
        {
            case ItemType.powerUp:


                if (healthBonus != 0)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.heal);
                    script.Heal(healthBonus);
                }

                PlayerController.Instance.ApplyBonuses(powerUps);

                break;
                

            case ItemType.collection:
                // TODO
                break;
        }
        particles.Play();
        Destroy(gameObject, timeBeforeDestroy);
    }

    private IEnumerator AnimateCollectible()
    {
        float timer = 0;

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
