using UnityEngine;

public class Exit : MonoBehaviour {
	// Use this for initialization
	void Start () {
	}

    private void ExitMap() {
        var map = GameObject.Find("Map").GetComponent<Map>();
        map.Regenerate(map.Level + 1);
    }

    // Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.F1)) {
            ExitMap();
        }
	}

    const float MAX_TRIGGER_SPEED_SQ = 4.0f;

    void OnTriggerStay2D(Collider2D collider) {
        if (collider.tag == "SpiderBody"
                && collider.GetComponent<Rigidbody2D>().velocity.SqrMagnitude() < MAX_TRIGGER_SPEED_SQ) {
            ExitMap();
        }
    }
}
