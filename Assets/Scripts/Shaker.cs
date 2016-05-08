using UnityEngine;

public class Shaker : MonoBehaviour {
    private new Camera camera;
    private float shake = 0.0f;
    private float shakeTime = 1.0f;
    private float shakeAmount = 0.7f;
 
	// Use this for initialization
	void Start () {
	    camera = GetComponentInParent<Camera>();
	}

    public void Shake(float factor) {
        shakeTime = factor;
        shake = shakeTime;
        shakeAmount = factor;
    }
	
	// Update is called once per frame
	void Update () {
        if (shake > 0.0f) {
            camera.transform.localPosition += Random.insideUnitSphere * (shake / shakeTime) * shakeAmount;
            shake -= Time.deltaTime;
        } else {
            shake = 0.0f;
        }
    }
}
