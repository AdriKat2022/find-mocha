using System.Collections;
using UnityEngine;

public class HeartGun : MonoBehaviour
{
    [SerializeField]
    private GameObject heartPrefab;
    [SerializeField]
    private PlayerController controller;

    [Header("Heart Gun")]
    [SerializeField]
    private float shootCooldown;
    [SerializeField]
    private int heartNumberPerShot;
    [SerializeField]
    private float heartRate;

    [Header("Hearts properties")]
    [SerializeField]
    private float damagePerHeart;
    [Space]
    [SerializeField]
    private float heartAngle;
    [SerializeField]
    private float heartAngleVariance;
    [Space]
    [SerializeField]
    private float heartSpan;
    [SerializeField]
    private float heartSpanVariance;
    [Space]
    [SerializeField]
    private float heartSpeed;
    [SerializeField]
    private float heartSpeedVariance;


    [Header("Offset")]
    [SerializeField]
    private float offsetX;
    [SerializeField]
    private float offsetY;


    [Header("Debug")]
    [SerializeField]
    private bool activateOnAwake;


    private float time;

    private bool isActive;
    private bool isUnlocked;


#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.right * offsetX + Vector3.up * offsetY, .1f);
        Gizmos.DrawSphere(transform.position - Vector3.right * offsetX + Vector3.up * offsetY, .1f);
    }

#endif

    private void OnEnable()
    {
        PlayerController.OnPlayerKnockedOut += DisableHeartGun;
        PlayerController.OnPlayerReady += EnableHeartGun;
    }
    private void OnDisable()
    {
        PlayerController.OnPlayerKnockedOut -= DisableHeartGun;
        PlayerController.OnPlayerReady -= EnableHeartGun;
    }


    private void Awake()
    {
        SetGunActive(activateOnAwake);
        if (activateOnAwake)
            Debug.LogWarning("Activate on awake is enabled for the heart gun");

        time = 0f;
    }

    private void DisableHeartGun()
    {
        isActive = false;
    }
    private void EnableHeartGun()
    {
        isActive = true;
    }
    public void UnlockHeartGun()
    {
        isUnlocked = true;
    }
    private void SetGunActive(bool activate)
    {
        isActive = activate;
        isUnlocked = activate;
    }

    private void Update()
    {
        if (isActive && isUnlocked)
            CheckForShot();
    }

    private void CheckForShot()
    {
        time += Time.deltaTime;

        if (time < shootCooldown)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
            StartCoroutine(SendHeartsBurst(heartNumberPerShot));
    }

    private Vector3 GetCurrentOffset()
    {
        return controller.IsFacingRight ? (Vector3.right * offsetX + Vector3.up * offsetY) : (-Vector3.right * offsetX + Vector3.up * offsetY);
    }

    private void SendHeart()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.heartSpawn);

        GameObject heartInstance = Instantiate(heartPrefab, transform.position + GetCurrentOffset(), Quaternion.identity);
        heartInstance.TryGetComponent(out HeartProjectile projectile);
        projectile.Launch(
                GetRandomDirection(),
                GetRandom(heartSpeed, heartSpeedVariance),
                damagePerHeart,
                GetRandom(heartSpan, heartSpanVariance)
            );
    }

    private float GetRandom(float baseNumber, float variance)
    {
        return baseNumber + variance * Random.Range(-1f, 1f);
    }
    private Vector2 GetRandomDirection()
    {
        float angle = GetRandom(heartAngle, heartAngleVariance);
        Vector2 dir = new(Mathf.Cos(angle * Mathf.PI / 180), Mathf.Sin(angle * Mathf.PI / 180));

        dir.x = controller.IsFacingRight ? dir.x : -dir.x;

        return dir;
    }

    private IEnumerator SendHeartsBurst(int heartsNumberBurst)
    {
        time = 0;

        float timer = 0;
        int spawnedHearts = 0;

        while(spawnedHearts < heartsNumberBurst)
        {
            timer += Time.deltaTime;

            while(spawnedHearts < heartRate * timer && spawnedHearts < heartsNumberBurst)
            {
                spawnedHearts++;
                SendHeart();
            }

            yield return null;
        }
    }
}
