using System;
using UnityEngine;

namespace Mono
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        
        public AudioSource m_AudioSource;
        public AudioClip m_ShootAudioClip;
        public AudioClip m_HitAudioClip;

        public float playHitAudioInterval = 0.2f;
        public float playHitAudioValidTime = 0.1f;
        public float lastPlayerHitAudioTime = 0.2f;
        
        private void Awake()
        {
            instance = this;
        }

        public void PlayShoot()
        {
            m_AudioSource.PlayOneShot(m_ShootAudioClip);
        }

        public void PlayHitAudio()
        {
            m_AudioSource.PlayOneShot(m_HitAudioClip);
        }

        private void Update()
        {
            if (ShareData.gameSharedData.Data.playHitAudio 
                && Time.time - lastPlayerHitAudioTime > playHitAudioInterval
                && Time.time - ShareData.gameSharedData.Data.playHitAudioTime < Time.deltaTime)
            {
                lastPlayerHitAudioTime = Time.time;
                PlayHitAudio();
                ShareData.gameSharedData.Data.playHitAudio = false;
            }
        }
    }
}