using UnityEngine;

namespace Assets {
    public class ExplosionFactory {
        private static GameObject CreateParticleSystem(Vector2 position,
                                                       float force) {
            var explosion = (GameObject)GameObject.Instantiate(Resources.Load("Explosion"),
                                                               new Vector3(position.x, position.y),
                                                               Quaternion.identity);

            ParticleSystem system = explosion.GetComponent<ParticleSystem>();
            short numParticles = (short) (Mathf.Min(1000.0f, 150.0f*force));
            ParticleSystem.Burst burst = new ParticleSystem.Burst(0, numParticles);
            system.emission.SetBursts(new ParticleSystem.Burst[] {burst});
            system.startSpeed = force;
            system.startSize = Mathf.Max(1.0f, force);

            return explosion;
        }

        private static void ShakeScreen(float force) {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            Shaker shaker = camera.GetComponent<Shaker>();
            shaker.Shake(force);
        }

        private static void PlaySound(Vector3 position) {
            AudioClip sound = (AudioClip)Resources.Load("Explosion", typeof(AudioClip));
            AudioSource.PlayClipAtPoint(sound, position, 0.5f);
        }

        public static GameObject Create(Vector2 position,
                                        float force = 1.0f) {
            PlaySound(position);
            ShakeScreen(Mathf.Sqrt(force));
            return CreateParticleSystem(position, force * 0.1f);
        }
    }
}