using UnityEngine;
using System.Collections;


public class TimeEntity : MonoBehaviour {

	private TimeEntityInfo _myInfo;
	private CharacterMotor _motor;
	[SerializeField]
	private GameObject _camera;
	[SerializeField]
	Transform _viewPoint;
	private bool _isSimulated = true;

	public bool SimulateMe {
		get { return _isSimulated; }
		set {
			_isSimulated = value;
			_camera.SetActive(value);
		}
	}

	void OnValidate () {
		_camera.SetActive(_isSimulated);
	}

	private Transform _transform;
	void Awake () {
		_myInfo.entityId = this;
		_transform = transform;
		_motor = GetComponent<CharacterMotor>();
	}

	void Start () {
		TimeManager.g.RegisterEntity(this);
	}

	public void SetTo (TimeEntityInfo info) {
		if (!_isSimulated) {
			_transform.position = info.location;
			_transform.rotation = info.rotation;
		}
	}

	public TimeEntityInfo Simulate () {
		if (!_isSimulated) { return _myInfo; }
		_motor.Simulate();
		// _transform.position += Time.fixedDeltaTime * _transform.forward;
		_myInfo.location = _transform.position;
		_myInfo.rotation = _transform.rotation;
		return _myInfo;
	}

	public Vector3 Forward {
		get {
			return _viewPoint.forward;
		}
	}

	public Vector3 EyeLocation {
		get {
			return _viewPoint.position;
		}
	}

	public Vector3 Location {
		get {
			return _transform.position;
		}
	}

	public Quaternion Rotation {
		get {
			return _transform.rotation;
		}
	}
}