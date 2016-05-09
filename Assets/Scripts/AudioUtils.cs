using System;
using UnityEngine;

namespace Assets.Scripts {
    class AudioUtils {
        private static AudioClip LoadAudio(string resourceName) {
            return (AudioClip) Resources.Load(resourceName, typeof (AudioClip));
        }
        public static void Play(string resourceName,
                                Vector3 position) {
            AudioSource.PlayClipAtPoint(LoadAudio(resourceName), position);
        }

        public static void Play(string resourceName) {
            // surprisingly, that doesn't override background music, but rather
            // attaches new sound to the same AudioSource
            BackgroundMusic.PlayOneShot(LoadAudio(resourceName));
        }

        public static AudioSource BackgroundMusic { get {
            return GameObject.FindGameObjectWithTag("AudioSource")
                             .GetComponent<AudioSource>();
        } }
    }
}
