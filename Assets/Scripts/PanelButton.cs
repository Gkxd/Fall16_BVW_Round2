using UnityEngine;
using UnityEngine.Events;
using System.Collections;

using UsefulThings;

public class PanelButton : MonoBehaviour {

    public UnityEvent onButtonPress;

    private Transform buttonVisual;
    private Renderer buttonRenderer;

    // Use this for initialization
    void Start () {
        buttonVisual = transform.Find("Visual");
        buttonRenderer = buttonVisual.GetComponent<Renderer>();
    }

    void FixedUpdate()
    {
        buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, Vector3.zero, 0.1f);
    }

    // Update is called once per frame
    void OnTriggerEnter() {

        Debug.Log("asdasdasd");

        onButtonPress.Invoke();
        buttonVisual.localPosition = Vector3.back * 0.002f;
    }
}
