using System.Collections;
using TMPro;
using UnityEngine;

public class HeartGunUnlocker : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [Header("Hint")]
    [SerializeField]
    private bool displayHint;
    [SerializeField]
    private float hintDuration;
    [SerializeField]
    private float hintWait;
    [SerializeField]
    private TMP_Text hint;

    private const float alphaSpeed = .5f;

    private bool triggered = false;

    private void Start()
    {
        hint.alpha = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered || collision == null)
            return;

        if(collision.TryGetComponent(out PlayerController player))
        {
            triggered = true;
            animator.SetTrigger("Collected");
            player.UnlockHeartGun();
            if (displayHint)
                StartCoroutine(DisplayHint(hintWait, hintDuration));
            else
                Destroy(gameObject, 4f);
        }
    }

    private IEnumerator DisplayHint(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        float timer = 0f;

        Debug.Log("Go!");

        bool start = true;

        while(timer < duration)
        {
            if (start)
            {
                hint.alpha = Mathf.Clamp01(timer * alphaSpeed);
                
                if(timer * alphaSpeed > 1f || timer > duration / 2)
                    start = false;
            }

            if (!start)
            {
                hint.alpha = Mathf.Clamp01((duration - timer) * alphaSpeed);
            }


            timer += Time.deltaTime;

            yield return null;
        }

        Destroy(gameObject);
    }
}
