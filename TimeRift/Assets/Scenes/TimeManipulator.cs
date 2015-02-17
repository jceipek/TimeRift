using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum TimeState
{
	Playing,
	Rewinding
}

public struct TimeEntityInfo
{
	public TimeEntity entityId;
	public Vector3 location;
	public Quaternion rotation;

	public override string ToString ()
	{
		if (entityId == null) {
			return "EID: null";
		}
		return ("EID: " + entityId.gameObject.name);
	}
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

	[SerializeField]
	Text _timeText;
	[SerializeField]
	int _maxSecondsInitializer;
	[SerializeField]
	TimeEntity _newestSelf;
	private int _maxFrames;
	private int _currFrame;
	private int _maxFramesSimulated;
	private TimeState _timeState;


	private string ReadableTime {
		get {
			int t = _maxFrames - _currFrame;
			int seconds = (int)(t/(1f/Time.fixedDeltaTime));
			int centiseconds = (int)(t - seconds*(1f/Time.fixedDeltaTime));
			return seconds.ToString().PadLeft(2, '0') + ":" + centiseconds.ToString().PadLeft(2,'0');
		}
	}

	// Use this for initialization
	void Start () {
		_timeState = TimeState.Playing;
		_currFrame = 0;
		_maxFrames = (int)(1f/Time.fixedDeltaTime) * _maxSecondsInitializer;
		_maxFramesSimulated = 0;

		AvatarTravelInfo newestAvatarHistory = new AvatarTravelInfo(_newestSelf, _maxFrames);
		_avatarHistories.Add(newestAvatarHistory);

		_newestSelf.SimulateMe = true;
	}

	float _separationToCatch = 1f;
	float _fovToCatch = 0.75f;
	void FixedUpdate () {

		_timeText.text = ReadableTime;

		if (_timeState == TimeState.Playing) {
			for (int i = 0; i < _avatarHistories.Count - 1; i++) {
				var entityToTest = _avatarHistories[i].entity;
				if (_newestSelf == entityToTest) { continue; }
				Vector3 toOther = _newestSelf.EyeLocation - entityToTest.EyeLocation;
				if (toOther.sqrMagnitude <= _separationToCatch * _separationToCatch) {
					Debug.Log("Too Close!");
				}
				Debug.DrawLine(entityToTest.EyeLocation, entityToTest.EyeLocation + entityToTest.Forward, Color.red);
				float f = Vector3.Dot(entityToTest.Forward, toOther.normalized);
				if (f >= _fovToCatch) {
					Debug.DrawLine(_newestSelf.EyeLocation, entityToTest.EyeLocation, Color.blue);
					Debug.Log("In FOV: "+f+"!");
					RaycastHit hitInfo;
					if (Physics.Raycast(entityToTest.EyeLocation, toOther.normalized, out hitInfo, distance: Mathf.Infinity)) {
						if (hitInfo.collider.gameObject.GetComponent<TimeEntity>() != null) {
							Debug.DrawLine(_newestSelf.EyeLocation, entityToTest.EyeLocation, Color.yellow);
							Debug.DrawLine(hitInfo.point, entityToTest.EyeLocation, Color.green);
							Debug.Log("Spotted!");
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

				GameObject newest = Instantiate(_newestSelf.gameObject,
									oldestHistory.timeTravelFrames[0].location,
									oldestHistory.timeTravelFrames[0].rotation) as GameObject;
				oldestHistory.entity = newest.GetComponent<TimeEntity>();
				oldestHistory.entity.SimulateMe = false;


				AvatarTravelInfo newestAvatarHistory = new AvatarTravelInfo(_newestSelf, _maxFrames);
				_avatarHistories.Add(newestAvatarHistory);

				_currFrame = 0;
			}
		}

		if (_timeState == TimeState.Rewinding) {
			if (_currFrame > 0) {

				for (int i = 0; i < _avatarHistories.Count; i++) {
					_avatarHistories[i].entity.SetTo(_avatarHistories[i].timeTravelFrames[_currFrame]);
				}

				_currFrame--;
			}
		}

		_maxFramesSimulated = (int)Mathf.Max(_currFrame, _maxFramesSimulated);
	}

	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.R)) {
			_timeState = TimeState.Rewinding;
		}
		if (Input.GetKeyUp (KeyCode.R)) {
			_timeState = TimeState.Playing;
		}
	}
}
