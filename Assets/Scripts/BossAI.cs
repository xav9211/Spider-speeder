using UnityEngine;
using System.Collections;

namespace Assets.Scripts {
	public class BossAI : EnemyAI {

		System.Random rng;

		void Start () {
			base.Start ();
			enemyType = EnemyType.Boss;
			damageSound = "enemy2Damage";
			rb.velocity = new Vector2(1.0f, 1.0f) * speed;
		}

		public void initialize (int level, System.Random rngg){
			rng = rngg;
			speed = rng.Next (2 * level + 2, 3 * level + 1);
			damage = rng.Next (20 * level, 30 * level);
			life = 2.0F * level;
			currentLife = life;

			CircleCollider2D bossColider = transform.GetComponentsInParent<CircleCollider2D> ()[0];
			for (int i=0; i<life; i++){
				GameObject buttonObj = (GameObject)Instantiate(Resources.Load<Object>("Button"),
					transform.position,
					Quaternion.identity);
				buttonObj.name = "Button" + i;
				float angle = i / life * 360.0f;

				Transform btnTransform = buttonObj.transform;
				btnTransform.parent = transform;
				btnTransform.localScale = btnTransform.lossyScale;

				Vector2 pos = new Vector2 (Mathf.Cos((angle + 90.0f) * Mathf.Deg2Rad), 
										   Mathf.Sin((angle + 90.0f) * Mathf.Deg2Rad));
				pos.Normalize ();
				pos *= (bossColider.radius);
				btnTransform.localPosition = pos;
				btnTransform.localEulerAngles = new Vector3 (0, 0, angle);
			}
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