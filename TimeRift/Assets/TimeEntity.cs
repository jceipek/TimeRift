using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct TimeEntityInfo
{
	public Vector3 location;
	public Quaternion rotation;
	public Quaternion viewRotation;
}

public class TimeEntity : MonoBehaviour {

	private TimeEntityInfo _myInfo;
	private CharacterMotor _motor;
	[SerializeField]
	private GameObject _camera;
	[SerializeField]
	private GameObject _viewportModel;
	private int _viewportModelLayer;
	[SerializeField]
	Transform _viewPoint;
	private bool _isSimulated = true;

	public bool SimulateMe {
		get { return _isSimulated; }
		set {
			_isSimulated = value;
			_camera.SetActive(value);
			if (value) {
				_viewportModel.layer = _viewportModelLayer;
			} else {
				_viewportModel.layer = 0;
			}
		}
	}

	void OnValidate () {
		_camera.SetActive(_isSimulated);
	}

	private Transform _transform;
	void Awake () {
		_viewportModelLayer = _viewportModel.layer;
		_transform = transform;
		_motor = GetComponent<CharacterMotor>();
	}

	public void SetTo (TimeEntityInfo info) {
		// if (!_isSimulated) {
			_transform.position = info.location;
			_transform.rotation = info.rotation;
			_viewPoint.rotation = info.viewRotation;
		// }
	}

	public TimeEntityInfo Simulate () {
		if (!_isSimulated) { return _myInfo; }
		_motor.Simulate();
		// _transform.position += Time.fixedDeltaTime * _transform.forward;
		_myInfo.location = _transform.position;
		_myInfo.rotation = _transform.rotation;
		_myInfo.viewRotation = _viewPoint.rotation;
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

	public void FreezeMotion () {
		GetComponent<Rigidbody>().isKinematic = true;
	}

	public void UnFreezeMotion () {
		GetComponent<Rigidbody>().isKinematic = false;
	}

	private List<TimeCollectibleEntity> _collected = new List<TimeCollectibleEntity>();
	public void AddCollection (TimeCollectibleEntity e) {
		_collected.Add(e);
	}

	public void UnCollectAll () {
		foreach (var e in _collected) {
			e.Uncollect();
		}
		_collected.Clear();
	}
}