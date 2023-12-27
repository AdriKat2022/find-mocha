using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance => instance;

    [SerializeField]
    private AudioSource musicSource, effectSource;


    [Header("Musics")]

    public AudioClip mainMenu;
    public AudioClip defaultLevel, invincibility, gameOver;

    [Header("Sound effects")]

    public AudioClip jump;
    public AudioClip damage, heal, hit1, hit2, invincibled, select1, select2, select3, collect1, collect2, collect3, onground, text_char, button_press, level_clear, revive;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        //effectSource.clip = invincibled;
        //effectSource.Play();
        //effectSource.Pause();
    }

    public void PlaySound(AudioClip clip)
    {
        //Debug.Log("PLAY!");
        //effectSource.clip = clip;
        //effectSource.PlayScheduled(0);
        effectSource.PlayOneShot(clip);
        //effectSource.UnPause();
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
