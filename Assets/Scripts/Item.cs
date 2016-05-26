using System;
using Assets.Scripts.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts {
    public class Item : MonoBehaviour {
        public enum Type {
            HealthPack
        }

        public static GameObject CreateRandom(Vector3 position) {
            Type[] values = (Type[]) Enum.GetValues(typeof (Type));
            return Create(values.Random(new System.Random()),
                          position);
        }

        public static GameObject Create(Type type,
                                        Vector3 position) {
            GameObject itemObj = (GameObject) GameObject.Instantiate(Resources.Load<Object>("Item"),
                                                                     position,
                                                                     Quaternion.identity);
            SpriteRenderer renderer = itemObj.GetComponent<SpriteRenderer>();
            Item item = itemObj.GetComponent<Item>();

            Texture2D texture = Resources.Load<Texture2D>("Items/" + Enum.GetName(typeof (Type), type));
            Rect textureRect = Rect.MinMaxRect(0.0f, 0.0f, texture.width, texture.height);
            renderer.sprite = Sprite.Create(texture, textureRect, new Vector2(0.5f, 0.5f));

            itemObj.transform.localScale = new Vector2(0.5f, 0.5f);

            switch (type) {
            case Type.HealthPack:
                item.RestoreHealth = 100.0f;
                break;
            }

            return itemObj;
        }

        private Map map;

        public float? RestoreHealth;

        // Use this for initialization
        void Start () {
            map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
        }
	
        // Update is called once per frame
        void Update () {
            if (map.Player && map.Player.SelectedItem == this) {
                GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            } else {
                GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        void OnTriggerStay2D(Collider2D collider) {
            if (collider.tag == "Player"
                && map.Player
                && map.Player.SelectedItem == null) {
                map.Player.SelectedItem = this;
            }
        }

        void OnTriggerExit2D(Collider2D collider) {
            if (collider.tag == "Player"
                && map.Player
                && map.Player.SelectedItem == this) {
                map.Player.SelectedItem = null;
            }
        }
    }
}
