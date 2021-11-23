using System;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
	[SerializeField]                private PlayerController player;
	[SerializeField] [Range(0, 1f)] private float            movingAverageForce;
	[SerializeField] [Min(0)]       private float            accelerationWeight;
	[SerializeField] [Min(0)]       private float            accelerationClamp;

	private Rigidbody _playerRigidbody;

	private float   _movingAverage;
	private Vector3 _lastUpdateVelocity;

	private void Start()
	{
		_playerRigidbody = player.GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		var currentVelocity = _playerRigidbody.velocity;
		var acceleration    = currentVelocity.magnitude - _lastUpdateVelocity.magnitude;
		_lastUpdateVelocity = _playerRigidbody.velocity;
		_movingAverage = _movingAverage * (1 - movingAverageForce) +
						 Math.Min(Math.Max(acceleration * accelerationWeight, -accelerationClamp), accelerationClamp) * movingAverageForce;

		transform.localScale = Vector3.one + Vector3.up * _movingAverage;
		transform.rotation   = Quaternion.LookRotation(Vector3.forward, currentVelocity);
	}
}