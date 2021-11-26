using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	// Movement
	[SerializeField] private float jumpVerticalForce;
	[SerializeField] private float jumpHorizontalForce;
	[SerializeField] private float playerGravity;
	[SerializeField] private float groundMoveSpeed;
	[SerializeField] private float groundAcceleration;
	[SerializeField] private float groundDeceleration;
	[SerializeField] private float groundDrag;
	[SerializeField] private float airMoveAcceleration;

	// Hooks
	[SerializeField] private float hookRange;
	[SerializeField] private float maxHookPull;

	private Rigidbody _rigidbody;

	// Movement state
	private Vector2? _collisionOffset;
	private Vector2? _collisionNormal;
	private Vector2  _inputMovement = Vector2.zero;
	private bool     _shouldJump;

	// Hooks state
	private List<Hook> _hooks;
	private Hook       _closestHook;
	private bool       _canHook;
	private bool       _hooked;
	private bool       _shouldHook;
	private bool       _shouldUnhook;

	private void Start()
	{
		_rigidbody      = GetComponent<Rigidbody>();
		Physics.gravity = Vector3.down * playerGravity;
		_hooks          = FindObjectsOfType<Hook>().ToList();
	}

	private void Update()
	{
		GetInput();
		ComputeHookState();
		OnDrawDebug();
	}

	private void FixedUpdate()
	{
		Move();
	}

	private void OnCollisionStay(Collision other)
	{
		var contact = other.GetContact(0);
		if (_collisionNormal == null || Vector2.Dot(contact.normal, Vector2.up) > Vector2.Dot(_collisionNormal.Value, Vector2.up))
		{
			_collisionOffset = contact.point - transform.position;
			_collisionNormal = contact.normal;
		}
	}

	private void OnCollisionExit()
	{
		_collisionOffset = null;
		_collisionNormal = null;
	}

	private void OnDrawDebug()
	{
		if (_collisionOffset.HasValue && _collisionNormal.HasValue)
		{
			Debug.DrawRay(
				(Vector3)_collisionOffset.Value + transform.position,
				_collisionNormal.Value.normalized,
				Color.red
			);
		}
		foreach (var hook in _hooks)
		{
			Debug.DrawLine(
				transform.position,
				hook.transform.position,
				hook == _closestHook
					? _canHook
						? _hooked
							? Color.red
							: Color.green
						: Color.yellow
					: Color.gray
			);
		}
	}

	private void GetInput()
	{
		_inputMovement = new Vector2(
			Input.GetAxis("Horizontal"),
			Input.GetAxis("Vertical")
		);
		_shouldJump   = Input.GetButton("Jump");
		_shouldHook   = Input.GetButtonDown("Hook");
		_shouldUnhook = Input.GetButtonUp("Hook");
	}

	private void ComputeHookState()
	{
		_closestHook = _hooks
			.OrderBy(hook => Vector2.Distance(transform.position, hook.transform.position))
			.First();
		_canHook = Vector2.Distance(transform.position, _closestHook.transform.position) <= hookRange;

		if (_canHook && _shouldHook)
		{
			_hooked     = true;
			_shouldHook = false;
		}
		if (_hooked && _shouldUnhook)
		{
			_hooked       = false;
			_shouldUnhook = false;
		}
	}

	private bool OnGround()
	{
		return _collisionNormal.HasValue && Vector2.Dot(_collisionNormal.Value, Vector2.up) > .5f;
	}

	private void Move()
	{
		var acceleration = Vector2.zero;
		if (OnGround())
		{
			var currentSpeed = _rigidbody.velocity.x;
			var targetSpeed  = _inputMovement.x * groundMoveSpeed;
			if (targetSpeed - currentSpeed > 0)
			{
				if (currentSpeed >= 0 && targetSpeed >= 0)
					acceleration = Vector2.right * Math.Max(Math.Min(groundAcceleration * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
				else if (currentSpeed < 0 && targetSpeed > 0)
					acceleration = Vector2.right * Math.Max(Math.Min(groundDeceleration * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
				else
					acceleration = Vector2.right * Math.Max(Math.Min(groundDrag * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
			}
			else if (targetSpeed - currentSpeed < float.Epsilon)
			{
				if (currentSpeed <= 0 && targetSpeed <= 0)
					acceleration = Vector2.right * Math.Min(Math.Max(-groundAcceleration * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
				else if (currentSpeed > 0 && targetSpeed < 0)
					acceleration = Vector2.right * Math.Min(Math.Max(-groundDeceleration * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
				else
					acceleration = Vector2.right * Math.Min(Math.Max(-groundDrag * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
			}
		}
		else
		{
			acceleration = Vector2.right * (_inputMovement.x * airMoveAcceleration * Time.fixedDeltaTime);
		}

		if (_shouldJump && OnGround())
		{
			acceleration += new Vector2(
				jumpHorizontalForce * _inputMovement.x,
				jumpVerticalForce
			);
			_shouldJump = false;
		}

		_rigidbody.velocity += (Vector3)acceleration;

		Debug.DrawLine(transform.position, transform.position + _rigidbody.velocity, Color.blue);
		if (_hooked)
		{
			var hookDirection = (_closestHook.transform.position - transform.position).normalized;
			var hookPull      = -Vector2.Dot(_rigidbody.velocity, hookDirection);
			if (hookPull > 0)
			{
				var normalizedHookPull = maxHookPull * hookPull / (hookPull + maxHookPull);
				_rigidbody.velocity += hookDirection * normalizedHookPull;
			}
		}
	}
}