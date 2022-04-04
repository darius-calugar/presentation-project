using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI scoreP1Text;
	[SerializeField] private TextMeshProUGUI scoreP2Text;

	private int scoreP1;
	private int scoreP2;

	public PlayerController player1;
	public PlayerController player2;

	private void Start()
	{
		UpdateUI();
	}

	private void UpdateUI()
	{
		scoreP1Text.text = scoreP1.ToString("00");
		scoreP2Text.text = scoreP2.ToString("00");
	}

	public void AddP1Score()
	{
		scoreP1++;
		UpdateUI();
	}

	public void AddP2Score()
	{
		scoreP2++;
		UpdateUI();
	}
}