using UnityEngine;
using System.Collections;

public abstract class TouchMonoBehaviour : MonoBehaviour, ITouchInputInterface {

	abstract public void Tap (string name);

	abstract public void DoubleTap (string name);

	abstract public void LongTouch (string name);
}
