using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Assets.Scripts {
    static class TimeSpanExtensions {
        public static string ToGameTimeString(this TimeSpan time) {
            return string.Format("{0}:{1:D2}.{2:D3}",
                                 Mathf.FloorToInt((float) time.TotalMinutes),
                                 time.Seconds,
                                 time.Milliseconds);
        }
    }

    class GameStatistics: MonoBehaviour {
        public struct Stats {
            public float TotalDamageDealt;
            public float MaxDamageDealt;
            public int EnemiesKilled;
            public int ItemsPickedUp;
        }

        public static Stats[] PerPlayerStats;

        public static float LastResetTime { get; private set; }

        // TODO: public float TotalDamageReceived { get; private set; }
        // TODO: public float MaxDamageReceived { get; private set; }

        public static void Add(DamageInfo dmgInfo) {
            if (dmgInfo.PlayerNumber.HasValue) {
                Stats s = PerPlayerStats[dmgInfo.PlayerNumber.Value];
                s.TotalDamageDealt += dmgInfo.Damage;
                s.MaxDamageDealt = Mathf.Max(s.MaxDamageDealt, dmgInfo.Damage);
            }
            // TODO: global stats
        }

        public static void AddItemPickup(int playerNumber,
                                         Item item) {
            Assert.IsTrue(playerNumber == 1 || playerNumber == 2);

            ++PerPlayerStats[playerNumber - 1].ItemsPickedUp;
        }

        public static void AddKill(int? playerNumber,
                                   EnemyAI killedEnemy) {
            Assert.IsTrue(!playerNumber.HasValue || (playerNumber.Value == 1 || playerNumber.Value == 2));

            if (playerNumber.HasValue) {
                ++PerPlayerStats[playerNumber.Value - 1].EnemiesKilled;
            }
            // TODO: remember toughest enemy killed or sth
        }

        public static void Reset() {
            PerPlayerStats = new Stats[2];
            LastResetTime = Time.time;
        }

        private struct LabeledStat {
            public string label;
            public string[] values;
        };

        void OnEnable() {
            if (PerPlayerStats == null) {
                Reset();
            }

            LabeledStat[] perPlayerStats = new LabeledStat[] {
                new LabeledStat() {
                    label = "TotalDamage",
                    values = new string[] {
                        PerPlayerStats[0].TotalDamageDealt.ToString(),
                        PerPlayerStats[1].TotalDamageDealt.ToString()
                    }
                },
                new LabeledStat() {
                    label = "MaxDamage",
                    values = new string[] {
                        PerPlayerStats[0].MaxDamageDealt.ToString(),
                        PerPlayerStats[1].MaxDamageDealt.ToString()
                    }
                },
                new LabeledStat() {
                    label = "Kills",
                    values = new string[] {
                        PerPlayerStats[0].EnemiesKilled.ToString(),
                        PerPlayerStats[1].EnemiesKilled.ToString()
                    }
                },
                new LabeledStat() {
                    label = "ItemsPickedUp",
                    values = new string[] {
                        PerPlayerStats[0].ItemsPickedUp.ToString(),
                        PerPlayerStats[1].ItemsPickedUp.ToString()
                    }
                },
            };

            Transform perPlayerStatsContainer = transform.FindChild("PerPlayerStats");

            foreach (LabeledStat stat in perPlayerStats) {
                Transform statContainer = perPlayerStatsContainer.FindChild(stat.label);

                statContainer.FindChild("P1").GetComponent<Text>().text = stat.values[0];
                statContainer.FindChild("P2").GetComponent<Text>().text = stat.values[1];

                // TODO: that cast is soo ugly
                statContainer.FindChild("Star1").gameObject.SetActive(double.Parse(stat.values[0]) >= double.Parse(stat.values[1]));
                statContainer.FindChild("Star2").gameObject.SetActive(double.Parse(stat.values[0]) <= double.Parse(stat.values[1]));
            }

            Transform generalStatsContainer = transform.FindChild("GeneralStats");

            Map.Map map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map.Map>();
            generalStatsContainer.FindChild("Level/Value").GetComponent<Text>().text = map.Level.ToString();

            TimeSpan gameplayTime = TimeSpan.FromSeconds(Time.time - LastResetTime);
            generalStatsContainer.FindChild("GameplayTime/Value").GetComponent<Text>().text = gameplayTime.ToGameTimeString();
        }
    }
}
