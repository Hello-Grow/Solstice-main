using UnityEngine;
using Yarn.Unity;

public class RadioScript : MonoBehaviour
{
    public AudioSource speaker;
    public AudioClip[] songs; //we can add multipel songs here
    public int track = 0;

    [YarnCommand("play")]
    public void PlayMusic()
    {
        if (songs.Length == 0) return; 

        speaker.clip = songs[track];
        speaker.Play();
    }

    [YarnCommand("stop")]
    public void StopMusic()
    {
        speaker.Stop();
    }

    [YarnCommand("next")]
    public void NextMusic()
    {
        track = track + 1;
        
        if (track >= songs.Length)
        {
            track = 0; 
        }
        
        PlayMusic();
    }

    [YarnCommand("back")]
    public void BackMusic()
    {
        track = track - 1;
    
        if (track < 0)
        {
            track = songs.Length - 1; 
        }
        
        PlayMusic();
    }
}