using UnityEngine;

public class OneTimeExplosion : MonoBehaviour
{
	[SerializeField]
	private Explosion_Sound sound;
	[SerializeField]
	private float destroyAfter;

	private enum Explosion_Sound
	{
		None,
		Aggressive,
		Soft,
		Hearts
	}

	private void Start()
	{
		PlaySound();
		Destroy(gameObject, destroyAfter);
	}

	private void PlaySound()
	{
		switch (sound)
		{
			case Explosion_Sound.Aggressive:
				SoundManager.Instance.PlaySound(SoundManager.Instance.explosion);
				break;

            case Explosion_Sound.Soft:
                SoundManager.Instance.PlaySound(SoundManager.Instance.monsterHit);
                break;

            case Explosion_Sound.Hearts:
                SoundManager.Instance.PlaySound(SoundManager.Instance.low_hp_sound);
                break;
        }
	}
}
