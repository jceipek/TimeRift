using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum TimeState
{
	Playing,
	RewindingBecauseLoop,
	RewindingBecauseSeen,
	Rewinding
}

public class TimeManipulator : MonoBehaviour {

	class AvatarTravelInfo {
		public TimeEntity entity;
		public int framesSimulated;
		public TimeEntityInfo[] timeTravelFrames;

		public AvatarTravelInfo (TimeEntity entity, int maxFrameCount) {
			this.entity = entity;
			framesSimulated = 0;
			timeTravelFrames = new TimeEntityInfo[maxFrameCount];
		}
	}

	private List<AvatarTravelInfo> _avatarHistories = new List<AvatarTravelInfo>();
	private int _indexOfParadox = -1;
	private int _deathCount = 0;

	// Get volume based on http://answers.unity3d.com/questions/165729/editing-height-relative-to-audio-levels.html
	private float[] _volumeSamples = new float[1024]; // audio samples

	float GetVolume (AudioSource source) {
		source.GetOutputData(_volumeSamples, 0); // fill array with samples
		float sum = 0;
		int samples = _volumeSamples.Length;
		for (int i=0; i < samples; i++){
			sum += _volumeSamples[i]*_volumeSamples[i]; // sum squared samples
		}
		return Mathf.Sqrt(sum/samples); // rms = square root of average
	}

	[SerializeField]
	Light _alarmLight;
	[SerializeField]
	AnimationCurve _alarmLightIntensityCurve;

	[SerializeField]
	Text _timeText;
	[SerializeField]
	Text _deathText;
	[SerializeField]
	GameObject _paradoxTextObject;
	[SerializeField]
	Text _paradoxTextExpl;
	[SerializeField]
	int _maxSecondsInitializer;
	[SerializeField]
	TimeEntity _newestSelf;
	[SerializeField]
	private AudioClip _alarmSound;
	[SerializeField]
	private AudioClip[] _screamSounds;
	private AudioSource _alarmAudioSource;
	private AudioSource _screamAudioSource;
	private int _maxFrames;
	private static int _currFrame;
	public static int CurrFrame {
		get { return _currFrame; }
	}
	private int _maxFramesSimulated;
	private static TimeState _currTimeState;
	public static TimeState CurrTimeState {
		get { return _currTimeState; }
	}


	private string ReadableTime {
		get {
			int t = _maxFrames - _currFrame;
			int seconds = (int)(t*Time.fixedDeltaTime);
			int centiseconds = (int)(((t*Time.fixedDeltaTime) - seconds) * 100f);
			return seconds.ToString().PadLeft(2, '0') + ":" + centiseconds.ToString().PadLeft(2,'0');
		}
	}

	void Awake () {
		_alarmAudioSource = gameObject.AddComponent<AudioSource>();
		_screamAudioSource = gameObject.AddComponent<AudioSource>();
	}

	void Start () {
		_currTimeState = TimeState.Playing;
		_currFrame = 0;
		_maxFrames = (int)(1f/Time.fixedDeltaTime) * _maxSecondsInitializer;
		_maxFramesSimulated = 0;

		AvatarTravelInfo newestAvatarHistory = new AvatarTravelInfo(_newestSelf, _maxFrames);
		_avatarHistories.Add(newestAvatarHistory);

		_newestSelf.SimulateMe = true;
	}

	enum ParadoxCause {
		Seen,
		TooClose,
		Collection
	}

	private void InitiateCollectionParadox (CollectionParadox e) {
		for (int i = 0; i < _avatarHistories.Count; i++) {
			if (e.responsibleCharacter == _avatarHistories[i].entity) {
				InitiateParadox(ParadoxCause.Collection, i);
				break;
			}
		}
	}

	private void InitiateParadox (ParadoxCause cause, int characterIndex) {
		switch (cause) {
			case ParadoxCause.Seen:
				_paradoxTextExpl.text = "Your past self saw you!";
				break;
			case ParadoxCause.TooClose:
				_paradoxTextExpl.text = "Your past self heard you!";
				break;
			case ParadoxCause.Collection:
				_paradoxTextExpl.text = "You stopped your past self from collecting that!";
				break;
		}
		_screamAudioSource.clip = _screamSounds[Random.Range(0, _screamSounds.Length)];
		_screamAudioSource.Play();

		_alarmAudioSource.clip = _alarmSound;
		_alarmAudioSource.Play();
		_currTimeState = TimeState.RewindingBecauseSeen;

		for (int i = characterIndex; i < _avatarHistories.Count; i++) {
			_avatarHistories[i].entity.UnCollectAll();
		}

		_deathCount += _avatarHistories.Count - characterIndex - 1;

		_indexOfParadox = characterIndex;
	}

	void OnEnable () {
		Events.g.AddListener<CollectionParadox>(InitiateCollectionParadox);
	}

	void OnDisable () {
		Events.g.RemoveListener<CollectionParadox>(InitiateCollectionParadox);
	}

	float _separationToCatch = 1f;
	float _fovToCatch = 0.75f;
	void FixedUpdate () {

		_timeText.text = ReadableTime;
		_deathText.text = _deathCount.ToString();

		if (_currTimeState == TimeState.Playing) {
			for (int i = 0; i < _avatarHistories.Count - 1; i++) {
				var entityToTest = _avatarHistories[i].entity;
				if (_newestSelf == entityToTest) { continue; }
				Vector3 toOther = _newestSelf.EyeLocation - entityToTest.EyeLocation;
				if (toOther.sqrMagnitude <= _separationToCatch * _separationToCatch) {
					InitiateParadox(ParadoxCause.TooClose, i);
					break;
				}
				Debug.DrawLine(entityToTest.EyeLocation, entityToTest.EyeLocation + entityToTest.Forward, Color.red);
				float f = Vector3.Dot(entityToTest.Forward, toOther.normalized);
				if (f >= _fovToCatch) {
					Debug.DrawLine(_newestSelf.EyeLocation, entityToTest.EyeLocation, Color.blue);
					// Debug.Log("In FOV: "+f+"!");
					RaycastHit hitInfo;
					if (Physics.Raycast(entityToTest.EyeLocation, toOther.normalized, out hitInfo, distance: Mathf.Infinity)) {
						if (hitInfo.collider.gameObject.GetComponent<TimeEntity>() != null) {
							Debug.DrawLine(_newestSelf.EyeLocation, entityToTest.EyeLocation, Color.yellow);
							Debug.DrawLine(hitInfo.point, entityToTest.EyeLocation, Color.green);
							InitiateParadox(ParadoxCause.Seen, i);
							break;
						}
					}
				}
			}

			AvatarTravelInfo oldestHistory = _avatarHistories[_avatarHistories.Count - 1];
			if (_currFrame < _maxFrames) {

				for (int i = 0; i < _avatarHistories.Count - 1; i++) {
					_avatarHistories[i].entity.SetTo(_avatarHistories[i].timeTravelFrames[_currFrame]);
				}

				TimeEntityInfo segment = _newestSelf.Simulate();
				oldestHistory.timeTravelFrames[_currFrame] = segment;
				oldestHistory.framesSimulated = _currFrame + 1;

				_currFrame++;
			} else {

				GameObject newest = Instantiate(_newestSelf.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
				oldestHistory.entity = newest.GetComponent<TimeEntity>();
				oldestHistory.entity.SetTo(oldestHistory.timeTravelFrames[0]);
				oldestHistory.entity.SimulateMe = false;


				AvatarTravelInfo newestAvatarHistory = new AvatarTravelInfo(_newestSelf, _maxFrames);
				_avatarHistories.Add(newestAvatarHistory);

				_currTimeState = TimeState.RewindingBecauseLoop;

				_currFrame--;
			}
		}

		_maxFramesSimulated = (int)Mathf.Max(_currFrame, _maxFramesSimulated);
	}

	[SerializeField]
	int _rewindSpeed = 10;
	[SerializeField]
	int _paradoxRewindSpeed = 5;
	void Update ()
	{
		if (_currTimeState == TimeState.RewindingBecauseLoop) {

			if (_currFrame > 0) {
				_newestSelf.FreezeMotion();
				for (int i = 0; i < _avatarHistories.Count - 1; i++) {
					_avatarHistories[i].entity.SetTo(_avatarHistories[i].timeTravelFrames[_currFrame]);
				}

				_currFrame -= _rewindSpeed;
			} else {

				_currTimeState = TimeState.Playing;
				_currFrame = 0;
				_newestSelf.UnFreezeMotion();
			}
		}

		if (_currTimeState == TimeState.RewindingBecauseSeen) {

			if (_currFrame > 0) {
				_newestSelf.FreezeMotion();
				for (int i = 0; i < _avatarHistories.Count; i++) {
					_avatarHistories[i].entity.SetTo(_avatarHistories[i].timeTravelFrames[_currFrame]);
				}

				float volume = GetVolume(_alarmAudioSource);
				if (volume > 0.2f) {
					_paradoxTextObject.SetActive(true);
				} else {
					_paradoxTextObject.SetActive(false);
				}
				_alarmLight.intensity = _alarmLightIntensityCurve.Evaluate(volume);

				_currFrame -= _paradoxRewindSpeed;
			} else {
				if (_indexOfParadox < (_avatarHistories.Count - 1)) {
					_currFrame = _maxFramesSimulated - 1;

					// GameObject newest = Instantiate(_newestSelf.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
					_avatarHistories.RemoveAt(_avatarHistories.Count - 1);
					Destroy(_avatarHistories[_avatarHistories.Count - 1].entity.gameObject);
					AvatarTravelInfo oldestHistory = _avatarHistories[_avatarHistories.Count - 1];
					oldestHistory.entity = _newestSelf;
					oldestHistory.entity.SetTo(oldestHistory.timeTravelFrames[0]);
					// oldestHistory.entity.SimulateMe = false;

				} else {
					_currTimeState = TimeState.Playing;
					_currFrame = 0;
					_newestSelf.UnFreezeMotion();

					_paradoxTextObject.SetActive(false);
					_alarmLight.intensity = 0f;
					_alarmAudioSource.Stop();
				}
			}
		}


		// if (Input.GetKeyDown (KeyCode.R)) {
		// 	_currTimeState = TimeState.Rewinding;
		// }
		// if (Input.GetKeyUp (KeyCode.R)) {
		// 	_currTimeState = TimeState.Playing;
		// }
	}
}
