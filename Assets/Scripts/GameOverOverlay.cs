using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
    public class GameOverOverlay : MonoBehaviour {
        private Dictionary<CanvasRenderer, float> baseAlpha = new Dictionary<CanvasRenderer, float>();

        // Use this for initialization
        void Start () {
            foreach (CanvasRenderer renderer in GetComponentsInChildren<CanvasRenderer>()) {
                baseAlpha.Add(renderer, renderer.GetAlpha());
            }
        }

        // Update is called once per frame
        void Update() {
            foreach (KeyValuePair<CanvasRenderer, float> rc in baseAlpha) {
                float alpha = rc.Key.GetAlpha();
                if (alpha < rc.Value) {
                    rc.Key.SetAlpha(Mathf.Min(alpha + 0.01f * rc.Value, 1.0f));
                }
            }
        }

        void OnEnable () {
            foreach (CanvasRenderer renderer in baseAlpha.Keys) {
                renderer.SetAlpha(0.0f);
            }
        }
    }
}
