using System;
using UnityEngine;

namespace Assets.Scripts {
	public enum EnemyType {
		Regular,
		Boss
	}

    public class EnemyAI : MonoBehaviour
    {
        Transform lifeBar;
        private Map.Map map;
		protected EnemyType enemyType = EnemyType.Regular;
		protected float damage = 10F;
		protected float speed = 0.2F;
		protected float life = 10F;
		protected float currentLife;
		protected string damageSound;
		protected Rigidbody2D rb;
        Color green = new Color(0.0f, 0.7f, 0.0f, 0.9f);
        Color yellow = new Color(0.7f, 0.7f, 0.0f, 0.9f);
        Color red = new Color(0.7f, 0.0f, 0.0f, 0.9f);

        // Use this for initialization
        protected void Start()
        {
            lifeBar = transform.FindChild("LifeBar");
            map = FindObjectOfType<Map.Map>();
			rb = GetComponent<Rigidbody2D> ();
        }

        public float createMonster(int level,
                                   System.Random rng){
            var renderer = (SpriteRenderer)gameObject.GetComponent ("SpriteRenderer");

            var val = rng.Next(100);

			// Yet another fucking magic
			const float YAFM = 0.85f;
			speed = 0.5f + (YAFM * level * val) / 100.0F;
			damage = 10 + (YAFM * level * val) / 15.0F;
			life = 100 + (YAFM * level * val);

            if (val < 20){
                renderer.sprite = Resources.Load<Sprite> ("enemies/enemy2");
                damageSound = "enemy2Damage";
            }
            else if (val >= 20 && val < 40) {
                renderer.sprite = Resources.Load<Sprite> ("enemies/creature1");
                damageSound = "creature1Damage";
            } else if (val >= 40 && val < 60) {
                renderer.sprite = Resources.Load<Sprite> ("enemies/enemy3");
                damageSound = "enemy3Damage";
            } else if (val >= 60 && val < 80) {
                renderer.sprite = Resources.Load<Sprite> ("enemies/Przechwytywanie");
                damageSound = "przechwytywanieDamage";
            } else if (val >= 80 && val < 100) {
                renderer.sprite = Resources.Load<Sprite> ("enemies/putin");
                damageSound = "putinDamage";
            }

            currentLife = life;

            const float FUCKING_MAGIC = 0.7f;
            float newScale =  FUCKING_MAGIC / renderer.sprite.bounds.extents.x;
            transform.localScale = new Vector3(newScale, newScale, 1);

            Transform lifeBar = transform.GetChild(0);
            lifeBar.position = transform.localPosition + new Vector3(0.0f, FUCKING_MAGIC, 0.0f);
            lifeBar.localScale = new Vector3(0.5f/newScale, 0.1f/newScale, 1);
            lifeBar.GetComponentInChildren<SpriteRenderer>().color = green;
            
            var collider = gameObject.GetComponent <CircleCollider2D>();
            collider.radius = renderer.sprite.bounds.extents.x;

            return damage + life;
        }

        // Update is called once per frame
        void Update()
        {
            PlayerControl spider = map.Player;
            if (spider != null && Vector3.Distance(spider.transform.position, transform.position) < 10)
            {
                rb.velocity = (speed * Vector3.Normalize(spider.transform.position - transform.position));
            } else
            {
                rb.velocity = Vector2.zero;
            }

            if (currentLife > 0.6 * life) {
                lifeBar.GetComponentInChildren<SpriteRenderer>().color = green;
            } else if (currentLife > 0.3 * life) {
                lifeBar.GetComponentInChildren<SpriteRenderer>().color = yellow;
            } else {
                lifeBar.GetComponentInChildren<SpriteRenderer>().color = red;
            }
        }

        protected void Die(DamageInfo dmgInfo)
        {
            float oldLife = currentLife;
            currentLife -= dmgInfo.Damage;
            lifeBar.transform.localScale = new Vector3(currentLife / life * 0.5f / transform.localScale.x, 
                                                       lifeBar.transform.localScale.y, 
                                                       lifeBar.transform.localScale.z);

            if (currentLife < 0.1f) {
                ExplosionFactory.Create(transform.position, 1.0f);
                AudioUtils.Play("ZombieDeath", transform.position);

                BloodFactory.SplatFromDamageInfo(dmgInfo);
                GameStatistics.AddKill(dmgInfo.PlayerNumber, this);

                Destroy(this.gameObject);
				switch (enemyType) {
				case EnemyType.Regular:
					Item.CreateRandom (transform.position);
					break;
				case EnemyType.Boss:
					var tileset = (GameObject.Find ("Map")).GetComponent<Assets.Scripts.Map.Map>().tilesets;
				    GameObject stairs = (GameObject) Instantiate(tileset[Assets.Scripts.Map.Tile.Type.Exit][0],
                                                                 transform.position,
                                                                 Quaternion.identity);
				    stairs.transform.SetParent(map.transform);
					break;
				}
                
            }
            else {
                AudioUtils.Play(damageSound, transform.position, 10.0f);
            }
        
        }

        void OnTriggerEnter(Collider col)
        {
//        print(col.gameObject.name);
            if (col.gameObject.tag == "Player")
            {
                col.gameObject.SendMessage("Die", damage);
            }
        }

        protected void OnCollisionStay2D(Collision2D collision)
        {
//        print(collision.gameObject.name);
            if(collision.gameObject.tag == "Player")
            {
                AudioUtils.Play("ZombieAttack", transform.position);
                map.Player.SendMessage("Die", damage);
            }
        }
    }
}