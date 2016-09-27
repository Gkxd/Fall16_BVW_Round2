using UnityEngine;
using System.Collections;

public class JumpScare : MonoBehaviour
{
    void OnTriggerEnter(Collider c)
    {
        Debug.Log("BOO!");
    }
}
