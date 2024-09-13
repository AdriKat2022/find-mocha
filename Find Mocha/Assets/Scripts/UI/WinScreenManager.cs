using System;
using System.Collections;
using TMPro;
using UnityEngine;

[Serializable]
public struct TimeData
{
    public int minutes;
    public int seconds;
    public int milliseconds;

    public TimeData(float time)
    {
        minutes = Mathf.FloorToInt(time / 60);
        seconds = Mathf.FloorToInt(time % 60);
        milliseconds = Mathf.FloorToInt((time * 1000) % 1000);
    }

    public float TotalTimeSeconds()
    {
        return minutes * 60 + seconds + milliseconds / 1000;
    }
    public string FormatTime()
    {
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
}

public class WinScreenManager : MonoBehaviour
{
    [Header("Golden Stats")]
    [SerializeField]
    private TimeData maxGoldenTime;
    [SerializeField]
    private Color goldenColor;
    [SerializeField]
    private float pulseIntensity = 1.5f;
    [SerializeField]
    private float pulseDuration = 1.5f;
    [SerializeField, Tooltip("Delay between each stat to be checked golden.")]
    private float goldenStatDelay = .5f;

    [Header("Animations")]
    [SerializeField]
    private float waitBeforeFadeIn = 3f;
    [SerializeField]
    private float fadeDuration = 1f;
    [SerializeField, Tooltip("The delay to have between each stat.")]
    private float fadeInElementDelay = 0.5f;

    [Header("Texts")]
    [SerializeField]
    private TextMeshProUGUI headerCoinText;
    [SerializeField]
    private TextMeshProUGUI headerTimeText;
    [SerializeField]
    private TextMeshProUGUI headerDepressionsText;
    [SerializeField]
    private TextMeshProUGUI coinText;
    [SerializeField]
    private TextMeshProUGUI timeText;
    [SerializeField]
    private TextMeshProUGUI depressionsText;

    private void Awake()
    {
        headerCoinText.alpha = 0;
        headerTimeText.alpha = 0;
        headerDepressionsText.alpha = 0;
        coinText.alpha = 0;
        timeText.alpha = 0;
        depressionsText.alpha = 0;
        GetStats();
        StartCoroutine(FadeInAll());
    }

    private void GetStats()
    {
        coinText.text = CollectibleManager.TotalCoinsCollected.ToString();
        timeText.text = new TimeData(CollectibleManager.TotalTimeSeconds).FormatTime();
        depressionsText.text = CollectibleManager.TotalDepressions.ToString();
    }

    private IEnumerator FadeInAll()
    {
        yield return new WaitForSeconds(waitBeforeFadeIn);
        StartCoroutine(FadeInElement(headerCoinText));
        StartCoroutine(FadeInElement(coinText));
        yield return new WaitForSeconds(fadeInElementDelay);
        StartCoroutine(FadeInElement(headerTimeText));
        StartCoroutine(FadeInElement(timeText));
        yield return new WaitForSeconds(fadeInElementDelay);
        StartCoroutine(FadeInElement(headerDepressionsText));
        StartCoroutine(FadeInElement(depressionsText));
        
        yield return new WaitForSeconds(.5f);

        StartCoroutine(VerifyStats());
    }

    private IEnumerator FadeInElement(TextMeshProUGUI elements)
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            elements.alpha = t;
            yield return null;
        }
    }

    private IEnumerator VerifyStats()
    {
        if (CollectibleManager.TotalCoinsCollected == CollectibleManager.TotalCoinsReferenced)
        {
            MakeStatGolden(coinText);
        }
        yield return new WaitForSeconds(goldenStatDelay);
        if (CollectibleManager.TotalTimeSeconds < maxGoldenTime.TotalTimeSeconds())
        {
            MakeStatGolden(timeText);
        }
        yield return new WaitForSeconds(goldenStatDelay);
        if (CollectibleManager.TotalDepressions == 0)
        {
            MakeStatGolden(depressionsText);
        }
    }

    private void MakeStatGolden(TextMeshProUGUI text)
    {
        text.color = goldenColor;
        StartCoroutine(PulseAnimation(text));
    }

    private IEnumerator PulseAnimation(TextMeshProUGUI text)
    {
        float t = pulseIntensity;

        text.transform.localScale = Vector3.one * t; 

        while (Mathf.Abs(t - 1) > 0.001f)
        {
            t = Mathf.Lerp(t, 1, Time.deltaTime / pulseDuration);
            text.transform.localScale = Vector3.one * t;
            yield return null;
        }

        text.transform.localScale = Vector3.one;
    }
}
