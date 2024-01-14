using UnityEngine;

public class HeartGunUnlocker : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered || collision == null)
            return;

        if(collision.TryGetComponent(out PlayerController player))
        {
            triggered = true;
            animator.SetTrigger("Collected");
            player.UnlockHeartGun();
            Destroy(gameObject, 4f);
        }
    }
}
