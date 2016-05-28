using UnityEngine;

namespace Assets.Scripts {
    public static class RandomExtensions {
        public static float NextFromRange(this System.Random rng,
                                          float minInclusive,
                                          float maxExclusive) {
            return minInclusive + (float) (rng.NextDouble()*(maxExclusive - minInclusive));
        }
    }

    public class TorchLight : MonoBehaviour {
        private new Light light;
        private float baseRange;
        private float rangeStep;
        private float targetRange;
        private System.Random rng = new System.Random();

        // Use this for initialization
        void Start () {
            light = GetComponent<Light>();
            baseRange = light.range;
            targetRange = baseRange;
        }
	
        // Update is called once per frame
        void Update () {
            if (Mathf.Abs(light.range - targetRange) < 0.001f) {
                float flickerFactor = 0.3f * baseRange;
                targetRange = baseRange + rng.NextFromRange(-flickerFactor, flickerFactor);
                rangeStep = (targetRange - light.range) / rng.Next(3, 10);
            }

            light.range += rangeStep;
        }
    }
}
