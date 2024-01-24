using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PGGE.Patterns;

public class MenuSoundManager : Singleton<MenuSoundManager>
{
    private AudioSource audioSource;
    public AudioClip btnClickSound;

    private void Awake()
    {
        base.Awake();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null )
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }


    public void PlayButtonClickSound()
    {
        if ( btnClickSound != null )
        {
            audioSource.PlayOneShot( btnClickSound );
        }
    }

}
