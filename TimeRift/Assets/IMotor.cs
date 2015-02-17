using UnityEngine;
using System.Collections;

public interface IMotor {
	bool Enabled { get; set; }
	void Move (Vector2 inputVector);
	void Look (Vector2 lookInputVector);
	void Jump ();
}
