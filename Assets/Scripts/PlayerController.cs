using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float jumpForce;
	[SerializeField] private float playerGravity;
	[SerializeField] private float groundMoveSpeed;
	[SerializeField] private float groundAcceleration;
	[SerializeField] private float groundDeceleration;
	[SerializeField] private float groundDrag;
	[SerializeField] private float airMoveAcceleration;

	private Rigidbody _rigidbody;

	private Vector2? _collisionOffset;
	private Vector2? _collisionNormal;
	private Vector2  _inputMovement = Vector2.zero;
	private bool     _shouldJump;

	private void Start()
	{
		_rigidbody      = GetComponent<Rigidbody>();
		Physics.gravity = Vector3.down * playerGravity;
	}

	private void Update()
	{
		GetInput();
	}

	private void FixedUpdate()
	{
		Move();
		if (_shouldJump && OnGround()) Jump();
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

	private void OnDrawGizmos()
	{
		if (_collisionOffset.HasValue && _collisionNormal.HasValue)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(new Ray(
				(Vector3)_collisionOffset.Value + transform.position,
				_collisionNormal.Value.normalized
			));
		}
	}

	private void GetInput()
	{
		_inputMovement = new Vector2(
			Input.GetAxis("Horizontal"),
			Input.GetAxis("Vertical")
		);
		_shouldJump = Input.GetButton("Jump");
	}

	private bool OnGround()
	{
		return _collisionNormal.HasValue && Vector2.Dot(_collisionNormal.Value, Vector2.up) > .5f;
	}

	private void Move()
	{
		if (OnGround())
		{
			var   currentSpeed = _rigidbody.velocity.x;
			var   targetSpeed  = _inputMovement.x * groundMoveSpeed;
			float acceleration = 0;
			if (targetSpeed - currentSpeed > 0)
			{
				if (currentSpeed >= 0 && targetSpeed >= 0)
					acceleration = Math.Max(Math.Min(groundAcceleration * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
				else if (currentSpeed < 0 && targetSpeed > 0)
					acceleration = Math.Max(Math.Min(groundDeceleration * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
				else
					acceleration = Math.Max(Math.Min(groundDrag * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
			}
			else if (targetSpeed - currentSpeed < float.Epsilon)
			{
				if (currentSpeed <= 0 && targetSpeed <= 0)
					acceleration = Math.Min(Math.Max(-groundAcceleration * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
				else if (currentSpeed > 0 && targetSpeed < 0)
					acceleration = Math.Min(Math.Max(-groundDeceleration * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
				else
					acceleration = Math.Min(Math.Max(-groundDrag * Time.fixedDeltaTime, targetSpeed - currentSpeed), 0);
			}
			_rigidbody.velocity += Vector3.right * acceleration;
		}
		else
		{
			var acceleration = _inputMovement.x * airMoveAcceleration * Time.fixedDeltaTime;
			_rigidbody.velocity += Vector3.right * acceleration;
		}
	}

	private void Jump()
	{
		_rigidbody.velocity += Vector3.up * jumpForce;
		_shouldJump         =  false;
	}
}