using UnityEngine;
using System.Collections;

public class BulletAI : MonoBehaviour {

	Vector2 dir;
	float speed;
	float damage;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		GetComponent<Rigidbody2D> ().velocity = (new Vector3 (dir.x, dir.y)) * speed;
	}

	public void init(Vector2 dir, float speed, float damage)
	{
		this.dir = dir;
		this.speed = speed;
		this.damage = damage;
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.name == "SpiderBody(Clone)") {
			collider.gameObject.SendMessage("Die", damage);
			Destroy (this.gameObject);
		}
		if (collider.gameObject.name == "Wall(Clone)") {
			Destroy (this.gameObject);
		}
	}
		

}
