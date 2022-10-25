using UnityEngine;

public class Timer {

	public static bool TimerReachedZero(ref float _input)
	{
		_input -= Time.deltaTime;
		return _input <= 0f;
	}
}
