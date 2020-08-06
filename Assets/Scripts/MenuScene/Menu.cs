using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
	public void Quit()
	{
		Application.Quit();
	}
	public void StartBasicMode()
	{
		Game.IsAdvancedMode = false;
		Game.Width = 10;
		Game.Height = 20;
		SceneManager.LoadScene("Game");
	}
	public void StartAdvancedMode()
	{
		Game.IsAdvancedMode = true;
		Game.Width = 12;
		Game.Height = 20;
		SceneManager.LoadScene("Game");
	}
}
