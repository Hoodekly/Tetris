using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
	private Slider slider;
	
	public void Awake()
	{
		slider = GetComponent<Slider>();
		slider.value = AudioListener.volume / AudioInit.MAX_VOLUME;
	}
	public void SetVolume(float volume)
	{
		AudioListener.volume = volume * AudioInit.MAX_VOLUME;
	}
}
