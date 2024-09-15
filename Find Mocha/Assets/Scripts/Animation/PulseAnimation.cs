using System.Collections;
using UnityEngine;

public class PulseAnimation : MonoBehaviour
{
    private const float MARGIN = 0.001f;

    [Header("Pulse Animation")]
    [SerializeField]
    private Transform logoTransform;
    [SerializeField, Tooltip("Growth intensity in scale unit")]
    private float growthOnPulse = 0.2f;
    [SerializeField, Tooltip("Duration of the pulse in seconds")]
    private float pulseDuration = 0.7f;

    [Header("Optimization")]
    [SerializeField, Tooltip("Use cached deltaTime to optimize the pulse animation (but might not be as accurate)")]
    private bool useCachedDeltaTime = false;

    private float originalScale;
    private IEnumerator pulseCoroutine;

    private void Start()
    {
        originalScale = transform.localScale.x;
    }

    public void Pulse(float intensity = 1f)
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        
        pulseCoroutine = PulseGrowCoroutine(intensity);
        StartCoroutine(pulseCoroutine);
    }

    private IEnumerator PulseGrowCoroutine(float intensity = 1f)
    {
        float tau = Mathf.Log(MARGIN / (growthOnPulse * intensity)) / pulseDuration;
        float interpolationFactor = 1 - Mathf.Exp(Time.deltaTime * tau);
        logoTransform.localScale = new Vector3(originalScale + growthOnPulse * intensity, originalScale + growthOnPulse * intensity, originalScale + growthOnPulse * intensity);

        while (Mathf.Abs(logoTransform.localScale.x - originalScale) > MARGIN)
        {
            if (!useCachedDeltaTime)
            {
                // Recalculate interpolation factor because Time.deltaTime can change
                interpolationFactor = 1 - Mathf.Exp(Time.deltaTime * tau);
            }

            float newScale = Mathf.Lerp(logoTransform.localScale.x, originalScale, interpolationFactor);
            logoTransform.localScale = Vector3.one * newScale;
            yield return null;
        }

        yield return null;
    }
}
