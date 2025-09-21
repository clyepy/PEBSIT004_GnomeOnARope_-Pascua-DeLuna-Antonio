using UnityEngine;
using System.Collections;

// BEGIN 2d_resettable
using UnityEngine.Events;

// Contains a UnityEvent that can be used to reset the state of this object.
public class Resettable : MonoBehaviour {

	// In the editor, connect this event to methods that should run when the
	// game resets.
	public UnityEvent onReset;

	// Called by the GameManager when the game resets.
public void Reset() {
    if (onReset != null) {
        onReset.Invoke();
    }
}

}
// END 2d_resettable