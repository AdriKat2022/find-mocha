using System;
using System.Collections;
using UnityEngine;

public struct PlayerStats
{
    public float maxHp;
    public float hp;

    public PlayerStats(float maxHp, float hp)
    {
        this.maxHp = maxHp;
        this.hp = hp;
    }
}

public class PlayerController : MonoBehaviour, IDamageble
{
    private readonly float Fall_height = -15;

    [Header("Milk properties")]
    public float maxHp;
    public float speed;
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
    //public float fallMultiplier;

    [Header("INV and hurt animation")]
    public float hurtingFrames;
    public float invicibilityFrames;
    public float slowBlinking;
    public float fastBlinking;
    [Range(0f,1f)]
    public float slowFastThreshold;
    [SerializeField]
    private float lowHpThreshold;

    public float LowHpThreshold => lowHpThreshold;

    public IInteractable Interactable;



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



    #region Bonuses variables

    private float speedBonus;
    private float jumpBonus;
    private float invincibleTimer;

    #endregion Bonuses variables



    public static PlayerController Instance;


    public static event Action OnPlayerInvincibility;
    public static event Action OnPlayerInvincibilityEnd;
    public static event Action OnPlayerKnockedOut;
    public static event Action OnPlayerReady;

    public static event Action<PlayerStats> OnPlayerChangeHP;


#if UNITY_EDITOR

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

#endif

#if true
    private void DebugFunc()
    {
        if (Input.GetKeyDown(KeyCode.E))
            Damage(2);

        if (Input.GetKeyDown(KeyCode.A))
            Heal(2);

        if (Input.GetKeyDown(KeyCode.Z))
            Heal(200);

        if (Input.GetKeyDown(KeyCode.S))
            Damage(200);

        if (Input.GetKeyDown(KeyCode.Q))
            Damage(currentHp - 1);
    }

#endif

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
            //Debug.Log("Loading !");
        }
        else
        {
            currentHp = maxHp;
            //Debug.Log("Set max hp by default");
        }

        OnPlayerReady?.Invoke();
    }

    private void Update()
    {
        DebugFunc();

        if (isKnockedOut)
            return;

        CheckAlive();

        if (isKnockedOut)
            return;

        GroundCheck();

        if (isHurting || hasWon)
            return;

        if (dialogueUI.IsOpen)
            return;

        MovePlayer();

        AnimatorUpdate();

        CheckInteraction();

        _time += Time.deltaTime;
    }

    private void CheckInteraction()
    {
        if (Input.GetKey("e"))
        {
            if(Interactable != null)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.button_press);
                Interactable.Interact(this);
            }
        }
    }


    public bool CanJump => !isJumping || ((coyoteTime + lastTimeGrounded > _time) && coyoteUsable);
    private bool IsJumpBuffered => Input.GetKeyDown("space") || (lastJumpBuffered + bufferJumpTime > _time && !jumpBufferUsed);

    private void ManageJump()
    {
        if (Input.GetKeyDown("space"))
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
            Jump();
            coyoteUsable = false;
        }

        if(!jumpEndedEarly && isJumping && rb.velocity.y > 0 && !Input.GetKey("space"))
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * fastFallMultiplier);
            jumpEndedEarly = true;
        }

    }

    private void MovePlayer()
    {
        ManageJump();

        Vector2 newVelocity = rb.velocity;

        if (Input.GetKey("right"))
        {
            newVelocity.x = Mathf.Max(rb.velocity.x, speed + speedBonus);
            spriteRenderer.flipX = false;
        }
        else if (Input.GetKey("left"))
        {
            newVelocity.x = Mathf.Min(rb.velocity.x, -speed - speedBonus);
            spriteRenderer.flipX = true;
        }
        else if (rb.velocity.magnitude != 0f)
        {
            if (rb.velocity.x < -.01f)
                spriteRenderer.flipX = true;
            else if (rb.velocity.x > .01f)
                spriteRenderer.flipX = false;
            
            newVelocity.x = 0f;
        }
        newVelocity.y = Mathf.Max(-maxYSpeed, newVelocity.y);

        rb.velocity = newVelocity;
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce + jumpBonus);
        isJumping = true;
        //Debug.Log("Buf");
        animator.SetTrigger("bufJump");
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

    private void AnimatorUpdate()
    {
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isWalking", Mathf.Abs(rb.velocity.x) > 0.01);
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

    IEnumerator VictoryDance()
    {
        jumpForce = wonJumpForce;
        rb.velocity = Vector3.zero;

        while (true)
        {
            if(!isJumping)
                Jump();

            yield return null;
        }
    }

    private IEnumerator HurtPhase(bool overrideHitstun = false, float hitstun = 0)
    {
        if (isHurting || isInvulnerable)
            yield break;

        isHurting = true;
        animator.SetBool("isHurting", true);

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

    public void Heal(float heal)
    {
        if (isKnockedOut)
            return;

        SoundManager.Instance.PlaySound(SoundManager.Instance.heal);

        currentHp += heal;

        OnPlayerChangeHP?.Invoke(GetPlayerStats());

        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
    }

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
        return new PlayerStats(maxHp, currentHp);
    }
    public void LoadPlayerStats(PlayerStats stats)
    {
        maxHp = stats.maxHp;
        currentHp = stats.hp;
    }


    public void ApplyBonuses(PowerUp[] powerUps)
    {
        if (isKnockedOut)
            return;

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

                while (timer < powerUp.duration) {
                    if (isKnockedOut)
                        yield break;

                    speedBonus = Mathf.Max(powerUp.value, speedBonus);

                    timer += Time.deltaTime;

                    yield return null;
                }

                speedBonus = 0;

                break;




            case PowerUpType.jumpBoost:
                
                while (timer < powerUp.duration)
                {
                    if (isKnockedOut)
                        yield break;


                    jumpBonus = Mathf.Max(powerUp.value, jumpBonus);

                    timer += Time.deltaTime;

                    yield return null;
                }

                jumpBonus = 0;

                break;


            case PowerUpType.invincibilty:

                invincibleTimer = 0;

                if (isInvincible)
                {
                    yield break;
                }

                OnPlayerInvincibility?.Invoke();

                heartParticlesEmissionModule.enabled = true;

                SoundManager.Instance.PlayMusic(SoundManager.Instance.invincibility);

                rainbow.Activate();

                float previousMass = rb.mass ;
                rb.mass = 9999;

                while (invincibleTimer < powerUp.duration)
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

                SoundManager.Instance.StopMusic();

                break;

        }
    }
}
