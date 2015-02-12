using UnityEngine;
using System.Collections;
using InControl;

[RequireComponent(typeof(Rigidbody))]

public class CharacterMotor : MonoBehaviour, IMotor
{
#region Variables
	// Input Cache
	private bool _jumpFlag = false;
	private Vector3 _inputVector;

	private bool _grounded = false;
	private bool _walkingBackward = false;
	private Vector3 _groundVelocity; // For moving platforms
	private float _maxWalkSpeed;

	private Transform _transform;
	private Rigidbody _rigidbody;
#endregion

#region Properties

	[SerializeField]
	private Collider _feetCollider;
	[SerializeField]
	private float _minMagToRotate = 1f;

// Accelerations
	[SerializeField]
	private float _walkAcceleration = 40f;

// Speeds
	[SerializeField]
	private float _rotationSpeed = 10f;

// Air
	[SerializeField]
	private float _inAirControl = 0.2f;
	[SerializeField]
	private float _airFriction = 0.1f;
	[SerializeField]
	private float _jumpHeight = 0.5f;

// Can Flags
	[SerializeField]
	private bool canJump = true;

#endregion

	void Awake ()
	{
		_transform = transform;
		_rigidbody = rigidbody;
	}

	void OnEnable () {
		_rigidbody.freezeRotation = true;
		_rigidbody.useGravity = true;
	}

	void Update () {

	}

	public void Simulate () {
		Vector3 planarVelocity = new Vector3(_rigidbody.velocity.x,0,_rigidbody.velocity.z);
		float planarMagnitude = planarVelocity.magnitude;
		if (_grounded) {
			// Apply a force that attempts to reach our target velocity
			var velocityChange = CalculateVelocityChange(_inputVector, Time.fixedDeltaTime);
			_rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

			if (canJump && _jumpFlag) {
				_jumpFlag = false;
				_rigidbody.AddForce(Vector3.up * CalculateJumpForce(), ForceMode.Impulse);
			}

			// By setting the _grounded to false in every FixedUpdate we avoid
			// checking if the character is not _grounded on OnCollisionExit()
			_grounded = false;
		} else { // In mid-air
			// Uses the input vector to affect the mid air direction
			var velocityChange = _inputVector * _inAirControl;

			Vector3 damping = Vector3.zero;
			if (planarMagnitude > _airFriction) {
				damping = -planarVelocity.normalized * _airFriction;
			}
			_rigidbody.AddForce(damping + velocityChange, ForceMode.VelocityChange);
		}

		if (planarMagnitude > _minMagToRotate) {
			float walkDir = 1f;
			if (_walkingBackward) {
				walkDir = -1f;
			}
			_transform.forward = Vector3.RotateTowards(_transform.forward, walkDir * planarVelocity, Time.fixedDeltaTime * _rotationSpeed, _minMagToRotate);
		}
	}

	// Unparent if we are no longer standing on our parent
	void OnCollisionExit (Collision collision) {
		if (collision.transform == _transform.parent) {
			_transform.parent = null;
		}
	}

	// If there are collisions check if the character is _grounded
	void OnCollisionStay (Collision col) {
		TrackGrounded(col);
	}

	void OnCollisionEnter (Collision col) {
		TrackGrounded(col);
	}

	// From the user input calculate using the set up speeds the velocity change
	private Vector3 CalculateVelocityChange (Vector3 inputVector, float dt) {
		// Calculate how fast we should be moving
		var relativeVelocity = _rigidbody.velocity + (inputVector * _walkAcceleration * dt);
		Vector3 planarVelocity = relativeVelocity; planarVelocity.y = 0f;
		if (planarVelocity.sqrMagnitude > _maxWalkSpeed * _maxWalkSpeed) {
			planarVelocity = Vector3.ClampMagnitude(planarVelocity, _maxWalkSpeed);
			relativeVelocity.x = planarVelocity.x;
			relativeVelocity.z = planarVelocity.z;
		}

		// Calculate the delta velocity
		var currRelativeVelocity = _rigidbody.velocity - _groundVelocity;
		var velocityChange = relativeVelocity - currRelativeVelocity;

		return velocityChange;
	}

	// From the jump height and gravity we deduce the upwards speed for the character to reach at the apex.
	private float CalculateJumpForce () {
		return _rigidbody.mass * Mathf.Sqrt(2f * _jumpHeight * Mathf.Abs(Physics.gravity.y));
	}

	// Check if the base of the capsule is colliding to track if it's _grounded
	private void TrackGrounded (Collision collision) {
		var maxHeight = _feetCollider.bounds.min.y + _feetCollider.bounds.size.y/2f * .9f;
		foreach (var contact in collision.contacts) {
			if (contact.point.y < maxHeight) {
				if (isKinematic(collision)) {
					// Get the ground velocity and we parent to it
					_groundVelocity = collision.rigidbody.velocity;
					_transform.parent = collision.transform;
				} else if (isStatic(collision)) {
					// Just parent to it since it's static
					_transform.parent = collision.transform;
				} else {
					// We are standing over a dynamic object,
					// set the _groundVelocity to Zero to avoid jiggers and extreme accelerations
					_groundVelocity = Vector3.zero;
				}

				_grounded = true;
				break;
			}
		}
	}

	private bool isKinematic (Collision collision) {
		return isKinematic(collision.transform);
	}

	private bool isKinematic (Transform transform) {
		return transform.rigidbody && transform.rigidbody.isKinematic;
	}

	private bool isStatic (Collision collision) {
		return isStatic(collision.transform);
	}

	private bool isStatic (Transform transform) {
		return transform.gameObject.isStatic;
	}

	public bool WalkingBackward {
		get { return _walkingBackward; }
		set { _walkingBackward = value; }
	}

	public bool Enabled {
		get { return this.enabled; }
		set { this.enabled = value; }
	}

	public float MaxWalkSpeed {
		set { _maxWalkSpeed = value; }
	}

	public void Move (Vector2 inputVector) {
		_inputVector = new Vector3(inputVector.x, 0f, inputVector.y);
	}

	public void Jump () {
		// Cache the input
		if (_grounded) {
			_jumpFlag = true;
		}
	}

}