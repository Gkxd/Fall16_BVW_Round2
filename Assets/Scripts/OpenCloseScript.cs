using UnityEngine;
using System.Collections;
using Leap.Unity;

public class OpenCloseScript : MonoBehaviour {
    
    public enum OpenState {Close,Open};

    public OpenState openType;

    private bool interruptFired = false;

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
            GameObject.FindWithTag("Elevator").GetComponent<ElevatorControls>().MoveDoors((float)openType);
            
            if (openType == OpenState.Open)
            {
                StartCoroutine(CloseDoorsAutomatically(10));
            }
            else
            {

            }

        }
    }

    IEnumerator CloseDoorsAutomatically(float time)
    {
        yield return new WaitForSeconds(time);

        GameObject.FindWithTag("Elevator").GetComponent<ElevatorControls>().MoveDoors((float)OpenState.Close);
        // Code to execute after the delay
    }

    // Update is called once per frame
    void Update () {
	
	}
}
