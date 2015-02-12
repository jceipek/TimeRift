using UnityEngine;
using System.Collections;
using InControl;
using TR.Extensions.Interface;

public class AvatarController : MonoBehaviour {

	private IMotor[] _characterMotors;
	private CharacterMotor _defaultMotor;
	// private ClimbMotor _climbMotor;
	// private Transform _cameraTransform;

	// [SerializeField]
	// private float _climbSensitivity = 0.3f; // Closer to 0 = more sensitive
	[SerializeField]
	private float _maxMovementSpeed = 4.5f;
	// [SerializeField]
	// private float _pullingSpeedReduction = 0.01f;
	// [SerializeField]
	// private float _pullForce = 2f;
	// [SerializeField]
	// private bool _canPull = true;
	// [SerializeField]
	// private Pullable _pulling = null;
	// [SerializeField]
	// private float _releaseDistance = 1f;

	private Transform _transform;
	// private bool _wantsToPull = false;

	void OnValidate () {
		if (_defaultMotor != null) {
			_defaultMotor.MaxWalkSpeed = _maxMovementSpeed;
		}
	}

	void Awake () {
		_characterMotors = gameObject.GetInterfaces<IMotor>();
		_defaultMotor = gameObject.GetComponent<CharacterMotor>();
		// _climbMotor = gameObject.GetComponent<ClimbMotor>();
		_transform = transform;

		// _climbMotor.Enabled = false;
		_defaultMotor.Enabled = true;
		// _cameraTransform = Camera.main.transform;
	}

	void Start () {
		_defaultMotor.MaxWalkSpeed = _maxMovementSpeed;
	}

	void Update () {
		InputDevice device = InputManager.ActiveDevice;
		if (device != null) {
			int motorCount = _characterMotors.Length;
			for (int i = 0; i < motorCount; i++) {
				if (!_characterMotors[i].Enabled) {
					continue;
				}
				Vector2 inputVector = new Vector2(device.LeftStickX.Value, device.LeftStickY.Value);
				Vector3 tempVector = new Vector3( inputVector.x, 0, inputVector.y);
				Quaternion rot = Quaternion.AngleAxis(_transform.eulerAngles.y, Vector3.up);

				tempVector = rot * tempVector;
				inputVector = new Vector2(tempVector.x, tempVector.z);
				_characterMotors[i].Move(inputVector);

				if (device.Action1.WasPressed) {
					_characterMotors[i].Jump();
				}

				// if (_canPull && device.LeftBumper.WasPressed) {
				// 	_wantsToPull = true;
				// }
				// if (_canPull && device.LeftBumper.WasReleased) {
				// 	_wantsToPull = false;
				// 	StopPulling();
				// }

				// if (_pulling != null) {
				// 	Vector3 toPullable = _transform.position - _pulling.Position;
				// 	_pulling.PullWithForce(toPullable*_pullForce);
				// 	float releaseDistance = _releaseDistance;
				// 	if (toPullable.sqrMagnitude > releaseDistance * releaseDistance) {
				// 		StopPulling();
				// 	}
				// }
			}
		}
	}

	// private void StartPulling (Pullable pullable) {
	// 	if (_pullForce > pullable.Mass) {
	// 		_defaultMotor.MaxWalkSpeed = (1f - (pullable.Mass/_pullForce)) * _pullingSpeedReduction * _maxMovementSpeed;
	// 	} else {
	// 		_defaultMotor.MaxWalkSpeed = 0f;
	// 	}
	// 	_defaultMotor.WalkingBackward = true;
	// 	_pulling = pullable;
	// }

	// private void StopPulling () {
	// 	_defaultMotor.MaxWalkSpeed = _maxMovementSpeed;
	// 	_pulling = null;
	// 	_defaultMotor.WalkingBackward = false;
	// }

	// void OnTriggerEnter (Collider other) {
	// 	Climbable climbable = other.GetComponent<Climbable>();
	// 	if (climbable != null && Vector3.Dot(other.transform.forward, _transform.forward) < _climbSensitivity) {
	// 		_defaultMotor.Enabled = false;
	// 		_climbMotor.Enabled = true;
	// 		_climbMotor.ClimbingNormal = other.transform.forward;
	// 	}
	// }

	// void OnTriggerStay (Collider other) {
	// 	Pullable pullable = other.GetComponent<Pullable>();
	// 	if (_wantsToPull && pullable != null) {
	// 		_wantsToPull = false;
	// 		StartPulling(pullable);
	// 	}
	// }

	// void OnTriggerExit (Collider other) {
	// 	Climbable climbable = other.GetComponent<Climbable>();
	// 	if (climbable != null && Vector3.Dot(other.transform.forward, _transform.forward) < _climbSensitivity) {
	// 		_defaultMotor.Enabled = true;
	// 		_climbMotor.Enabled = false;
	// 	}
	// }
}