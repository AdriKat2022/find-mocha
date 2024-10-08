using System;
using System.Collections;
using UnityEngine;

public struct PlayerStats
{
	public float maxHp;
	public float hp;
	public bool heartGunUnlocked;

	public PlayerStats(float maxHp, float hp, bool heartGunUnlocked)
	{
		this.maxHp = maxHp;
		this.hp = hp;
		this.heartGunUnlocked = false;
	}
}

[Serializable]
public struct PlayerInput
{
	public bool jump;
	public bool jump_let;
	public bool right;
	public bool left;
	public bool shoot;
	public bool interact;
}

public class PlayerController : MonoBehaviour, IDamageble
{
	#region Variables

	[Header("Team")]
	[SerializeField]
	private Team team;


	private readonly float Fall_height = -15;


	[Header("Milk properties")]
	public float maxHp;
	public float topSpeed;
	public float acceleration;
	public float brakeForce;
	public float brakeForceInHitStun;
	public float wonJumpForce;
	public LayerMask jumpableGround;
	public float maxYSpeed;

	[Header("Jump")]
	public float defaultJumpForce;
	public float bufferJumpTime;
	public float coyoteTime;
	public float fastFallMultiplier;
	public Vector2 jumpCheckPadding;
	public float jumpCheckDown = .4f;

	[Header("INV and hurt animation")]
	public float hurtingFrames;
	public float invicibilityFrames;
	public float slowBlinking;
	public float fastBlinking;
	[Range(0f,1f)]
	public float slowFastThreshold;
	[SerializeField]
	private float lowHpThreshold;

	[Header("Stun")]
	[SerializeField]
	private float minStunAnimationStart;
	[SerializeField]
	private float stunAnimationMaxSpeed;
	[SerializeField]
	private float stunAnimationMaxStrength;
	[SerializeField]
	private bool isStunAnimationDeprogressive;


	public float LowHpThreshold => lowHpThreshold;

	public IInteractable Interactable;

	[Header("Power up visuals")]
	[SerializeField]
	private ParticleSystem jumpBoostVisual;
	[SerializeField]
	private ParticleSystem speedBoostVisual;

	private ParticleSystem.EmissionModule jumpBoostVisualModule;
	private ParticleSystem.EmissionModule speedBoostVisualModule;

	[Header("Internal components")]
	[SerializeField]
	private GameObject sprite;

	private new Collider2D collider;
	[HideInInspector]
	public Rigidbody2D rb;
	private Animator animator;
	private SpriteRenderer spriteRenderer;
	private Rainbow rainbow;

	[SerializeField]
	private ParticleSystem heartParticles;
	private ParticleSystem.EmissionModule heartParticlesEmissionModule;

	[SerializeField]
	private HeartGun heartgun;

	[Header("External components")]
	private DialogueUI dialogueUI;
	private CameraShake cameraShake;

	[Header("Debug")]
	[SerializeField]
	private bool debug;

	private float currentHp;
	public float CurrentHp => currentHp;
	private float jumpForce;
	[SerializeField]
	private float lastTimeGrounded;
	[SerializeField]
	private float lastJumpBuffered;
	[SerializeField]
	private bool jumpBufferUsed;
	[SerializeField]
	private bool coyoteUsable;
	private bool jumpEndedEarly;
	[SerializeField]
	private float _time;

	private bool isHurting;
	private bool isInvulnerable;
	private bool isInvincible;
	private bool isJumping;
	private bool hasWon;
	private bool isKnockedOut;
	private bool hasFallen;

	private bool isActivated;

	private bool hasUnlockedHeartGun;

	private PlayerInput playerInput;

	#region Bonuses variables

	private float speedBonus;
	private float jumpBonus;
	private float invincibleTimer;

	#endregion Bonuses variables


	#endregion

	public static PlayerController Instance;

	#region Events

	public static event Action OnPlayerInvincibility;
	public static event Action OnPlayerInvincibilityEnd;
	public static event Action OnPlayerKnockedOut;
	public static event Action OnPlayerReady;

	public static event Action<PlayerStats> OnPlayerChangeHP;

	#endregion


#if UNITY_EDITOR

	#region Debug stuff

	private void OnValidate()
	{
		collider = GetComponent<CapsuleCollider2D>();

		OnPlayerChangeHP?.Invoke(GetPlayerStats());

		//Debug.Log(cameraShake);
		//Debug.Log(hpBarRainbow);
		//Debug.Log(gameOverScreen);
		//Debug.Log(DialogueUI);

		//if (hpBarRainbow == null || gameOverScreen == null || DialogueUI == null || cameraShake == null)
		//    Debug.LogError("Missing components in playerController script");
	}

	private void OnDrawGizmosSelected()
	{
		if (!debug)
			return;

		Vector3 size = collider.bounds.size;
		size.x -= jumpCheckPadding.x;
		size.y -= jumpCheckPadding.y;

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(collider.bounds.center, size);
		Gizmos.DrawWireCube(collider.bounds.center+Vector3.down*jumpCheckDown, size);
	}

	#endregion


#endif

	#region Debug Events

	private bool warnedAboutDebugMode = false;

	private void DebugFunc()
	{
		if (!warnedAboutDebugMode)
		{
			Debug.LogWarning("WARNING: DEBUG MODE IS ENABLED");
			warnedAboutDebugMode = true;
		}

		if (Input.GetKeyDown(KeyCode.R))
			GameManager.Instance.ReloadLevel();
	
		if (Input.GetKeyDown(KeyCode.E))
			Damage(2);

		if (Input.GetKeyDown(KeyCode.A))
			Heal(2);

		if (Input.GetKeyDown(KeyCode.F))
			Heal(200);

		if (Input.GetKeyDown(KeyCode.S))
			Damage(200);

		if (Input.GetKeyDown(KeyCode.Q))
			Damage(currentHp - 1);

		if (Input.GetKeyDown(KeyCode.W))
			FlagManager.Instance.PlayerWin();

		if (Input.GetKeyDown(KeyCode.Alpha4))
			speedBonus += 1;


		if (Input.GetKeyDown(KeyCode.Alpha5))
			speedBonus = 0;

		if (Input.GetKeyDown(KeyCode.I))
		{
			isInvincible = !isInvincible;
			if (isInvincible)
				rainbow.Activate();
			else
				rainbow.Deactivate();
		}

		if (Input.GetKeyDown(KeyCode.U))
		{
			Debug.LogWarning("WARNING: HEARTGUN UNLOCKED");
			UnlockHeartGun();
		}
	}

	#endregion

	private bool IsGameOver => hasWon || isKnockedOut;

	public void SetPlayerActive(bool activate)
	{
		isActivated = activate;
	}
	public void StepFinalAnimation()
	{
		animator.SetTrigger("endAnimation");
	}

	private void Awake()
	{
		// Overwrite method
		if(Instance != null)
			Destroy(Instance.gameObject);

		Instance = this;


		TryGetComponent(out rb);
		TryGetComponent(out collider);
		TryGetComponent(out rainbow);
		sprite.TryGetComponent(out animator);
		sprite.TryGetComponent(out spriteRenderer);

		heartParticles.Play();
		heartParticlesEmissionModule = heartParticles.emission;
		heartParticlesEmissionModule.enabled = false;

		jumpBoostVisual.Play();
		jumpBoostVisualModule = jumpBoostVisual.emission;
		jumpBoostVisualModule.enabled = false;

		speedBoostVisual.Play();
		speedBoostVisualModule = speedBoostVisual.emission;
		speedBoostVisualModule.enabled = false;

		hasWon = false;
		isKnockedOut = false;
		isHurting = false;
		isInvulnerable = false;
		isJumping = false;

		invincibleTimer = 0;
		jumpForce = defaultJumpForce;


		_time = 0;
		lastTimeGrounded = -10f;
		lastJumpBuffered = -10f;
		jumpEndedEarly = false;
	}

	private void Start()
	{
		dialogueUI = GameManager.Instance.DialogueUI;
		Camera.main.transform.parent.TryGetComponent(out cameraShake);

		if (dialogueUI == null)
			dialogueUI = GameObject.Find("GameUI").GetComponent<DialogueUI>();

		if (GameManager.Instance.CheckPlayerStats(this, out PlayerStats stats))
		{
			LoadPlayerStats(stats);
		}
		else
		{
			currentHp = maxHp;
		}

		playerInput = new PlayerInput();

		OnPlayerReady?.Invoke();

		isActivated = true;
	}

	private void Update()
	{
		if(!isActivated) return;

		//DebugFunc();

		if (isKnockedOut)
			return;

		CheckAlive();

		if (isKnockedOut)
			return;

		GroundCheck();

		if (isHurting || hasWon || dialogueUI.IsOpen)
		{
			if (dialogueUI.IsOpen || hasWon)
				ResetInput();

			AnimatorUpdate();
			return;
		}

		GetPlayerInput();

		ManageJump();

		CheckInteraction();

		AnimatorUpdate();

		_time += Time.deltaTime;
	}

	private void FixedUpdate()
	{
		if (!hasWon)
		{
			MovePlayer();
		}
	}

	private void CheckInteraction()
	{
		if (playerInput.interact)
		{
			if(Interactable != null)
			{
				SoundManager.Instance.PlaySound(SoundManager.Instance.button_press);
				Interactable.Interact(this);
			}
		}
	}

	private void GetPlayerInput()
	{
		playerInput.jump_let = Input.GetKeyUp(KeyCode.Space);
		playerInput.jump = Input.GetKeyDown(KeyCode.Space);

		playerInput.right = Input.GetKey(KeyCode.RightArrow);
		playerInput.left = Input.GetKey(KeyCode.LeftArrow);

		playerInput.shoot = Input.GetKeyDown(KeyCode.Z);
		playerInput.interact = Input.GetKeyDown(KeyCode.E);
	}

	private void ResetInput()
	{
		playerInput.jump_let = false;
		playerInput.jump = false;
		playerInput.right = false;
		playerInput.left = false;
		playerInput.shoot = false;
		playerInput.interact = false;
	}

	public bool CanJump => !isJumping || ((coyoteTime + lastTimeGrounded > _time) && coyoteUsable);
	private bool IsJumpBuffered => playerInput.jump || (lastJumpBuffered + bufferJumpTime > _time && !jumpBufferUsed);

	private void ManageJump()
	{
		if (playerInput.jump)
		{
			lastJumpBuffered = _time;
			jumpBufferUsed = false;
		}

		if (!isJumping)
		{
			lastTimeGrounded = _time;
			if(coyoteTime + lastJumpBuffered < _time)
				coyoteUsable = true;

			jumpEndedEarly = false;
		}


		if (IsJumpBuffered && CanJump)
		{
			jumpBufferUsed = true;
			Jump(makeSound: true);
			coyoteUsable = false;
		}

		if(!jumpEndedEarly && isJumping && rb.velocity.y > 0 && playerInput.jump_let)
		{
			rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * fastFallMultiplier);
			jumpEndedEarly = true;
		}

	}

	private void MovePlayer()
	{
		if (isHurting || isKnockedOut)
		{
			float xVel = Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * brakeForceInHitStun);

			rb.velocity = new Vector2(xVel, rb.velocity.y);

			return;
		}

		Vector2 currentVelocity = rb.velocity;


		if (playerInput.right)
		{
			//force.x = Mathf.Max((topSpeed + speedBonus - currentVelocity.x) * rb.mass * acceleration, 0);
			currentVelocity.x = Mathf.Max(currentVelocity.x, topSpeed + speedBonus);

			spriteRenderer.flipX = false;
		}
		else if (playerInput.left)
		{
			//force.x = Mathf.Min((-topSpeed - speedBonus - currentVelocity.x) * rb.mass * acceleration, 0);
			currentVelocity.x = Mathf.Min(rb.velocity.x, -topSpeed - speedBonus);

			spriteRenderer.flipX = true;
		}
		else if (Mathf.Abs(rb.velocity.x) > .1f)
		{
			//force.x = -currentVelocity.x * rb.mass * brakeForce;

			if (rb.velocity.x < -.01f)
				spriteRenderer.flipX = true;
			else if (rb.velocity.x > .01f)
				spriteRenderer.flipX = false;

			currentVelocity.x = 0;
		}

		currentVelocity.y = Mathf.Max(-maxYSpeed, currentVelocity.y);

		rb.velocity = currentVelocity;

		//rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(-maxYSpeed, currentVelocity.y));


		//rb.AddForce(force);
	}

	private void Jump(bool makeSound = true)
	{
		rb.velocity = new Vector2(rb.velocity.x, jumpForce + jumpBonus);
		isJumping = true;

		animator.SetTrigger("bufJump");

		if(makeSound)
			SoundManager.Instance.PlaySound(SoundManager.Instance.jump);
    }

	private void GroundCheck()
	{
		Vector3 size = collider.bounds.size;
		size.x -= jumpCheckPadding.x;
		size.y -= jumpCheckPadding.y;

		//bool wasJumping = isJumping;

		isJumping = !Physics2D.BoxCast(collider.bounds.center, size, 0f, Vector2.down, jumpCheckDown, jumpableGround);

		/*if (wasJumping && !isJumping)
			SoundManager.Instance.PlaySound(SoundManager.Instance.onground);*/
	}

	private bool CheckAlive()
	{
		hasFallen = CheckFallen();

		if(currentHp <= 0f)
		{
			if (!isKnockedOut) // Triggers once
			{
				OnPlayerKnockedOut?.Invoke();
				animator.SetTrigger("isKnockedOut");
				SoundManager.Instance.PlaySound(SoundManager.Instance.milk_squeak, 1f/2f);
			}

			isKnockedOut = true;

			return false;
		}

		return true;
	}

	private bool CheckFallen()
	{
		if (transform.position.y < Fall_height)
		{
			rb.velocity = Vector2.zero;
			SoundManager.Instance.PlaySound(SoundManager.Instance.fell_sound);
			RawDamage(maxHp, hasShake: true, updateVisuals: true);
			return true;
		}
		
		return false;
	}

	public void UnlockHeartGun()
	{
		hasUnlockedHeartGun = true;
		heartgun.UnlockHeartGun();
	}

	public bool IsFacingRight => !spriteRenderer.flipX;

	private void AnimatorUpdate()
	{
		animator.SetBool("isJumping", isJumping);
		animator.SetBool("isWalking", Mathf.Abs(rb.velocity.x) > 0.1f);
		animator.SetFloat("yVelocity", rb.velocity.y);
		animator.SetFloat("milkSanity", currentHp/maxHp <= lowHpThreshold ? 0 : 1);
	}

	public bool HasWon()
	{
		if (!isKnockedOut)
		{
			hasWon = true;
			animator.SetTrigger("hasWon");
			StartCoroutine(VictoryDance());
		}

		return !isKnockedOut;
	}

	#region Animations
	private IEnumerator VictoryDance()
	{
		jumpForce = wonJumpForce;
		rb.velocity = Vector3.zero;

		while (true)
		{
			if(!isJumping)
				Jump(makeSound: false);

			yield return null;
		}
	}


	private IEnumerator StunAnimation(float duration)
	{
		float timer = 0;
		float total_time = 0;
		float timeBetweenElementaryShake = 1 / stunAnimationMaxSpeed;

		bool _switch = true;

		Vector2 basePosition = spriteRenderer.transform.parent.localPosition;

		float strength = stunAnimationMaxStrength;

		while (total_time < duration)
		{
			if(isStunAnimationDeprogressive)
				strength = Mathf.Lerp(stunAnimationMaxStrength, 0, total_time/duration);

			Debug.Log(_switch);

			if (_switch)
			{
				if(timer > timeBetweenElementaryShake)
					_switch = false;

				spriteRenderer.transform.parent.localPosition = basePosition + Vector2.right * strength;

				timer += Time.deltaTime;
			}
			else
			{
				if (timer < 0)
					_switch = true;

				spriteRenderer.transform.parent.localPosition = basePosition + Vector2.left * strength;

				timer -= Time.deltaTime;
			}

			total_time += Time.deltaTime;

			yield return null;
		}
		spriteRenderer.transform.localPosition = basePosition;
	}

	#endregion

	private IEnumerator HurtPhase(bool overrideHitstun = false, float hitstun = 0)
	{
		if (isHurting || isInvulnerable)
			yield break;

		isHurting = true;
		animator.SetBool("isHurting", true);

		if(hitstun > minStunAnimationStart)
			StartCoroutine(StunAnimation(hitstun));

		yield return new WaitForSeconds(overrideHitstun ? hitstun : hurtingFrames);


		animator.SetBool("isHurting", false);
		isInvulnerable = true;
		isHurting = false;

		float timer = 0f;
		float blinkTimer = 0f;

		bool fastBlink = false;

		while (timer < invicibilityFrames)
		{
			if(!fastBlink && blinkTimer > slowBlinking)
			{
				spriteRenderer.enabled = !spriteRenderer.enabled;
				blinkTimer = 0f;
			}
			else if(fastBlink && blinkTimer > fastBlinking)
			{
				spriteRenderer.enabled = !spriteRenderer.enabled;
				blinkTimer = 0f;
			}

			if (!fastBlink && timer/invicibilityFrames > slowFastThreshold)
			{
				fastBlink = true;
			}

			timer += Time.deltaTime;
			blinkTimer += Time.deltaTime;

			yield return null;
		}

		isInvulnerable = false;
		spriteRenderer.enabled = true;
	}

	private void RawDamage(float damage, bool hasShake = false, bool updateVisuals = false) // Bypasses everything, triggers nothing and doesn't check for KO
	{
		currentHp -= damage;

		if(updateVisuals)
			OnPlayerChangeHP?.Invoke(GetPlayerStats());

		if (hasShake)
			cameraShake.AddStress((damage+5)/maxHp);
	}

	public void InstaKill(IDamageble from, Vector2? knockback, float knockbackAngle = 0, float knockbackForce = 0)
	{
		isKnockedOut = true;
	}

	public void Damage(IDamageble from, DamageData damageData)
	{
		if (isHurting || isInvulnerable || isKnockedOut)
			return;

		cameraShake.AddStress((damageData.damage+5)/maxHp);

		if (isInvincible)
		{
			SoundManager.Instance.PlaySound(SoundManager.Instance.invincibled);
			from?.InstaKill(this, -damageData.knockback);
			GameManager.Instance.CreateHitStop(0.1f);
			return;
		}

		SoundManager.Instance.PlaySound(SoundManager.Instance.damage);

		currentHp -= damageData.damage;
		OnPlayerChangeHP?.Invoke(GetPlayerStats());

		if (CheckAlive())
		{
			if (damageData.knockback != null)
				Knockback((Vector2)damageData.knockback);

			StartCoroutine(HurtPhase(damageData.overrideHitstun, damageData.hitstun));
		}
	}

	public void Damage(float dmg)
	{
		Damage(null, new DamageData(dmg));
	}

	public void Heal(float heal, bool isSilent = false)
	{
		if (isKnockedOut)
			return;

		if(!isSilent)
			SoundManager.Instance.PlaySound(SoundManager.Instance.heal);

		bool notOverhealing = currentHp < maxHp;

		currentHp = Mathf.Clamp(currentHp + heal, 0f, maxHp);

		if(notOverhealing)
			OnPlayerChangeHP?.Invoke(GetPlayerStats());
	}

	public Team GetTeam() => team;

	public void Knockback(float angle, float force)
	{
		Vector2 direction = new Vector2(Mathf.Cos(Mathf.PI * angle / 180), Mathf.Sin(Mathf.PI * angle / 180));

		rb.velocity = direction * force;
	}

	public void Knockback(Vector2 knockback)
	{
		rb.velocity = knockback;
	}

	public PlayerStats GetPlayerStats()
	{
		return new PlayerStats(maxHp, currentHp, hasUnlockedHeartGun);
	}

	public void LoadPlayerStats(PlayerStats stats)
	{
		maxHp = stats.maxHp;
		currentHp = stats.hp;
		hasUnlockedHeartGun = stats.heartGunUnlocked;

		if(hasUnlockedHeartGun)
			heartgun.UnlockHeartGun();
	}

	#region PowerUps

	private int jumpPowerUpsActive = 0;
	private int speedPowerUpsActive = 0;

	public void ApplyBonuses(PowerUp[] powerUps)
	{
		if (isKnockedOut)
			return;

        SoundManager.Instance.PlaySound(SoundManager.Instance.power_up);

        foreach (PowerUp powerUp in powerUps)
		{
			StartCoroutine(ApplyPowerUp(powerUp));
		}
	}

	private IEnumerator ApplyPowerUp(PowerUp powerUp)
	{
		float timer = 0;

        switch (powerUp.powerUpType)
		{
			case PowerUpType.speedBoost:

				speedPowerUpsActive++;

				speedBoostVisualModule.enabled = true;

				while (timer < powerUp.duration && !IsGameOver) {
					if (isKnockedOut)
						yield break;

					speedBonus = Mathf.Max(powerUp.value, speedBonus);

					timer += Time.deltaTime;

					yield return null;
				}

				speedPowerUpsActive--;

				if (speedPowerUpsActive == 0)
					speedBoostVisualModule.enabled = false;

				speedBonus = 0;

				break;


			case PowerUpType.jumpBoost:

				jumpPowerUpsActive++;

				jumpBoostVisualModule.enabled = true;

				while (timer < powerUp.duration && !IsGameOver)
				{
					if (isKnockedOut)
						yield break;


					jumpBonus = Mathf.Max(powerUp.value, jumpBonus);

					timer += Time.deltaTime;

					yield return null;
				}


				jumpPowerUpsActive--;

				if (jumpPowerUpsActive == 0)
					jumpBoostVisualModule.enabled = false;


				jumpBonus = 0;

				break;


			case PowerUpType.invincibilty:

				invincibleTimer = 0;

				if (isInvincible)
					yield break;


				OnPlayerInvincibility?.Invoke();

				heartParticlesEmissionModule.enabled = true;

				SoundManager.Instance.CacheCurrentMusic();

				SoundManager.Instance.PlayMusic(SoundManager.Instance.invincibility);

				rainbow.Activate();

				float previousMass = rb.mass ;
				rb.mass = 9999;

				while (invincibleTimer < powerUp.duration && !IsGameOver)
				{
					isInvincible = true;

					invincibleTimer += Time.deltaTime;
					yield return null;
				}

				OnPlayerInvincibilityEnd?.Invoke();

				isInvincible = false;

				rainbow.Deactivate();

				heartParticlesEmissionModule.enabled = false;


				rb.mass = previousMass;

				SoundManager.Instance.RestoreCachedMusic();

				break;

		}
	}
	#endregion
}
