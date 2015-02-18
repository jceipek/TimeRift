using UnityEngine;
using System.Collections;

public class SetToCurrentScreenRes : MonoBehaviour {

	static SetToCurrentScreenRes g;

	void Awake () {
		DontDestroyOnLoad(gameObject);
		if (g == null) {
			g = this;
			ForceDefaultScreenRes();
		} else {
			Destroy(gameObject);
		}
	}

	void ForceDefaultScreenRes () {
		if (Application.isEditor) {
			Debug.Log("Will set sceen res to default in build!");
			return;
		}
		Resolution res = Screen.currentResolution;
		Screen.SetResolution(res.width, res.height, true, res.refreshRate);
	}
}
