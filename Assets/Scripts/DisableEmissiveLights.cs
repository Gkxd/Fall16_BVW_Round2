using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class DisableEmissiveLights : MonoBehaviour
{
    void OnDisable()
    {
        DynamicGI.SetEmissive(GetComponent<Renderer>(), Color.black);
    }
}
