using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
    public enum Statistic {
        MaxHealth,
        Damage,
    }

    struct Buff {
        public enum Type {
            Additive,
            Multiplicative
        }

        public float timeout;
        public Type type;
        public Statistic stat;
        public float value;
    }

    class BuffStack: MonoBehaviour {
        public Dictionary<Statistic, Dictionary<Buff.Type, List<Buff>>> Buffs { get; private set; }

        public BuffStack() {
            Buffs = new Dictionary<Statistic, Dictionary<Buff.Type, List<Buff>>>();

            foreach (Statistic stat in (Statistic[])Enum.GetValues(typeof(Statistic))) {
                Buffs[stat] = new Dictionary<Buff.Type, List<Buff>>();

                foreach (Buff.Type type in (Buff.Type[])Enum.GetValues(typeof(Buff.Type))) {
                    Buffs[stat][type] = new List<Buff>();
                }
            }
        }

        private IEnumerator RemoveWhenExpired(Buff buff) {
            yield return new WaitForSeconds(buff.timeout);
            Buffs[buff.stat][buff.type].Remove(buff);
        }

        public void Add(Buff buff) {
            Buffs[buff.stat][buff.type].Add(buff);

            // weird way to schedule something to be done after a delay
            // http://answers.unity3d.com/questions/897095/workaround-for-using-invoke-for-methods-with-param.html
            StartCoroutine(RemoveWhenExpired(buff));
        }

        public float Apply(float baseValue,
                           Statistic statistic) {
            var statBuffs = Buffs[statistic];

            float value = baseValue;
            foreach (Buff buff in statBuffs[Buff.Type.Additive]) {
                value += baseValue;
            }
            foreach (Buff buff in statBuffs[Buff.Type.Multiplicative]) {
                value *= baseValue;
            }

            return value;
        }
    }
}