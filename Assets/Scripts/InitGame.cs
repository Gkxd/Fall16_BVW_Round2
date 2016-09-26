using UnityEngine;
using System.Collections;

using UsefulThings;

public class InitGame : MonoBehaviour {
	void Start () {
        SfxManager.PlayLoop(0); // Start ambient noise
	}
}
