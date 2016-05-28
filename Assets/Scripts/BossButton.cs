using UnityEngine;
using System.Collections;

public class BossButton : MonoBehaviour {

	private Assets.Scripts.BossAI bossScript;
	// Use this for initialization
	void Start () {
		bossScript = transform.parent.GetComponent<Assets.Scripts.BossAI> ();
	}
	
	// Update is called once per frame
	void Update () {}

	void OnCollisionStay2D (Collision2D collision){
		bossScript.OnCollisionStay2D (collision);
	}

}
