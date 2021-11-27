using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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
	[HideInInspector] public Vector2? collisionOffset;
	[HideInInspector] public Vector2? collisionNormal;
	[HideInInspector] public Vector2  inputMovement = Vector2.zero;
	[HideInInspector] public bool     shouldJump;

	// Hooks state
	[HideInInspector] public List<Hook> hooks;
	[HideInInspector] public Hook       activeHook;
	[HideInInspector] public Hook       closestHook;
	[HideInInspector] public bool       canHook;
	[HideInInspector] public bool       hooked;
	[HideInInspector] public bool       shouldHook;
	[HideInInspector] public bool       shouldUnhook;

	private void Start()
	{
		_rigidbody      = GetComponent<Rigidbody>();
		Physics.gravity = Vector3.down * playerGravity;
		hooks           = FindObjectsOfType<Hook>().ToList();
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
		if (collisionNormal == null || Vector2.Dot(contact.normal, Vector2.up) > Vector2.Dot(collisionNormal.Value, Vector2.up))
		{
			collisionOffset = contact.point - transform.position;
			collisionNormal = contact.normal;
		}
	}

	private void OnCollisionExit()
	{
		collisionOffset = null;
		collisionNormal = null;
	}

	private void OnDrawDebug()
	{
		if (collisionOffset.HasValue && collisionNormal.HasValue)
		{
			Debug.DrawRay(
				(Vector3)collisionOffset.Value + transform.position,
				collisionNormal.Value.normalized,
				Color.red
			);
		}
		foreach (var hook in hooks)
		{
			Debug.DrawLine(
				transform.position,
				hook.transform.position,
				hook == closestHook
					? canHook
						? hooked
							? Color.red
							: Color.green
						: Color.yellow
					: Color.gray
			);
		}
	}

	private void GetInput()
	{
		inputMovement = new Vector2(
			Input.GetAxis("Horizontal"),
			Input.GetAxis("Vertical")
		);
		shouldJump   = Input.GetButton("Jump");
		shouldHook   = Input.GetButtonDown("Hook");
		shouldUnhook = Input.GetButtonUp("Hook");
	}

	private void ComputeHookState()
	{
		closestHook = hooks
			.OrderBy(hook => Vector2.Distance(transform.position, hook.transform.position))
			.FirstOrDefault();
		if (!hooked) activeHook = closestHook;
		canHook = Vector2.Distance(transform.position, closestHook.transform.position) <= hookRange;

		if (canHook && shouldHook)
		{
			hooked     = true;
			shouldHook = false;
		}
		if (hooked && shouldUnhook)
		{
			hooked       = false;
			shouldUnhook = false;
		}
	}

	private bool OnGround()
	{
		return collisionNormal.HasValue && Vector2.Dot(collisionNormal.Value, Vector2.up) > .5f;
	}

	private void Move()
	{
		var acceleration = Vector2.zero;
		if (OnGround())
		{
			var currentSpeed = _rigidbody.velocity.x;
			var targetSpeed  = inputMovement.x * groundMoveSpeed;
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
			acceleration = Vector2.right * (inputMovement.x * airMoveAcceleration * Time.fixedDeltaTime);
		}

		if (shouldJump && OnGround())
		{
			acceleration += new Vector2(
				jumpHorizontalForce * inputMovement.x,
				jumpVerticalForce
			);
			shouldJump = false;
		}

		_rigidbody.velocity += (Vector3)acceleration;

		Debug.DrawLine(transform.position, transform.position + _rigidbody.velocity, Color.blue);
		if (hooked)
		{
			var hookDirection = (activeHook.transform.position - transform.position).normalized;
			var hookPull      = -Vector2.Dot(_rigidbody.velocity, hookDirection);
			if (hookPull > 0)
			{
				var normalizedHookPull = maxHookPull * hookPull / (hookPull + maxHookPull);
				_rigidbody.velocity += hookDirection * normalizedHookPull;
			}
		}
	}
}