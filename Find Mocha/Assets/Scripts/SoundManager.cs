using System.Collections;
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
    public AudioClip damage, heal,
        hit1, hit2,
        coin,
        monsterHit,
        invincibled,
        explosion,
        select1, select2, select3,
        collect1, collect2, collect3,
        onground,
        text_char,
        button_press,
        level_clear, revive,
        low_hp_sound, gameOverSound,
        fell_sound, milk_squeak,
        heartSpawn;

    private int requiredScene;
    private bool fadingOutLoopSound = false;

    private AudioClip cachedMusic;


    public AudioClip GetCurrentMusic()
    {
        return musicSource.clip;
    }

    public void CacheCurrentMusic()
    {
        cachedMusic = musicSource.clip;
    }

    public void RestoreCachedMusic()
    {
        if (cachedMusic != null)
        {
            PlayMusicNow(cachedMusic);
            cachedMusic = null;
        }
    }

    public void ProgressivelyFadeOutLoopSound(float durationBeforeFadeout, float fadeoutSpeed)
    {
        fadingOutLoopSound = true;
        StartCoroutine(FadeOutLoopSound(durationBeforeFadeout, fadeoutSpeed));
    }

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

    public void PlaySound(AudioClip clip, float waitTime = 0)
    {
        if(waitTime > 0)
        {
            StartCoroutine(PlayOneShotAfterTime(effectSource, clip, waitTime));
            return;
        }
        effectSource.PlayOneShot(clip);
    }

    private IEnumerator PlayOneShotAfterTime(AudioSource source, AudioClip clip, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        source.PlayOneShot(clip);
    }

    public void PlayLoopSound(AudioClip clip, bool restart = false)
    {
        fadingOutLoopSound = false;
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
        fadingOutLoopSound = false;
        effectRepeatSource.loop = false;
        if (force)
            effectRepeatSource.Stop();
    }

    public void PlayMusic(AudioClip clip, float wait = 0)
    {
        StartCoroutine(PlayMusicAfterTime(clip, wait));
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

    private IEnumerator PlayMusicAfterTime(AudioClip clip, float delay, bool persist = false)
    {
        requiredScene = GameManager.Instance.SceneLoadNumber;

        if (delay>0)
            yield return new WaitForSecondsRealtime(delay);

        if (persist || requiredScene == GameManager.Instance.SceneLoadNumber)
            PlayMusicNow(clip);
    }

    private IEnumerator FadeOutLoopSound(float durationBeforeFadeout, float fadeoutSpeed)
    {
        yield return new WaitForSeconds(durationBeforeFadeout);


        while (effectRepeatSource.volume > 0 && fadingOutLoopSound)
        {
            effectRepeatSource.volume -= fadeoutSpeed * Time.deltaTime/100;
            yield return null;
        }

        effectRepeatSource.Stop();
        effectRepeatSource.volume = 1;
    }
}
