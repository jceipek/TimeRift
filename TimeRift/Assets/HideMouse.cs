using UnityEngine;
using System.Collections;

public class HideMouse : MonoBehaviour {
	void Start () {
		if (!Application.isEditor) {
			Screen.lockCursor = true;
		}
	}
}
