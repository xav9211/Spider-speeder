using UnityEngine;

namespace Assets {
    public class ExplosionFactory {
        public static GameObject Create(Vector2 position,
                                        float force = 1.0f) {
            force *= 0.1f;

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
    }
}