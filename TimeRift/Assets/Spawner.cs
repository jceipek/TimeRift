using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	[SerializeField]
	GameObject _avatarPrefab;

	TimeEntity _currTimeEntity;

	void OnEnable () {
		Events.g.AddListener<SpawnEvent>(Spawn);
	}

	void OnDisable () {
		Events.g.RemoveListener<SpawnEvent>(Spawn);
	}

	void Spawn (SpawnEvent e) {
		Debug.Log("SPAWN");
		// Instantiate(_avatarPrefab)
	}

}
