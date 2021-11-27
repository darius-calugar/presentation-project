using UnityEngine;

public class Hook : MonoBehaviour
{
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private Material     activeMaterial;
	[SerializeField] private float        activeWidth;
	[SerializeField] private Material     closestMaterial;
	[SerializeField] private float        closestWidth;

	private PlayerController _player;

	private void Start()
	{
		_player = FindObjectOfType<PlayerController>();
	}

	private void Update()
	{
		lineRenderer.SetPosition(1, _player.transform.position - transform.position);
		if (IsActive)
		{
			lineRenderer.enabled         = true;
			lineRenderer.material        = activeMaterial;
			lineRenderer.widthMultiplier = activeWidth;
		}
		else if (IsClosest)
		{
			lineRenderer.enabled         = true;
			lineRenderer.material        = closestMaterial;
			lineRenderer.widthMultiplier = closestWidth;
		}
		else
		{
			lineRenderer.enabled = false;
		}
	}

	private bool IsActive =>
		_player.hooked && _player.activeHook == this;

	private bool IsClosest =>
		_player.hooked && _player.closestHook == this && _player.activeHook != _player.closestHook && _player.canHook ||
		!_player.hooked && _player.closestHook == this && _player.canHook;
}