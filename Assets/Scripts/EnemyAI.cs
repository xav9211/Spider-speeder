using UnityEngine;
using System.Collections;

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
        GameObject spider = map.Player;
        if (spider != null && Vector3.Distance(spider.transform.position, transform.position) < 10)
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
//        print(col.gameObject.name);
        if (col.gameObject.name == "SpiderBody")
        {
            col.gameObject.SendMessage("Die");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
//        print(collision.gameObject.name);
        if(collision.gameObject.name == "SpiderBody")
        {
            GameObject.Find("Spider").SendMessage("Die");
        }
    }
}