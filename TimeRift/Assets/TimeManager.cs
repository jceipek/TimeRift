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

	private int _time;
	private int _maxTime;
	private int _maxTimeSimulated;

	private int[] _oldSegmentIndexAtTime;
	private TimeEntityInfo[] _oldTimeSegments;
	private int _oldTimeSegmentsFillAmount;

	private int[] _newSegmentIndexAtTime;
	private TimeEntityInfo[] _newTimeSegments;
	private int _newTimeSegmentsFillAmount;

	private TimeState _timeState;

	private List<TimeEntity> _registeredEntities = new List<TimeEntity> ();

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
			return _time.ToString ();
		}
	}

	void Start ()
	{
		_time = 0;

		_maxTimeSimulated = 0;
		_maxTime = 30*30;
		int maxEntitiesPerFrame = 500;

		_oldSegmentIndexAtTime = new int[_maxTime + 1];
		_oldTimeSegments = new TimeEntityInfo[_maxTime * maxEntitiesPerFrame];
		_oldTimeSegmentsFillAmount = 0;

		_newSegmentIndexAtTime = new int[_maxTime + 1];
		_newTimeSegments = new TimeEntityInfo[_maxTime * maxEntitiesPerFrame];
		_newTimeSegmentsFillAmount = 0;

		_timeState = TimeState.Playing;
	}

	// void Update ()
	// {
	// 	if (Input.GetKeyDown (KeyCode.R)) {
	// 		_timeState = TimeState.Rewinding;
	// 	}
	// 	if (Input.GetKeyUp (KeyCode.R)) {
	// 		_timeState = TimeState.Playing;
	// 	}
	// }

	void FixedUpdate ()
	{
		// Note: this must run before every other script in the scene!
		_timeText.text = ReadableTime;
		LoadFrame(_time);

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

				_registeredEntities.Clear ();
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

		for (int i = 0; i < _registeredEntities.Count; i++) {
			TimeEntityInfo segment = _registeredEntities [i].Simulate ();
			_newTimeSegments [currIndex] = segment;
			currIndex++;
			_newTimeSegmentsFillAmount++;
		}
		_newSegmentIndexAtTime [t + 1] = currIndex;
	}

	public void RegisterEntity (TimeEntity e)
	{
		_registeredEntities.Add (e);
	}

}