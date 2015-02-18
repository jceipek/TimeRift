using UnityEngine;
using System.Collections;
using InControl;

public class StartOnClick : MonoBehaviour {

	[SerializeField]
	int _levelToLoad;

	void Update () {
		if (Input.anyKey) {
			Application.LoadLevel(_levelToLoad);
			Debug.Log("Load Level!");
			Destroy(gameObject);
		}
	}

}
