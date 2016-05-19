using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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
	    float previousPlayerHealth = rectTransform.rect.width;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentPlayerHealth);

	    if (previousPlayerHealth > currentPlayerHealth) {
	        GameObject drops = (GameObject) Instantiate(Resources.Load("HealthDrops"));
	        drops.transform.SetParent(transform);
	        float scale = 1.0f;
            drops.transform.localScale = new Vector3(scale, scale, scale);
            drops.transform.localPosition = new Vector3(rectTransform.rect.xMax,
                                                        rectTransform.rect.center.y,
                                                        transform.position.z);
	    }
	}
}
