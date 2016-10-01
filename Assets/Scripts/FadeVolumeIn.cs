using UnityEngine;
using System.Collections;

public class FadeVolumeIn : MonoBehaviour
{

    public float volume;
    public float time;

    private AudioSource audioSource;
    private float startTime;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        startTime = Time.time;
    }

    void Update()
    {
        audioSource.volume = Mathf.Lerp(0, volume, (Time.time - startTime) / time);
    }
}
