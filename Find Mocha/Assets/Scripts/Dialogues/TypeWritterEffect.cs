using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypeWritterEffect : MonoBehaviour
{

    public float typingSpeed;

    //private TMP_Text dialogText;

    public bool IsRunning { get; private set; }


    private readonly Dictionary<HashSet<char>, float> punctuations = new Dictionary<HashSet<char>, float>()
    {
        {new HashSet<char>(){'.', '!', '?'}, 0.6f },
        {new HashSet<char>(){',', ';', '-', ':'}, 0.3f }
    };

    private Coroutine typingCoroutine;

    public void RunDialog(string textToType, TMP_Text dialogText)
    {
        typingCoroutine = StartCoroutine(TypeText(textToType, dialogText));
    }

    public void Stop()
    {
        StopCoroutine(typingCoroutine);
        IsRunning = false;
    }

    private IEnumerator TypeText(string textToType, TMP_Text dialogText)
    {
        IsRunning = true;

        float t = 0;
        int charIndex = 0;

        while(charIndex < textToType.Length)
        {
            int lastCharIndex = charIndex;



            t += Time.deltaTime * typingSpeed;

            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);


            for(int i = lastCharIndex; i<charIndex; i++)
            {
                bool isLast = i >= textToType.Length-1;

                dialogText.text = textToType.Substring(0, i + 1);

                SoundManager.Instance.PlaySound(SoundManager.Instance.text_char);

                if (IsPunctuation(textToType[i], out float waitTime) && !isLast)
                {
                    if (IsPunctuation(textToType[i+1], out float secondWaitTime))
                    {
                        waitTime /= 2;
                    }

                    yield return new WaitForSeconds(waitTime);
                }
            }



            yield return null;
        }

        IsRunning = false;
    }

    private bool IsPunctuation(char character, out float waitTime)
    {
        foreach(KeyValuePair<HashSet<char>, float> punctuationCategory in punctuations)
        {
            if (punctuationCategory.Key.Contains(character))
            {
                waitTime = punctuationCategory.Value;
                return true;
            }
        }

        waitTime = default;

        return false;
    }
}
