using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ProximityAlpha : MonoBehaviour
{
    [Header("Distances")]
    [SerializeField]
    private Axis checkedAxes;
    [SerializeField]
    private float maxDistance;
    [SerializeField]
    private float minDistance;

    [Header("Alpha values")]
    [SerializeField]
    private float maxAlpha = 1;
    [SerializeField]
    private float minAlpha = 0;


    [Header("Settings")]
    [SerializeField]
    private MediaType mediaType;


    [Header("Overrides")]

    [SerializeField]
    private Image overrideTargetImage;
    [SerializeField]
    private SpriteRenderer overrideTargetSpriteRenderer;
    [SerializeField]
    private TMP_Text overrideTargetText;



    private Image targetImage;
    private TMP_Text targetText;
    private SpriteRenderer targetSpriteRenderer;

    private Transform player;

    private float distanceFromPlayer;
    private float alpha;


#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }

#endif


    private void Awake()
    {
        distanceFromPlayer = maxDistance;

        switch (mediaType)
        {
            case MediaType.Text:
                if (overrideTargetText != null)
                    targetText = overrideTargetText;
                else
                    targetText = GetComponent<TMP_Text>();
                break;

            case MediaType.Sprite:
                if (overrideTargetSpriteRenderer != null)
                    targetSpriteRenderer = overrideTargetSpriteRenderer;
                else
                    targetSpriteRenderer = GetComponent<SpriteRenderer>();
                break;

            case MediaType.Image:
                if (overrideTargetImage != null)
                    targetImage = overrideTargetImage;
                else
                    targetImage = GetComponent<Image>();
                break;
        }
    }

    private void Start()
    {
        player = PlayerController.Instance.transform;
    }

    private void Update()
    {
        CalculateAlpha();
        UpdateAlpha();
    }

    private void CalculateAlpha()
    {
        if (player == null)
            return;

        switch (checkedAxes)
        {
            case Axis.X:
                distanceFromPlayer = Mathf.Abs((player.position - transform.position).x);
                break;

            case Axis.Y:
                distanceFromPlayer = Mathf.Abs((player.position - transform.position).y);
                break;

            case Axis.Default:
                distanceFromPlayer = (player.position - transform.position).magnitude;
                break;
        }

        alpha = Mathf.Lerp(minAlpha, maxAlpha, (maxDistance-distanceFromPlayer)/(maxDistance-minDistance));
    }

    private void UpdateAlpha()
    {
        Color color;

        switch (mediaType)
        {
            case MediaType.Text:

                color = targetText.color;
                color.a = alpha;
                targetText.color = color;

                break;

            case MediaType.Sprite:

                color = targetSpriteRenderer.color;
                color.a = alpha;
                targetSpriteRenderer.color = color;

                break;

            case MediaType.Image:

                color = targetImage.color;
                color.a = alpha;
                targetImage.color = color;

                break;
        }
    }
}
