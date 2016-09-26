using UnityEngine;
using System.Collections;

public class FlickerLights : MonoBehaviour
{

    private Renderer r;
    private Color onColor;

    void OnEnable()
    {
        r = GetComponent<Renderer>();
        onColor = r.material.GetColor("_EmissionColor");
        SetColor(Color.black);
        StartCoroutine(Flicker());
    }
    
    private IEnumerator Flicker() {
        yield return new WaitForSeconds(Random.Range(0, 5f));
        while (true)
        {
            for (int i = 0; i < Random.Range(1, 4); i++)
            {
                SetColor(onColor);
                yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
                SetColor(Color.black);
                yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
            }

            yield return new WaitForSeconds(Random.Range(3f, 5f));
        }
    }

    private void SetColor(Color c)
    {
        DynamicGI.SetEmissive(r, c);
        r.material.SetColor("_EmissionColor", c);
    }
}
