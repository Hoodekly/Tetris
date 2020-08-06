using UnityEngine;

public class AudioInit : MonoBehaviour
{
	public const float STANDARD_VOLUME = 0.2f;
	public const float MAX_VOLUME = 0.4f;

	private void Awake()
	{
		AudioSource backgroundSoundtrack = GetComponent<AudioSource>();
		if (GameObject.FindGameObjectsWithTag("Music").Length > 1)
		{
			Destroy(backgroundSoundtrack);
		}
		else
		{
			AudioListener.volume = STANDARD_VOLUME;
			DontDestroyOnLoad(backgroundSoundtrack);
		}
	}
}
