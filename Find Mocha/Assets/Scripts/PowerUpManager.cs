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
	// in units per period
	private const float PHASE_SPACIAL_PERIOD = 10;

	[SerializeField]
	private ItemType itemType;

	#region Collection

	[Header("Collection")]
	[SerializeField]
	private int value = 1;

    #endregion

    #region Bonuses granted
    [Header("Power-up")]
	[SerializeField]
	private float healthBonus;

	[Header("Temporary bonuses")]
	[SerializeField]
	private PowerUp[] powerUps;
	#endregion Bonuses granted

	[Header("Respawn")]
	[SerializeField]
	private bool respawns;
	[SerializeField]
	private float respawnTime;


	[Header("Animation properties")]
	[SerializeField]
	private float animationStartPhase;
    [SerializeField]
    private StartingPhase startingPhaseType;
    [SerializeField]
	private float animationSpeed;
	[SerializeField]
	private float animationDepth;
	[SerializeField]
	private float timeBeforeDestroy = 1f;
    [SerializeField]
    private ParticleSystem particles;
    [SerializeField]
    private SpriteRenderer overrideSpriteRenderer;

    private Vector2 originalPos;
	private SpriteRenderer spriteRenderer;
	private bool isConsumed;
	private Rainbow rainbow;

	private bool hasRainbow;

	private enum ItemType
	{
		powerUp,
		collection // Deprecated
	}

	private enum StartingPhase
	{
		Custom,
		RandomizePhase,
		InWave
	}

	private void Start()
	{
		if(itemType == ItemType.collection)
			CollectibleManager.Instance.RegisterCollectibleForLevel();

		switch (startingPhaseType)
		{
			case StartingPhase.RandomizePhase:
                animationStartPhase = Random.Range(0f, Mathf.PI * 2);
                break;

			case StartingPhase.InWave:
				animationStartPhase = transform.position.x / PHASE_SPACIAL_PERIOD * Mathf.PI * 2;
				break;
		}
			

		originalPos = transform.position;
		isConsumed = false;
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null)
			spriteRenderer = overrideSpriteRenderer;
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

                SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
                CollectibleManager.Instance.AddCoin(value);

                break;
		}
		particles?.Play();

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
