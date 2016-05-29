﻿using System;
using Assets.Scripts.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts {
    public class Item : MonoBehaviour {
        public enum Type {
            HealthPack,
            StrengthBonus,
            StrengthDouble,
            WebSwingRangeBonus,
            WebSwingRangeInfinite,
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

            renderer.sprite = Resources.Load<Sprite>("Items/" + type);
            itemObj.transform.localScale = new Vector3(0.5f, 0.5f, 1.0f);

            const float DEFAULT_BUFF_TIMEOUT_S = 10.0f;

            switch (type) {
            case Type.HealthPack:
                item.RestoreHealth = 100.0f;
                break;
            case Type.StrengthBonus:
                item.ApplyBuff = new Buff(Statistic.Damage,
                                          Buff.Type.Additive,
                                          10.0f,
                                          DEFAULT_BUFF_TIMEOUT_S,
                                          "Items/StrengthBonus");
                break;
            case Type.StrengthDouble:
                item.ApplyBuff = new Buff(Statistic.Damage,
                                          Buff.Type.Multiplicative,
                                          2.0f,
                                          DEFAULT_BUFF_TIMEOUT_S,
                                          "Items/StrengthDouble");
                break;
            case Type.WebSwingRangeBonus:
                item.ApplyBuff = new Buff(Statistic.WebSwingRange,
                                          Buff.Type.Additive,
                                          10.0f,
                                          DEFAULT_BUFF_TIMEOUT_S,
                                          "Items/WebSwingRangeBonus");
                break;
            case Type.WebSwingRangeInfinite:
                item.ApplyBuff = new Buff(Statistic.WebSwingRange,
                                          Buff.Type.Additive,
                                          1000000.0f, // good approximation of infinity
                                          DEFAULT_BUFF_TIMEOUT_S,
                                          "Items/WebSwingRangeInfinite");
                break;
            }

            return itemObj;
        }

        private Map.Map map;

        public float? RestoreHealth;
        public Buff ApplyBuff;

        // Use this for initialization
        void Start () {
            map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map.Map>();
        }
	
        // Update is called once per frame
        void Update () {
            if (map.Player && map.Player.SelectedItem == this) {
                GetComponentInChildren<Light>().enabled = true;
            } else {
                GetComponentInChildren<Light>().enabled = false;
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
