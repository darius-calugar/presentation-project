using UnityEngine;

public class Hook : MonoBehaviour
{
	[SerializeField] private LineRenderer lineRendererP1;
	[SerializeField] private LineRenderer lineRendererP2;
	[SerializeField] private Material     activeMaterial;
	[SerializeField] private float        activeWidth;
	[SerializeField] private Material     closestMaterial;
	[SerializeField] private float        closestWidth;

	private GameController game;

	private void Start()
	{
		game = FindObjectOfType<GameController>();
	}

	private void Update()
	{
		lineRendererP1.SetPosition(1, game.player1.transform.position - transform.position);
		if (IsActive(game.player1))
		{
			lineRendererP1.enabled         = true;
			lineRendererP1.material        = activeMaterial;
			lineRendererP1.widthMultiplier = activeWidth;
		}
		else if (IsClosest(game.player1))
		{
			lineRendererP1.enabled         = true;
			lineRendererP1.material        = closestMaterial;
			lineRendererP1.widthMultiplier = closestWidth;
		}
		else
		{
			lineRendererP1.enabled = false;
		}

		lineRendererP2.SetPosition(1, game.player2.transform.position - transform.position);
		if (IsActive(game.player2))
		{
			lineRendererP2.enabled         = true;
			lineRendererP2.material        = activeMaterial;
			lineRendererP2.widthMultiplier = activeWidth;
		}
		else if (IsClosest(game.player2))
		{
			lineRendererP2.enabled         = true;
			lineRendererP2.material        = closestMaterial;
			lineRendererP2.widthMultiplier = closestWidth;
		}
		else
		{
			lineRendererP2.enabled = false;
		}
	}

	private bool IsActive(PlayerController player) =>
		player.hooked && player.activeHook == this;

	private bool IsClosest(PlayerController player) =>
		player.hooked && player.closestHook == this && player.activeHook != player.closestHook && player.canHook ||
		!player.hooked && player.closestHook == this && player.canHook;
}