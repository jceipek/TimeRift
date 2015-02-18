using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CollectibleDisplay : MonoBehaviour {

	[SerializeField]
	TimeCollectibleEntity[] _collectibles;
	[SerializeField]
	Text _collectedText;
	[SerializeField]
	int _winLevel = 2;

	void Update () {
		int collected = 0;
		for (int i = 0; i < _collectibles.Length; i++) {
			if (_collectibles[i].IsCollected) {
				collected++;
			}
		}
		_collectedText.text = collected.ToString()+" / " + _collectibles.Length.ToString();

		if (collected == _collectibles.Length) {
			Debug.Log("Load Win Screen!");
			Application.LoadLevel(_winLevel);
		}
	}
}
