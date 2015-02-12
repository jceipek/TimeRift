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

public class TimeManager : MonoBehaviour
{

	[SerializeField]
	Text
		_timeText;
	[SerializeField]
	private int _maxTimeInitializer;
	private int _maxTime;
	private int _time;

	private int _maxTimeSimulated;

	private int[] _oldSegmentIndexAtTime;
	private TimeEntityInfo[] _oldTimeSegments;
	private int _oldTimeSegmentsFillAmount;

	private int[] _newSegmentIndexAtTime;
	private TimeEntityInfo[] _newTimeSegments;
	private int _newTimeSegmentsFillAmount;

	private TimeState _timeState;

	private List<TimeEntity> _registeredEntities = new List<TimeEntity> ();
	[SerializeField]
	private TimeEntity _newestSelf;

	public static TimeManager g;
	void Awake ()
	{
		if (g == null) {
			g = this;
		} else {
			Destroy (this);
		}
	}

	private string ReadableTime {
		get {
			int t = _maxTime - _time;
			int seconds = (int)(t/(1f/Time.fixedDeltaTime));
			int centiseconds = (int)(t - seconds*(1f/Time.fixedDeltaTime));
			return seconds.ToString().PadLeft(2, '0') + ":" + centiseconds.ToString().PadLeft(2,'0');
		}
	}

	void Start ()
	{
		_time = 0;

		_maxTimeSimulated = 0;
		_maxTime = (int)(1f/Time.fixedDeltaTime) * _maxTimeInitializer;
		int maxEntitiesPerFrame = 500;

		_oldSegmentIndexAtTime = new int[_maxTime + 1];
		_oldTimeSegments = new TimeEntityInfo[_maxTime * maxEntitiesPerFrame];
		_oldTimeSegmentsFillAmount = 0;

		_newSegmentIndexAtTime = new int[_maxTime + 1];
		_newTimeSegments = new TimeEntityInfo[_maxTime * maxEntitiesPerFrame];
		_newTimeSegmentsFillAmount = 0;

		_timeState = TimeState.Playing;

		_newestSelf.SimulateMe = true;
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

	void FixedUpdate ()
	{
		// Note: this must run before every other script in the scene!
		_timeText.text = ReadableTime;
		LoadFrame(_time);

		if (!_newestSelf.gameObject.activeSelf) {
			_newestSelf.gameObject.SetActive(true);
		}

		switch (_timeState) {
		case TimeState.Rewinding:
			_time--;
			if (_time < 0) {
				_time = 0;
			} else {
				_newTimeSegmentsFillAmount = _newSegmentIndexAtTime[_time];
			}
			break;
		case TimeState.Playing:
			SimulateFrame (_time);
			_time++;
			if (_time >= _maxTime) {
				_time = 0;

				var temp1 = _oldTimeSegments;
				_oldTimeSegments = _newTimeSegments;
				_newTimeSegments = temp1;
				var temp2 = _oldSegmentIndexAtTime;
				_oldSegmentIndexAtTime = _newSegmentIndexAtTime;
				_newSegmentIndexAtTime = temp2;
				_oldTimeSegmentsFillAmount = _newTimeSegmentsFillAmount;
				_newTimeSegmentsFillAmount = 0;


				if (_newestSelf != null) {
					GameObject newest = Instantiate(_newestSelf.gameObject,
										_newestSelf.gameObject.transform.position,
										_newestSelf.gameObject.transform.rotation) as GameObject;
					_newestSelf.SimulateMe = false;
					_newestSelf = newest.GetComponent<TimeEntity>();
					_newestSelf.SimulateMe = true;
					newest.SetActive(false);
				}
				// Events.g.Raise(new SpawnEvent());

				// _registeredEntities.Clear ();
			}
			break;
		}
		_maxTimeSimulated = Mathf.Max (_time, _maxTimeSimulated);
	}

	public void LoadFrame (int t)
	{
		if (t <= _maxTimeSimulated) { // Can't load history if it doesn't exist yet
			// Load history for this time
			int minSegIndex = _oldSegmentIndexAtTime [t];
			int maxSegIndex;
			if (t < _maxTimeSimulated) {
				maxSegIndex = _oldSegmentIndexAtTime [t + 1];
			} else {
				maxSegIndex = _oldTimeSegmentsFillAmount;
			}
			for (int i = minSegIndex; i < maxSegIndex; i++) {
				_oldTimeSegments [i].entityId.SetTo (_oldTimeSegments [i]);
			}
		}
	}

	public void LoadFrameFromNew (int t)
	{
		if (t <= _maxTimeSimulated) { // Can't load history if it doesn't exist yet
			// Load history for this time
			int minSegIndex = _newSegmentIndexAtTime [t];
			int maxSegIndex;
			if (t < _maxTimeSimulated) {
				maxSegIndex = _newSegmentIndexAtTime [t + 1];
			} else {
				maxSegIndex = _newTimeSegmentsFillAmount;
			}
			for (int i = minSegIndex; i < maxSegIndex; i++) {
				_newTimeSegments [i].entityId.SetTo (_newTimeSegments [i]);
			}
		}
	}

	public void SimulateFrame (int t)
	{
		// Record current entity locations
		int oldStartIndex = 0;
		int oldEndIndex = 0;
		int currIndex = 0;
		if (_maxTimeSimulated > 0) {
			currIndex = _newSegmentIndexAtTime [t];
			oldStartIndex = _oldSegmentIndexAtTime [t];
			if (t + 1 >= _oldSegmentIndexAtTime.Length) {
				Debug.Log (t + " " + _oldSegmentIndexAtTime.Length);
			}
			oldEndIndex = _oldSegmentIndexAtTime [t + 1];
		}

		for (int i = oldStartIndex; i < oldEndIndex; i++) {
			TimeEntityInfo segment = _oldTimeSegments [i];
			_newTimeSegments [currIndex] = segment;
			currIndex++;
			_newTimeSegmentsFillAmount++;
		}

		if (_newestSelf != null) {
			TimeEntityInfo segment = _newestSelf.Simulate();
			_newTimeSegments [currIndex] = segment;
			currIndex++;
			_newTimeSegmentsFillAmount++;
		}

		// for (int i = 0; i < _registeredEntities.Count; i++) {
		// 	TimeEntityInfo segment = _registeredEntities [i].Simulate ();
		// 	_newTimeSegments [currIndex] = segment;
		// 	currIndex++;
		// 	_newTimeSegmentsFillAmount++;
		// }
		_newSegmentIndexAtTime [t + 1] = currIndex;
	}

	public void RegisterEntity (TimeEntity e)
	{
		_registeredEntities.Add (e);
	}

}