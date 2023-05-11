using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioClip[] swish;
    
    public AudioClip click;

    public AudioSource audioSource;

    public GameObject backgroundMusic;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void playClick()
    {
        audioSource.clip = click;
        audioSource.Play();
    }

    public void playSwish()
    {
        audioSource.clip = swish[Random.Range(0, swish.Length)];
        audioSource.Play();
    }

}
