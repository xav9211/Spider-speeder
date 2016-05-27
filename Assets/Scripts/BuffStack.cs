using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
    public enum Statistic {
        MaxHealth,
        Damage,
    }

    public class Buff {
        public enum Type {
            Additive,
            Multiplicative
        }

        public Statistic stat;
        public Type type;
        public float value;
        public float timeout;
        public string icon;

        public Buff(Statistic stat,
                    Type type,
                    float value,
                    float timeout,
                    string icon) {
            this.stat = stat;
            this.type = type;
            this.value = value;
            this.timeout = timeout;
            this.icon = icon;
        }
    }

    public class BuffStack: MonoBehaviour {
        public Dictionary<Statistic, Dictionary<Buff.Type, List<Buff>>> Buffs { get; private set; }
        private BuffStackDisplay display;

        void Start() {
            display = GameObject.FindGameObjectWithTag("BuffStackDisplay").GetComponent<BuffStackDisplay>();
        }

        public BuffStack() {
            Buffs = new Dictionary<Statistic, Dictionary<Buff.Type, List<Buff>>>();

            foreach (Statistic stat in (Statistic[])Enum.GetValues(typeof(Statistic))) {
                Buffs[stat] = new Dictionary<Buff.Type, List<Buff>>();

                foreach (Buff.Type type in (Buff.Type[])Enum.GetValues(typeof(Buff.Type))) {
                    Buffs[stat][type] = new List<Buff>();
                }
            }
        }

        private List<Buff> AllBuffs() {
            List<Buff> allBuffs = new List<Buff>();

            foreach (var perStatBuffs in Buffs.Values) {
                foreach (var perStatPerTypeBuffs in perStatBuffs.Values) {
                    allBuffs.AddRange(perStatPerTypeBuffs);
                }
            }

            return allBuffs;
        }

        private IEnumerator RemoveWhenExpired(Buff buff) {
            yield return new WaitForSeconds(buff.timeout);
            //print("destroying buff: " + buff.stat + ", " + buff.type + ", " + buff.value + " for " + buff.timeout);
            Buffs[buff.stat][buff.type].Remove(buff);
            display.Refresh(AllBuffs());
        }

        public void Add(Buff buff) {
            //print("adding buff: " + buff.stat + ", " + buff.type + ", " + buff.value + " for " + buff.timeout);
            Buffs[buff.stat][buff.type].Add(buff);
            display.Refresh(AllBuffs());

            // weird way to schedule something to be done after a delay
            // http://answers.unity3d.com/questions/897095/workaround-for-using-invoke-for-methods-with-param.html
            StartCoroutine(RemoveWhenExpired(buff));
        }

        public float Apply(float baseValue,
                           Statistic statistic) {
            var statBuffs = Buffs[statistic];

            float value = baseValue;
            foreach (Buff buff in statBuffs[Buff.Type.Additive]) {
                value += buff.value;
            }
            foreach (Buff buff in statBuffs[Buff.Type.Multiplicative]) {
                value *= buff.value;
            }

            //print(statistic + " = " + baseValue + ", buffed: " + value);
            return value;
        }
    }
}