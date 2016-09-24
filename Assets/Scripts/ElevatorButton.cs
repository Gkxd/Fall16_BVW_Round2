using UnityEngine;
using UnityEngine.Events;
using System.Collections;

using UsefulThings;

public class ElevatorButton : MonoBehaviour
{
    public UnityEvent onButtonPress;

    private Transform buttonVisual;
    private Renderer buttonRenderer;

    void Start()
    {
        buttonVisual = transform.Find("Visual");
        buttonRenderer = buttonVisual.GetComponent<Renderer>();
    }

    void FixedUpdate()
    {
        buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, Vector3.zero, 0.1f);
    }

    void OnTriggerEnter(Collider c)
    {
        onButtonPress.Invoke();
        SfxManager.PlaySfx(0);
        buttonVisual.localPosition = Vector3.forward * 0.175f;
    }
}
