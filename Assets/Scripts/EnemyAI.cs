using System;
using UnityEngine;

namespace Assets.Scripts {
    public class EnemyAI : MonoBehaviour
    {
        Transform lifeBar;
        private Map.Map map;

        float damage = 10F;
        float speed = 0.2F;
        float life = 10F;
        float currentLife;
        string damageSound;
        Color green = new Color(0.0f, 0.7f, 0.0f, 0.9f);
        Color yellow = new Color(0.7f, 0.7f, 0.0f, 0.9f);
        Color red = new Color(0.7f, 0.0f, 0.0f, 0.9f);
        // Use this for initialization
        void Start()
        {
            lifeBar = transform.FindChild("LifeBar");
            map = FindObjectOfType<Map.Map>();

        }

        public float createMonster(int level,
                                   System.Random rng){
            var renderer = (SpriteRenderer)gameObject.GetComponent ("SpriteRenderer");

            var val = rng.Next(100*level);
            if (val < 20){
                renderer.sprite = Resources.Load<Sprite> ("enemies/enemy2");
                speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 50F);
                damage = Math.Max (1, rng.Next(val, 100*level) / 10F);
                life = Math.Max (1, rng.Next(val, 100*level) * 3F);
                damageSound = "enemy2Damage";
            }
            else if (val >= 20 && val < 40) {
                renderer.sprite = Resources.Load<Sprite> ("enemies/creature1");
                speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 50F);
                damage = Math.Max (1, rng.Next(val, 100*level) / 10F);
                life = Math.Max (1, rng.Next(val, 100*level) * 3F);
                damageSound = "creature1Damage";
            } else if (val >= 40 && val < 60) {
                renderer.sprite = Resources.Load<Sprite> ("enemies/enemy3");
                speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 50F);
                damage = Math.Max (1, rng.Next(val, 100*level) / 10F);
                life = Math.Max (1, rng.Next(val, 100*level) * 3F);
                damageSound = "enemy3Damage";
            } else if (val >= 60 && val < 80) {
                renderer.sprite = Resources.Load<Sprite> ("enemies/Przechwytywanie");
                speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 50F);
                damage = Math.Max (1, rng.Next(val, 100*level) / 10F);
                life = Math.Max (1, rng.Next(val, 100*level) * 3F);
                damageSound = "przechwytywanieDamage";
            } else if (val >= 80 && val < 100) {
                renderer.sprite = Resources.Load<Sprite> ("enemies/putin");
                speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 30F);
                damage = Math.Max (1, rng.Next(val, 100*level) / 2F);
                life = Math.Max (1, rng.Next(val, 100*level) * 6F);
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
                GetComponent<Rigidbody2D>().velocity = (speed * Vector3.Normalize(spider.transform.position - transform.position));
            } else
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }

            if (lifeBar.transform.localScale.x > 0.6)
            {
                lifeBar.GetComponentInChildren<SpriteRenderer>().color = green;
            }
            else if (lifeBar.transform.localScale.x > 0.3)
            {
                lifeBar.GetComponentInChildren<SpriteRenderer>().color = yellow;
            }
            else
            {
                lifeBar.GetComponentInChildren<SpriteRenderer>().color = red;
            }
        }

        void Die(DamageInfo dmgInfo)
        {
            float oldLife = currentLife;
            currentLife -= dmgInfo.Damage;
            lifeBar.transform.localScale = new Vector3(currentLife / life, 
                                                       lifeBar.transform.localScale.y, 
                                                       lifeBar.transform.localScale.z);

            if (lifeBar.transform.localScale.x < 0.1) {
                ExplosionFactory.Create(transform.position, 1.0f);
                AudioUtils.Play("ZombieDeath", transform.position);

                BloodFactory.SplatFromDamageInfo(dmgInfo);
                GameStatistics.AddKill(dmgInfo.PlayerNumber, this);

                Destroy(this.gameObject);
                Item.CreateRandom(transform.position);
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

        void OnCollisionStay2D(Collision2D collision)
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