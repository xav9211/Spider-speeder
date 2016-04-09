﻿using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    Transform lifeBar;
    
    float speed = 0.2F;
    // Use this for initialization
    void Start()
    {
        lifeBar = transform.FindChild("LifeBar");
    }

    // Update is called once per frame
    void Update()
    {
        GameObject spider = GameObject.Find("SpiderBody");
        if (spider != null)
        {
            GetComponent<Rigidbody2D>().velocity = (speed * Vector3.Normalize(spider.transform.position - transform.position));
        } else
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }

    void Die()
    {
        lifeBar.transform.localScale -= Vector3.up*0.1F;
        if(lifeBar.transform.localScale.y < 0.1)
        {
            Destroy(this.gameObject);
        }
        
    }

    void OnTriggerEnter(Collider col)
    {
        print(col.gameObject.name);
        if (col.gameObject.name == "SpiderBody")
        {
            col.gameObject.SendMessage("Die");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        print(collision.gameObject.name);
        if(collision.gameObject.name == "SpiderBody")
        {
            GameObject.Find("Spider").SendMessage("Die");
        }
    }
}