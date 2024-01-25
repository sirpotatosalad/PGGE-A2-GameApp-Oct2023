using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PGGE.Patterns;

public class MenuSoundManager : Singleton<MenuSoundManager>
{
    // this sound manager is a singleton that makes use of the Singleton pattern


    private AudioSource audioSource;
    public AudioClip btnClickSound;

    private void Awake()
    {
        // uses the base class' awake function to add this gameObject to DontDestroyOnLoad
        base.Awake();

        // setting the audioSource component, and adding if it does not exist.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null )
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // function for other scripts to call to play the button sounds when pressing a button in the menu.
    public void PlayButtonClickSound()
    {
        if ( btnClickSound != null )
        {
            audioSource.PlayOneShot( btnClickSound );
        }
    }

}
