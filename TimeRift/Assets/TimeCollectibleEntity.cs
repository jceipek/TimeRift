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
		if (_collectionTime != -1 && _collectionTime > TimeManipulator.CurrFrame) {
			if (!_aesthetics.activeSelf) {
				_aesthetics.SetActive(true);
			}
		}

		_aesthetics.transform.rotation = Quaternion.Euler(0f,_rotationSpeed * TimeManipulator.CurrFrame, 0f);
	}

	void OnTriggerEnter () {
		if (_aesthetics.activeSelf) {
			if (_collectionTime == -1) {
				_collectionTime = TimeManipulator.CurrFrame;
			} else {
				Debug.Log("Collection Paradox!");
			}
			_aesthetics.SetActive(false);
			AudioSource.PlayClipAtPoint(_collectionSound, transform.position);
		}
	}
}
