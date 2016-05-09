﻿using UnityEngine;
using System.Collections;
using Assets;
using Assets.Scripts;

public class EnemyAI : MonoBehaviour
{
    Transform lifeBar;
    private Map map;
    
    float speed = 0.2F;
    // Use this for initialization
    void Start()
    {
        lifeBar = transform.FindChild("LifeBar");
        map = FindObjectOfType<Map>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject spider = map.PlayerBody;
        if (spider != null && Vector3.Distance(spider.transform.position, transform.position) < 10)
        {
            GetComponent<Rigidbody2D>().velocity = (speed * Vector3.Normalize(spider.transform.position - transform.position));
        } else
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }

    void Die(float damage)
    {
        lifeBar.transform.localScale -= Vector3.up*(damage / 100.0f);
        if (lifeBar.transform.localScale.y < 0.1) {
            ExplosionFactory.Create(transform.position, 1.0f);
            AudioUtils.Play("ZombieDeath", transform.position);
            Destroy(this.gameObject);
        }
        else {
            AudioUtils.Play("ZombieHit", transform.position);
        }
        
    }

    void OnTriggerEnter(Collider col)
    {
//        print(col.gameObject.name);
        if (col.gameObject.tag == "SpiderBody")
        {
            col.gameObject.SendMessage("Die", 10.0f);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
//        print(collision.gameObject.name);
        if(collision.gameObject.tag == "SpiderBody")
        {
            AudioUtils.Play("ZombieAttack", transform.position);
            map.Player.SendMessage("Die", 10.0f);
        }
    }
}