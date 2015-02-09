using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public struct TimeEntityInfo {
	public TimeEntity entityId;
	public Vector3 location;
	public Quaternion rotation;
}

public class HistorySegment {
	private TimeEntityInfo[] history;
	private int segmentsUsed;
	public HistorySegment (int maxSegements) {
		history = new TimeEntityInfo[maxSegements];
		segmentsUsed = 0;
	}
	public void AddSegment (TimeEntityInfo segment) {
		if (segmentsUsed >= history.Length) {
			Debug.LogError("Ran out of space in this segment!");
			return;
		}
		history[segmentsUsed] = segment;
		segmentsUsed++;
	}
	public TimeEntityInfo GetSegment (int index) {
		return history[index];
	}
	public void OverwriteSegment (TimeEntityInfo segment, int index) {
		history[index] = segment;
	}

	public void Clear () {
		segmentsUsed = 0;
	}
}

public enum TimeState {
	Playing,
	Rewinding
}

public class TimeManager : MonoBehaviour {

	private int _iteration = 0;
	private int _time = 0;
	public static TimeManager g;
	private int _oldSegmentCounter = 0;
	private int _newSegmentCounter = 0;
	private HistorySegment _oldHistory = new HistorySegment(100000);
	private HistorySegment _newHistory = new HistorySegment(100000);
	private TimeState _timeState;
	public bool ShouldSimulate {
		get { return _timeState == TimeState.Playing; }
	}


	[SerializeField]
	Text _timeText;

	private string ReadableTime {
		get {
			return _time.ToString();
		}
	}

	int _timeLoopDuration;
	private int[] _timeLine;

	void Awake () {
		if (g == null) {
			g = this;
		} else {
			Destroy (this);
		}
	}

	void Start () {
		int maxTimeLoop = 10000;
		_timeLoopDuration = maxTimeLoop;
		_timeLine = new int[maxTimeLoop];
		_timeState = TimeState.Playing;
	}

	void UpdateAllStoredEntities (int time) {
		if (_iteration == 0) {
			return;
		}
		for (int i = 0; i < _timeLine[time]; i++) {
			TimeEntityInfo segment = _oldHistory.GetSegment(_oldSegmentCounter);
			// Debug.Log(segment.entityId);
			segment.entityId.SetTo(segment);
			_oldSegmentCounter++;
		}
	}

	void UpdateAllStoredEntitiesRev (int time) {
		for (int i = 0; i < _timeLine[time]; i++) {
			TimeEntityInfo segment = _oldHistory.GetSegment(_oldSegmentCounter);
			// Debug.Log(segment.entityId);
			segment.entityId.SetTo(segment);
			_oldSegmentCounter--;
		}
	}

	int _segmentIndexAtFrameStart;
	void FixedUpdate () {
		// Note: this must run before every other script in the scene!
		_timeText.text = ReadableTime;
		switch (_timeState) {
			case TimeState.Rewinding:
				PlayBackwdByOneFrame();
			break;
			case TimeState.Playing:
				PlayFwdByOneFrame();
			break;
		}
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.R)) {
			_timeState = TimeState.Rewinding;
		}
		if (Input.GetKeyUp(KeyCode.R)) {
			_timeState = TimeState.Playing;
		}
	}

	bool PlayBackwdByOneFrame () {
		if (_time > 0) {
			_segmentIndexAtFrameStart = _oldSegmentCounter;
			UpdateAllStoredEntitiesRev(_time);
			_time -= 1;
			return false;
		}
		return true;
	}

	void PlayFwdByOneFrame () {
		_segmentIndexAtFrameStart = _oldSegmentCounter;
		UpdateAllStoredEntities(_time);
		_time += 1;

		if (_time > _timeLoopDuration) {
			var temp = _oldHistory;
			_oldHistory = _newHistory;
			_newHistory = temp;
			_newHistory.Clear();
			_time = 0;
			_oldSegmentCounter = 0;
			_newSegmentCounter = 0;
			_iteration++;
		}
	}

	public void RecordTimeEntityInfo (TimeEntityInfo info) {
		if (_timeState != TimeState.Playing) {
			return; // Shouldn't record while rewinding!
		}
		int t = _time+1;
		for (int i = 0; i < _timeLine[t]; i++) {
			TimeEntityInfo segment = _oldHistory.GetSegment(_segmentIndexAtFrameStart+i);
			if (info.entityId == segment.entityId) {
				_newHistory.AddSegment(info);
				_newSegmentCounter++;
				return;
			}
		}
		_newHistory.AddSegment(info);
		_newSegmentCounter++;
		_timeLine[t]++;
	}

}