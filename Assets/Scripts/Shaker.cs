using UnityEngine;

namespace Assets.Scripts {
    public class Shaker : MonoBehaviour {
        private new Camera camera;
        private float shake = 0.0f;
        private float shakeTime = 1.0f;
        private float shakeAmount = 0.7f;
 
        // Use this for initialization
        void Start () {
            camera = GetComponentInParent<Camera>();
        }

        public void Shake(float factor) {
            shakeTime = factor;
            shake = shakeTime;
            shakeAmount = factor;
        }
	
        // Update is called once per frame
        void Update () {
            if (shake > 0.0f) {
                Vector2 shakeDir = Random.insideUnitCircle * (shake / shakeTime) * shakeAmount;
                camera.transform.localPosition = new Vector3(shakeDir.x, shakeDir.y, camera.transform.localPosition.z);
                shake -= Time.deltaTime;
            } else {
                shake = 0.0f;
            }
        }
    }
}
