using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts {
    public class EnableRotationEffect: Effect {
        private Map.Map map;
        public float Timeout { get { return 10; } }

        public EnableRotationEffect(Map.Map map) {
            this.map = map;
        }
        
        public void Apply() {
            if (map.Player) {
                Rigidbody2D rigidbody = map.Player.GetComponent<Rigidbody2D>();
                rigidbody.freezeRotation = false;
            }
        }

        public IEnumerator Unapply() {
            if (map.Player) {
                Rigidbody2D rigidbody = map.Player.GetComponent<Rigidbody2D>();
                float rotation = (rigidbody.rotation % 360.0f + 360.0f) % 360.0f;
                if (rotation > 180.0f) {
                    rotation -= 360.0f;
                }

                rigidbody.rotation = rotation;
                rigidbody.freezeRotation = true;

                while (Mathf.Abs(rigidbody.rotation) > 0.01f) {
                    rigidbody.rotation /= 1.1f;
                    yield return new WaitForSeconds(0.033f);
                }

                rigidbody.rotation = 0.0f;
            }
        }
    }

    public class EffectContainer: MonoBehaviour {
        private List<Effect> activeEffects = new List<Effect>();

        private IEnumerator UnapplyAfter(Effect effect) {
            yield return new WaitForSeconds(effect.Timeout);
            //print("destroying buff: " + buff.stat + ", " + buff.type + ", " + buff.value + " for " + buff.timeout);
            activeEffects.Remove(effect);

            // TODO: there's only one effect type for now...
            if (activeEffects.Count == 0) {
                StartCoroutine(effect.Unapply());
            }
    }

        public void Add(Effect effect) {
            effect.Apply();
            activeEffects.Add(effect);

            // weird way to schedule something to be done after a delay
            // http://answers.unity3d.com/questions/897095/workaround-for-using-invoke-for-methods-with-param.html
            StartCoroutine(UnapplyAfter(effect));
        }
    }

    public interface Effect {
        float Timeout { get; }

        void Apply();
        IEnumerator Unapply();
    }
}