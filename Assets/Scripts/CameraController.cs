using UnityEngine;

public class CameraController : MonoBehaviour
{
	private     GameController game;
	private new Camera         camera;

	private void Start()
	{
		game   = FindObjectOfType<GameController>();
		camera = GetComponent<Camera>();
	}

	private void Update()
	{
		camera.transform.position =  (game.player1.transform.position + game.player2.transform.position) / 2;
		camera.transform.position += camera.transform.rotation * Vector3.back * 100;
		camera.orthographicSize   =  Mathf.Clamp((game.player1.transform.position - game.player2.transform.position).magnitude / 2, 15, 200);
	}
}