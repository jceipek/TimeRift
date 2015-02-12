// Based on http://www.willrmiller.com/?p=87

using System.Collections;
using System.Collections.Generic;

public class GameEvent
{
}

public class Events
{
	static Events _g = null;
	public static Events g {
		get {
			if (_g == null) {
				_g = new Events ();
			}

			return _g;
		}
	}

	public delegate void EventDelegate<T> (T e) where T : GameEvent;
	private delegate void EventDelegate (GameEvent e);

	private Dictionary<System.Type, EventDelegate> _delegates = new Dictionary<System.Type, EventDelegate> ();
	private Dictionary<System.Delegate, EventDelegate> _delegateLookup = new Dictionary<System.Delegate, EventDelegate> ();

	public void AddListener<T> (EventDelegate<T> del) where T : GameEvent {
		// Early-out if we've already registered this delegate
		if (_delegateLookup.ContainsKey (del))
			return;

		// Create a new non-generic delegate which calls our generic one.
		// This is the delegate we actually invoke.
		EventDelegate internalDelegate = (e) => del ((T)e);
		_delegateLookup [del] = internalDelegate;

		EventDelegate tempDel;
		if (_delegates.TryGetValue (typeof(T), out tempDel)) {
			_delegates [typeof(T)] = tempDel += internalDelegate;
		} else {
			_delegates [typeof(T)] = internalDelegate;
		}
	}

	public void RemoveListener<T> (EventDelegate<T> del) where T : GameEvent {
		EventDelegate internalDelegate;
		if (_delegateLookup.TryGetValue (del, out internalDelegate)) {
			EventDelegate tempDel;
			if (_delegates.TryGetValue (typeof(T), out tempDel)) {
				tempDel -= internalDelegate;
				if (tempDel == null) {
					_delegates.Remove (typeof(T));
				} else {
					_delegates [typeof(T)] = tempDel;
				}
			}

			_delegateLookup.Remove (del);
		}
	}

	public void Raise (GameEvent e) {
		EventDelegate del;
		if (_delegates.TryGetValue (e.GetType (), out del)) {
			del.Invoke (e);
		}
	}
}