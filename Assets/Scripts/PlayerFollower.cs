using UnityEngine;
using System.Collections;

public class PlayerFollower : MonoBehaviour {
    private Map map;
    private Vector3 lastPosition;

    void Start() {
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
    }

	void Update () {
	    if (map.Player != null) {
	        lastPosition = map.Player.transform.position;
	    } else if (Input.GetKeyDown(KeyCode.Joystick1Button0)
                   || Input.GetKeyDown(KeyCode.Joystick2Button0)) {
            map.Regenerate(1);
	    }

	    transform.position = new Vector3(lastPosition.x, lastPosition.y, transform.position.z);
	}
}
