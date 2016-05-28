using UnityEngine;
using System.Collections;

namespace Assets.Scripts {
	public class BossAI : EnemyAI {

		System.Random rng;

		void Start () {
			base.Start ();

			enemyType = EnemyType.Boss;
			damage = 25F;
			speed = 4.0F;
			life = 2.0F;
			currentLife = life;
			damageSound = "enemy2Damage";
			rb.velocity = new Vector2(1.0f, 1.0f) * speed;
			rng = new System.Random ((int)(damage * speed * life));
		}

		void Update () {

		}

		void Die (DamageInfo dmgInfo){
			string colliderName = dmgInfo.Collider.name;
			if (colliderName.Contains ("Button")) {
				AudioUtils.Play("bossButton", transform.position);
				GameObject g = transform.FindChild (colliderName).gameObject;
				g.transform.position = new Vector2 (transform.position.x, transform.position.y);
				dmgInfo.Damage = 1.0f;
				base.Die (dmgInfo);
			}

		}

		public void OnCollisionStay2D (Collision2D collision){
			Vector2 normal = Vector2.zero;
			foreach (var contact in collision.contacts){
				normal += contact.normal;
			}
			normal.Normalize();
			/* crazy random movement */
			Vector2 vel = new Vector2 ((float)(rng.NextDouble()*2 - 1), (float)(rng.NextDouble()*2 - 1));
			vel.Normalize ();
			vel = new Vector2 (vel.x * Mathf.Sign(normal.x), vel.y * Mathf.Sign(normal.y));
			vel.Normalize ();
			vel *= speed;
			rb.velocity = vel;

			base.OnCollisionStay2D (collision);
		}

	}
}