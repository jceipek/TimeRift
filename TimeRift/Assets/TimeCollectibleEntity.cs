using UnityEngine;
using System.Collections;

public class TimeCollectibleEntity : MonoBehaviour {

	[SerializeField]
	GameObject _aesthetics;
	[SerializeField]
	AudioClip _collectionSound;
	[SerializeField]
	float _rotationSpeed = 1f;

	int _collectionTime = -1;

	void Update () {
		if (_collectionTime == -1 || _collectionTime > TimeManipulator.CurrFrame) {
			if (!_aesthetics.activeSelf) {
				_aesthetics.SetActive(true);
			}
		}

		_aesthetics.transform.rotation = Quaternion.Euler(0f,_rotationSpeed * TimeManipulator.CurrFrame, 0f);
	}

	void OnTriggerEnter (Collider other) {
		TimeEntity e = other.gameObject.GetComponent<TimeEntity>();
		if (e == null || TimeManipulator.CurrTimeState != TimeState.Playing) { return; }
		if (_aesthetics.activeSelf) {
			if (_collectionTime == -1) {
				e.AddCollection(this);
				_collectionTime = TimeManipulator.CurrFrame;
			} else if (_collectionTime != TimeManipulator.CurrFrame) {
				Debug.Log(TimeManipulator.CurrFrame + " but expected " + _collectionTime);
				Events.g.Raise(new CollectionParadox (e));
				Debug.Log("Collection Paradox!");
			}
			_aesthetics.SetActive(false);
			AudioSource.PlayClipAtPoint(_collectionSound, transform.position);
		}
	}

	public bool IsCollected {
		get { return _collectionTime != -1 && _collectionTime <= TimeManipulator.CurrFrame; }
	}

	public void Uncollect () {
		_collectionTime = -1;
	}
}
