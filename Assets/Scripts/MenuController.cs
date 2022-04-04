using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	public void OnPlay()
	{
		SceneManager.LoadScene(1);
	}
	
	public void OnExit()
	{
		Application.Quit();
	}
}