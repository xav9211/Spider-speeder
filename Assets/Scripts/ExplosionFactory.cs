using UnityEngine;

namespace Assets.Scripts {
    public class ExplosionLight: MonoBehaviour {
        private new Light light;

        void Start() {
            light = GetComponent<Light>();
        }

        void Update() {
            light.range /= 1.1f;
        }
    }

    public class ExplosionFactory {
        private static GameObject CreateParticleSystem(Vector2 position,
                                                       float force) {
            var explosion = (GameObject)GameObject.Instantiate(Resources.Load("Explosion"),
                                                               new Vector3(position.x, position.y),
                                                               Quaternion.identity);
            explosion.transform.SetParent(GameObject.FindGameObjectWithTag("Map").transform);

            ParticleSystem system = explosion.GetComponent<ParticleSystem>();
            short numParticles = (short) MathUtils.ScaleToRange(force, 15.0f, 100.0f);
            ParticleSystem.Burst burst = new ParticleSystem.Burst(0, numParticles);
            system.emission.SetBursts(new ParticleSystem.Burst[] {burst});
            system.startSpeed = force;
            system.startSize = MathUtils.ScaleToRange(force, 0.5f, 1.0f);

            var lightObj = new GameObject("ExplosionLight", typeof(Light), typeof(ExplosionLight));
            lightObj.transform.SetParent(explosion.transform);
            lightObj.transform.localPosition = new Vector3(0.0f, 0.0f, -2.5f);

            Light light = lightObj.GetComponent<Light>();
            light.range = MathUtils.ScaleToRange(force, 25.0f, 100.0f);
            light.color = Color.white;

            return explosion;
        }

        private static void ShakeScreen(float force) {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            Shaker shaker = camera.GetComponentInChildren<Shaker>();
            shaker.Shake(force);
        }

        public static GameObject Create(Vector2 position,
                                        float force = 1.0f) {
            AudioUtils.Play("Explosion", position, 0.5f);
            ShakeScreen(MathUtils.ScaleToRange(force, 0.5f, 3.0f));
            return CreateParticleSystem(position, force * 0.1f);
        }
    }
}