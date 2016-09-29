using UnityEngine;
using System.Collections;

using UsefulThings;

[RequireComponent(typeof(TimeKeeper))]
public class LerpPositions : MonoBehaviour {

    public Transform destination;
    public Curve movementCurve;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private TimeKeeper tk;

	void Start () {
        startPosition = transform.localPosition;
        endPosition = destination.localPosition;

        tk = GetComponent<TimeKeeper>();
	}
	
	void Update () {
        transform.localPosition = Vector3.Lerp(startPosition, endPosition, movementCurve.Evaluate(tk));
	}
}
