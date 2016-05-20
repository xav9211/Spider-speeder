using UnityEngine;
using System.Collections;
using Assets;
using Assets.Scripts;
using System;
public class EnemyAI : MonoBehaviour
{
    Transform lifeBar;
	private Map map;
	private System.Random rng;


	float damage = 10F;
    float speed = 0.2F;
	float life = 10F;
	float currentLife;
    // Use this for initialization
    void Start()
    {
        lifeBar = transform.FindChild("LifeBar");
        map = FindObjectOfType<Map>();

	}

	public float createMonster(int level){
		var renderer = (SpriteRenderer)gameObject.GetComponent ("SpriteRenderer");

		rng = new System.Random ();
		var val = rng.Next(100*level);
		if (val < 20){
			renderer.sprite = Resources.Load<Sprite> ("enemies/enemy2");
			speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 50F);
			damage = Math.Max (1, rng.Next(val, 100*level) / 10F);
			life = Math.Max (1, rng.Next(val, 100*level) * 3F);
		}
		else if (val >= 20 && val < 40) {
			renderer.sprite = Resources.Load<Sprite> ("enemies/creature1");
			speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 50F);
			damage = Math.Max (1, rng.Next(val, 100*level) / 10F);
			life = Math.Max (1, rng.Next(val, 100*level) * 3F);
		} else if (val >= 40 && val < 60) {
			renderer.sprite = Resources.Load<Sprite> ("enemies/enemy3");
			speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 50F);
			damage = Math.Max (1, rng.Next(val, 100*level) / 10F);
			life = Math.Max (1, rng.Next(val, 100*level) * 3F);
		} else if (val >= 60 && val < 80) {
			renderer.sprite = Resources.Load<Sprite> ("enemies/Przechwytywanie");
			speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 50F);
			damage = Math.Max (1, rng.Next(val, 100*level) / 10F);
			life = Math.Max (1, rng.Next(val, 100*level) * 3F);
		} else if (val >= 80 && val < 100) {
			renderer.sprite = Resources.Load<Sprite> ("enemies/putin");
			speed = Math.Max (0.1F, rng.Next (val, 100 * level) / 30F);
			damage = Math.Max (1, rng.Next(val, 100*level) / 2F);
			life = Math.Max (1, rng.Next(val, 100*level) * 6F);
		}

		currentLife = life;





		float newScale =  0.7F / renderer.sprite.bounds.extents.x;
		transform.localScale = new Vector3(newScale, newScale, 1);

		transform.GetChild(0).transform.localScale = new Vector3(1/newScale, 1/newScale, 1);

		var collider = (BoxCollider2D)gameObject.GetComponent ("BoxCollider2D");
		collider.size = new Vector2(renderer.sprite.bounds.extents.x*2, renderer.sprite.bounds.extents.y*2);

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
    }

    void Die(DamageSource damageSource)
    {
		float oldLife = currentLife;
		currentLife -= damageSource.Damage;
		lifeBar.transform.localScale = new Vector3(lifeBar.transform.localScale.x, 
			                                       lifeBar.transform.localScale.y * life / oldLife * currentLife / life, 
			                                       lifeBar.transform.localScale.z);

        if (lifeBar.transform.localScale.y < 0.1) {
            ExplosionFactory.Create(transform.position, 1.0f);
            AudioUtils.Play("ZombieDeath", transform.position);

            BloodFactory.SplatFromDamageSource(transform.position, damageSource);

            Destroy(this.gameObject);
        }
        else {
            AudioUtils.Play("ZombieHit", transform.position);
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