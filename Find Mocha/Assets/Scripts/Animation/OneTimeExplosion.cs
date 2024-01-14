using UnityEngine;

public class OneTimeExplosion : MonoBehaviour
{
    [SerializeField]
    private float destroyAfter;

    private void Start()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.explosion);
        Destroy(gameObject, destroyAfter);
    }
}
