using UnityEngine;
using System.Collections;

public class FlickerLights : MonoBehaviour
{
    public bool flickerOff;

    private Renderer r;
    private Color onColor;

    private Coroutine flickerRoutine;

    void OnEnable()
    {
        r = GetComponent<Renderer>();
        onColor = r.material.GetColor("_EmissionColor");
        if (!flickerOff) SetColor(Color.black); 
        flickerRoutine = StartCoroutine(Flicker());
    }

    void OnDisable()
    {
        StopCoroutine(flickerRoutine);
        SetColor(flickerOff ? onColor : Color.black);
    }
    
    private IEnumerator Flicker() {
        yield return new WaitForSeconds(Random.Range(0, 5f));
        while (true)
        {
            for (int i = 0; i < Random.Range(1, 4); i++)
            {
                SetColor(flickerOff ? Color.black : onColor);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                SetColor(flickerOff ? onColor : Color.black);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }

            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }

    private void SetColor(Color c)
    {
        DynamicGI.SetEmissive(r, c);
        r.material.SetColor("_EmissionColor", c);
    }
}
