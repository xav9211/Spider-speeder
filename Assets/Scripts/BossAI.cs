using UnityEngine;
using System.Collections;

namespace Assets.Scripts {
	public class BossAI : EnemyAI {

		// added because unity's method OnCollisionEnter2d does not work properly
		// and in method OnCollisionStay2D velocity is changed because collision occured
		// lame
		Vector2 velocity = new Vector2(0f, 0f);

		void Start () {
			base.Start ();

			damage = 10F;
			speed = 0.2F;
			life = 2.0F;
			currentLife = life;

			rb.velocity = velocity;
		}

		void Update () {

		}

		void Die (DamageInfo dmgInfo){
			string colliderName = dmgInfo.Collider.name;
			if (colliderName.Contains ("Button")) {
				GameObject g = transform.FindChild (colliderName).gameObject;
				g.transform.position = new Vector2 (transform.position.x, transform.position.y);
				dmgInfo.Damage = 1.0f;
				base.Die (dmgInfo);
			}

		}

		void OnCollisionStay2D (Collision2D collision){
			Vector2 normal = Vector2.zero;
			foreach (var contact in collision.contacts){
				normal += contact.normal;
			}
			normal.Normalize();
			velocity = Vector2.Reflect (velocity, normal);
			rb.velocity = velocity;

			base.OnCollisionStay2D (collision);
		}

	}
}