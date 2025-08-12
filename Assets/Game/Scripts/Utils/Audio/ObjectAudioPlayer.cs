using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectAudioPlayer : MonoBehaviour
{
        [System.Serializable]
        public class SoundIndexes
        {
                public int index;
                public float minSoundPitch = .8f;
                public float maxSoundPitch = 1.3f;
        }
        
        [SerializeField] private SoundIndexes[] soundsIndexes;
        [SerializeField] private float playSoundDelay = .1f;
        private float _lastPlaySoundTime = 0;

        public void PlaySound(int index, bool forceDelay = false)
        {
                if (soundsIndexes == null || soundsIndexes.Length == 0 || (Time.time - _lastPlaySoundTime < playSoundDelay && !forceDelay))
                        return;
                _lastPlaySoundTime = Time.time;
                
                SoundIndexes soundIndex = soundsIndexes[index];
                float pitch = Random.Range(soundIndex.minSoundPitch, soundIndex.maxSoundPitch);
                GlobalAudioPlayer.PlayAudio(soundIndex.index, pitch);
        }

        public void StopSound(int index)
        {
                GlobalAudioPlayer.StopAudio(soundsIndexes[index].index);
        }
        
        public void StopSound()
        {
                for (int i = 0; i < soundsIndexes.Length; i++)
                {
                        GlobalAudioPlayer.StopAudio(soundsIndexes[i].index);
                }
        }
        
        public void PlaySound()
        {
                if (soundsIndexes == null || soundsIndexes.Length == 0 || Time.time - _lastPlaySoundTime < playSoundDelay)
                        return;
                
                int index = Random.Range(0, soundsIndexes.Length);
                float pitch = Random.Range(soundsIndexes[index].minSoundPitch, soundsIndexes[index].maxSoundPitch);

                if (!GlobalAudioPlayer.PlayAudio(soundsIndexes[index].index, pitch))
                {
                        for (int i = 0; i < soundsIndexes.Length; i++)
                        {
                                pitch = Random.Range(soundsIndexes[index].minSoundPitch, soundsIndexes[index].maxSoundPitch);
                                if (GlobalAudioPlayer.PlayAudio(soundsIndexes[i].index, pitch))
                                        break;
                        }
                }
                _lastPlaySoundTime = Time.time;
        }
}