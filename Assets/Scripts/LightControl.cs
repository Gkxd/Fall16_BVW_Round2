using UnityEngine;
using System.Collections;

public class LightControl : MonoBehaviour {

    public GameObject elevatorLight;
    public GameObject redLight;
    public Renderer[] ceilingLights;

    public Color ceilingLightColor;

    public void CeilingLightOff (int i)
    {
        if (i == 4)
        {
            ceilingLights[i].materials[1].SetColor("_EmissionColor", Color.black);
            DynamicGI.SetEmissive(ceilingLights[i], Color.black);
        }
        else
        {
            ceilingLights[i].material.SetColor("_EmissionColor", Color.black);
            DynamicGI.SetEmissive(ceilingLights[i], Color.black);
        }
    }

    public void CeilingLightOn(int i)
    {
        if (i == 4)
        {
            ceilingLights[i].materials[1].SetColor("_EmissionColor", ceilingLightColor);
            DynamicGI.SetEmissive(ceilingLights[i], ceilingLightColor);
        }
        else
        {
            ceilingLights[i].material.SetColor("_EmissionColor", ceilingLightColor);
            DynamicGI.SetEmissive(ceilingLights[i], ceilingLightColor);
        }
    }

    public void AllLightsOff()
    {
        elevatorLight.SetActive(false);
        redLight.SetActive(true);
        
        for (int i = 0; i < ceilingLights.Length; i++)
        {
            CeilingLightOff(i);
        }
    }

    public void AllLightsOn()
    {
        elevatorLight.SetActive(true);
        redLight.SetActive(false);

        for (int i = 0; i < ceilingLights.Length; i++)
        {
            CeilingLightOn(i);
        }
    }
}
