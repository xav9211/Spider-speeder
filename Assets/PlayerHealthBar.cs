using UnityEngine;
using System.Collections;

public class PlayerHealthBar : MonoBehaviour {
    private Map map;
    private RectTransform rectTransform;

	// Use this for initialization
	void Start () {
	    map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
	    rectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		float currentPlayerHealth = map.Player ? map.Player.Health : 0.0F;
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentPlayerHealth);
	}
}
