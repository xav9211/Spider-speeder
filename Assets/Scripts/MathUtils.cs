using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts {
    class MathUtils {
        public static float ScaleToRange(float value,
                                         float min,
                                         float max) {
            Assert.IsTrue(value >= 0.0f);
            return min + (max - min)*(1.0f - 1.0f/(1.0f + Mathf.Exp(-value)));
        }

    }
}
