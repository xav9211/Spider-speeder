using UnityEngine;

public class Item : MonoBehaviour {
    private Map map;

    public float? RestoreHealth;

	// Use this for initialization
	void Start () {
	    map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();

	    RestoreHealth = 100.0f;
	}
	
	// Update is called once per frame
	void Update () {
	    if (map.Player && map.Player.SelectedItem == this) {
	        GetComponent<SpriteRenderer>().color = Color.green;
	    } else {
	        GetComponent<SpriteRenderer>().color = Color.white;
	    }
	}

    void OnTriggerEnter2D() {
        if (map.Player) {
            map.Player.SelectedItem = this;
        }
    }

    void OnTriggerExit2D() {
        if (map.Player && map.Player.SelectedItem == this) {
            map.Player.SelectedItem = null;
        }
    }
}
