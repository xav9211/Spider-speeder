using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts {
    public class HumanFriendlySeed {
        private static string vowels = "aeiouy";
        private static string consonants = "bcdfghjklmnpqrstvwxz";

        private int value;

        public int IntValue { get { return value; } }
        public string StringValue { get { return ToString(); } }

        public HumanFriendlySeed(int value) {
            this.value = value;
        }

        private static string SanitizeString(string str) {
            string sanitized = "";

            bool needsVowel = false;
            for (int i = 0; i < str.Length; ++i) {
                if ((needsVowel && vowels.Contains(str[i])) || (!needsVowel && consonants.Contains(str[i]))) {
                    sanitized += str[i];
                    needsVowel = !needsVowel;
                }
            }

            if (sanitized.Length % 2 == 1) {
                return sanitized.Substring(0, sanitized.Length - 1);
            }
            return sanitized;
        }

        private static int ParseDigraph(string str) {
            Assert.IsTrue(str.Length == 2);
            Assert.IsTrue(str == SanitizeString(str));

            return consonants.IndexOf(str[0]) * vowels.Length + vowels.IndexOf(str[1]);
        }

        private static string MakeDigraph(int n) {
            Assert.IsTrue(n < vowels.Length * consonants.Length);

            return consonants[n / vowels.Length].ToString() + vowels[n % vowels.Length].ToString();
        }

        public static HumanFriendlySeed FromString(string str) {
            long val = 0;
            string sanitized = SanitizeString(str);

            for (int i = 0; i < sanitized.Length; i += 2) {
                val *= vowels.Length * consonants.Length;
                val += ParseDigraph(sanitized.Substring(i, 2));
            }

            return new HumanFriendlySeed((int)val);
        }

        public override string ToString() {
            string stringValue = "";
            long val = (uint)value;

            while (val > 0) {
                stringValue = MakeDigraph((int)(val % (vowels.Length * consonants.Length))) + stringValue;
                val /= vowels.Length * consonants.Length;
            }

            return stringValue;
        }
    }
}
