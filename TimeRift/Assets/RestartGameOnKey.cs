using UnityEngine;
using System.Collections;

public class RestartGameOnKey : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift)) {
			Application.LoadLevel(0);
		}
	}
}
