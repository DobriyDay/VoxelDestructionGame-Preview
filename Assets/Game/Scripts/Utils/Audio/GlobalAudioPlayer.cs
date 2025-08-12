using System.Collections.Generic;
using UnityEngine;

public class GlobalAudioPlayer : MonoBehaviour
{
    [System.Serializable]
    public class AudioSourceData
    {
        public AudioSource audioSource;
        public int count;
    }
    
    [SerializeField] private AudioSourceData[] audioSourceDataList;
    private static Dictionary<int, List<AudioSource>> audioSources = new();
    
    private void Awake()
    {
        CreateAudioSources();
    }
    
    private void CreateAudioSources()
    {
        for (int i = 0; i < audioSourceDataList.Length; i++)
        {
            audioSources[i] = new List<AudioSource>();
            audioSources[i].Add(audioSourceDataList[i].audioSource);
            for (int j = 0; j < audioSourceDataList[i].count - 1; j++)
            {
                AudioSource newSource = Instantiate(audioSourceDataList[i].audioSource, transform);
                newSource.playOnAwake = false;
                audioSources[i].Add(newSource);
            }
        }
    }

    private static bool WhoCanPlay(int index, out AudioSource audioSource)
    {
        audioSource = null;
        if (!audioSources.ContainsKey(index)) 
            return false;
        
        foreach (var source in audioSources[index])
        {
            if (source != null && !source.isPlaying)
            {
                audioSource = source;
                return true;
            }
        }
        return false;
    }
    
    public static bool PlayAudio(int index, float pitch = 1)
    {
        if (WhoCanPlay(index, out AudioSource audioSource))
        {
            audioSource.pitch = pitch;
            audioSource.Play();
            return true;
        }

        return false;
    }

    public static void StopAudio(int index)
    {
        foreach (var source in audioSources[index])
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }
}