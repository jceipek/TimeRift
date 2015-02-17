using UnityEngine;
using System.Collections;

public class PlayAudioInCorrectDir : MonoBehaviour {

	[SerializeField]
	float _rewindSpeed = 10f;
	[SerializeField]
	float _paradoxRewindSpeed = 5f;
	AudioSource _audio;

	float _initialPitch;

	void Awake () {
		_audio = GetComponent<AudioSource>();
		_initialPitch = _audio.pitch;
	}

	// Update is called once per frame
	void Update () {
		if (TimeManipulator.CurrTimeState == TimeState.RewindingBecauseLoop) {
			_audio.pitch = -_initialPitch * _rewindSpeed;
		} else if (TimeManipulator.CurrTimeState == TimeState.RewindingBecauseSeen) {
			_audio.pitch = -_initialPitch * _paradoxRewindSpeed;
		} else {
			_audio.pitch = _initialPitch;
		}
	}
}
