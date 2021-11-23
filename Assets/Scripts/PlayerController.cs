using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float moveSpeed;
	[SerializeField] private float jumpForce;
	[SerializeField] private float playerGravity;

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
		if (_shouldJump && CanJump()) Jump();
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

	private bool CanJump()
	{
		return _collisionNormal.HasValue && Vector2.Dot(_collisionNormal.Value, Vector2.up) > .5f;
	}

	private void Move()
	{
		_rigidbody.velocity = new Vector3(
			moveSpeed * _inputMovement.x,
			_rigidbody.velocity.y,
			0
		);
	}

	private void Jump()
	{
		_rigidbody.velocity += Vector3.up * jumpForce;
		_shouldJump         =  false;
	}
}