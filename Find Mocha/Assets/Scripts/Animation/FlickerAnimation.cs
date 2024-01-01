using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public interface IAnimation
{
    public void Activate();
    public void Deactivate();
}

public enum MediaType
{
    Text,
    Sprite,
    Image
}

public class FlickerAnimation : MonoBehaviour, IAnimation
{
    [Header("Animation properties")]

    [SerializeField]
    private float speed;
    [SerializeField]
    private float baseAlpha;
    [SerializeField, Range(0f, 1f)]
    private float depth;
    [SerializeField, Range(0f, 2*Mathf.PI)]
    private float phaseLead;
    [SerializeField]
    private bool setPhaseLeadForMax;

    [Header("Settings")]
    [SerializeField]
    private MediaType mediaType;
    [SerializeField]
    private bool useRealTime = true;
    [SerializeField]
    private bool activateOnEnable = false;


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

    
    private bool activated;


#if UNITY_EDITOR

    private void OnValidate()
    {
        CheckParameters();
    }

#endif


    private void OnDisable()
    {
        Deactivate();
    }

    private void OnEnable()
    {
        if(activateOnEnable)
            Activate();
    }

    private void Awake()
    {
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

        CheckParameters();
    }


    public void Activate()
    {
        if (activated)
            return;

        activated = true;
        if (useRealTime)
            StartCoroutine(ManageFlickerIndependent());
        else
            StartCoroutine(ManageFlicker());
    }
    public void Deactivate()
    {
        activated = false;
        StopAllCoroutines();
        ResetAlpha();
    }



    private void CheckParameters()
    {
        float max = baseAlpha + depth;
        float min = baseAlpha - depth;

        if (depth == 0 || min >= 1 || max <= 0)
        {
            Debug.LogWarning("Set up settings for " + gameObject + ", does not render any animation.");
        }
        else if (setPhaseLeadForMax)
        {
            phaseLead = -Mathf.Acos((1 - baseAlpha) / depth);
        }
    }

    private IEnumerator ManageFlickerIndependent()
    {
        float referenceTime = Time.unscaledTime;

        while (activated)
        {
            UpdateFlicker(Time.unscaledTime - referenceTime);

            yield return null;
        }
    }
    private IEnumerator ManageFlicker()
    {
        float referenceTime = Time.time;

        while (activated)
        {
            UpdateFlicker(Time.time - referenceTime);

            yield return null;
        }
    }

    private void UpdateFlicker(float time)
    {
        float alpha = Mathf.Clamp(baseAlpha + Mathf.Cos(phaseLead + time * speed) * depth, 0f, 1f);

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

    private void ResetAlpha()
    {
        Color color;

        switch (mediaType)
        {
            case MediaType.Text:

                color = targetText.color;
                color.a = 1;
                targetText.color = color;

                break;

            case MediaType.Sprite:

                color = targetSpriteRenderer.color;
                color.a = 1;
                targetSpriteRenderer.color = color;

                break;

            case MediaType.Image:

                color = targetImage.color;
                color.a = 1;
                targetImage.color = color;

                break;
        }
    }
}
