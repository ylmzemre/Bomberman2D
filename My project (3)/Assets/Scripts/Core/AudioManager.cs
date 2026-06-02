using UnityEngine;

namespace Bomberman2D.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource sfxSource;
        public AudioSource musicSource;

        [Header("Audio Clips")]
        public AudioClip explosionClip;
        public AudioClip dropBombClip;
        public AudioClip clickClip;
        public AudioClip deathClip;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayExplosion()
        {
            if (explosionClip != null) sfxSource.PlayOneShot(explosionClip);
        }

        public void PlayDropBomb()
        {
            if (dropBombClip != null) sfxSource.PlayOneShot(dropBombClip);
        }

        public void PlayClick()
        {
            if (clickClip != null) sfxSource.PlayOneShot(clickClip);
        }

        public void PlayDeath()
        {
            if (deathClip != null) sfxSource.PlayOneShot(deathClip);
        }
    }
}
