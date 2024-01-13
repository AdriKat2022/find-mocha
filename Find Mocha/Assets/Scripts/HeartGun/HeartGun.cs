using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HeartGun : MonoBehaviour
{
    [SerializeField]
    private GameObject heartPrefab;

    [SerializeField]
    private PlayerController controller;
    [SerializeField]
    private int heartNumber;
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



    private bool isActive;


#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.right * offsetX + Vector3.up * offsetY, .1f);
        Gizmos.DrawSphere(transform.position - Vector3.right * offsetX + Vector3.up * offsetY, .1f);
    }

#endif


    public void SetGunActive(bool activate)
    {
        isActive = activate;
    }


    private void Start()
    {
        SetGunActive(activateOnAwake);
    }

    private void Update()
    {
        if(isActive)
        {
            if (Input.GetKeyDown(KeyCode.Z))
                StartCoroutine(SendHeartsBurst(heartNumber));
        }
    }

    private Vector3 GetCurrentOffset()
    {
        return controller.IsFacingRight ? (Vector3.right * offsetX + Vector3.up * offsetY) : (-Vector3.right * offsetX + Vector3.up * offsetY);
    }

    private void SendHeart()
    {
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
