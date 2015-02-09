using UnityEngine;
using System.Collections;


public class TimeEntity : MonoBehaviour {

	private TimeEntityInfo _myInfo;

	private Transform _transform;
	void Awake () {
		_myInfo.entityId = this;
		_transform = transform;
	}

	void FixedUpdate () {
		if (!TimeManager.g.ShouldSimulate) { return; }
		_transform.position += Time.fixedDeltaTime * _transform.forward;

		_myInfo.location = _transform.position;
		_myInfo.rotation = _transform.rotation;
		TimeManager.g.RecordTimeEntityInfo(_myInfo);
	}

	public void SetTo (TimeEntityInfo info) {
		_transform.position = info.location;
		_transform.rotation = info.rotation;
	}

}