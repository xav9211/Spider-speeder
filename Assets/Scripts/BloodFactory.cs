using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts {
    class BloodFactory {
        public static GameObject SplatFromDamageInfo(DamageInfo info) {
            return SplatFromDirection(info.TargetPosition, info.TargetPosition - info.SourcePosition, info.Damage);
        }

        public static GameObject SplatFromDirection(Vector3 position,
                                                    Vector3 direction,
                                                    float force) {
            var splat = (GameObject)GameObject.Instantiate(Resources.Load("BloodSplat"),
                                                           new Vector3(position.x, position.y),
                                                           Quaternion.identity);
            splat.transform.SetParent(GameObject.FindGameObjectWithTag("Map").transform);

            splat.transform.rotation = Quaternion.Euler(180.0f + (float) (Mathf.Rad2Deg*Math.Atan2(direction.y, direction.x)),
                                                        270.0f,
                                                        0.0f);

            ParticleSystem system = splat.GetComponent<ParticleSystem>();
            system.startSize = MathUtils.ScaleToRange(Mathf.Sqrt(force), 1.0f, 3.0f);

            return splat;
        }
    }
}
