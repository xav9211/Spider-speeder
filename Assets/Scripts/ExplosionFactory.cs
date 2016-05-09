using Assets.Scripts;
using UnityEngine;

namespace Assets {
    public class ExplosionFactory {
        private static GameObject CreateParticleSystem(Vector2 position,
                                                       float force) {
            var explosion = (GameObject)GameObject.Instantiate(Resources.Load("Explosion"),
                                                               new Vector3(position.x, position.y),
                                                               Quaternion.identity);

            ParticleSystem system = explosion.GetComponent<ParticleSystem>();
            short numParticles = (short) MathUtils.ScaleToRange(force, 15.0f, 100.0f);
            ParticleSystem.Burst burst = new ParticleSystem.Burst(0, numParticles);
            system.emission.SetBursts(new ParticleSystem.Burst[] {burst});
            system.startSpeed = force;
            system.startSize = MathUtils.ScaleToRange(force, 0.5f, 1.0f);

            return explosion;
        }

        private static void ShakeScreen(float force) {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            Shaker shaker = camera.GetComponent<Shaker>();
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