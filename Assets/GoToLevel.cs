using UnityEngine;
using System.Collections;
using Leap.Unity;

public class GoToLevel : MonoBehaviour {

    public int floor;

	// Use this for initialization
	void Start () {
	
	}

    private bool IsHand(Collider other)
    {
        if (other.transform.parent && other.transform.parent.parent && other.transform.parent.parent.GetComponent<HandModel>())
            return true;
        else
            return false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (IsHand(col))
        {
            gameObject.GetComponent<AudioSource>().Play();
        }
    }

	// Update is called once per frame
	void Update () {
	    
	}
}
