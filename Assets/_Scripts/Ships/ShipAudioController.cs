using UnityEngine;
using System.Collections;

[System.Serializable]
public class ShipAudioController : System.Object
{
    public AudioSource _audioSource;

    [Header("Sounds")]
    public AudioClip primarySound;
    public AudioClip secondarySound;
    public AudioClip teleportSound;
    public AudioClip collisionSound;
    public AudioClip deathSound;

    public void Play(AudioClip audioClip, float volume = 1f)
    {
        _audioSource.PlayOneShot(audioClip, volume);
    }

    public void Loop(AudioClip audioClip)
    {
        _audioSource.clip = audioClip;

        if(_audioSource.loop == false)
        {
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }

    public void StopLoop()
    {
        _audioSource.loop = false;
    }
}
