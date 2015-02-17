using UnityEngine;

public class SpottedEvent: GameEvent {
	public GameObject target;
	public GameObject by;
	public SpottedEvent (GameObject target, GameObject by) {
		this.target = target;
		this.by = by;
	}
}