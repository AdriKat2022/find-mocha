using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance => instance;

    [SerializeField]
    private AudioSource musicSource, effectSource, effectRepeatSource;


    [Header("Musics")]

    public AudioClip mainMenu;
    public AudioClip defaultLevel, invincibility, gameOver;

    [Header("Sound effects")]

    public AudioClip jump;
    public AudioClip damage, heal, hit1, hit2, invincibled,
        select1, select2, select3,
        collect1, collect2, collect3,
        onground,
        text_char,
        button_press,
        level_clear, revive,
        low_hp_sound, gameOverSound;


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
    public void PlayLoopSound(AudioClip clip, bool restart = false)
    {
        effectRepeatSource.loop = true;
        if (!restart && clip == effectRepeatSource.clip)
        {
            if(!effectRepeatSource.isPlaying)
                effectRepeatSource.Play();
            return;
        }

        effectRepeatSource.clip = clip;
        effectRepeatSource.Play();
    }

    public void StopLoopSound(bool force = false)
    {
        effectRepeatSource.loop = false;
        if (force)
            effectRepeatSource.Stop();
    }

    public void PlayMusic(AudioClip clip, float wait = 0)
    {
        StartCoroutine(PlayMusicAfter(clip, wait));
    }

    public void PlayMusicNow(AudioClip clip, bool restart = false)
    {
        if (!restart && clip == musicSource.clip)
        {
            if(!musicSource.isPlaying)
                musicSource.Play();
            return;
        }

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    private IEnumerator PlayMusicAfter(AudioClip clip, float delay)
    {
        if(delay>0)
            yield return new WaitForSecondsRealtime(delay);

        PlayMusicNow(clip);
    }
}
