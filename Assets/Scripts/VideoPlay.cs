using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;



using UsefulThings;

public class VideoPlay : MonoBehaviour
{
    private MovieTexture movie;
    private AudioSource aud;
    void Start()
    {
        Renderer r = GetComponent<Renderer>();


        movie = (MovieTexture)r.material.mainTexture;
        movie.Play();

        //Debug.Log(movie.duration);

        aud = GetComponent<AudioSource>();
        aud.clip = movie.audioClip;
        aud.Play();

        StartCoroutine(LoadMainLevel());
    }

    IEnumerator LoadMainLevel()
    {
        yield return new WaitForSeconds(movie.duration);
        movie.Stop();
        aud.Stop();
        //SceneManager.LoadScene(1);
        
    }
}
