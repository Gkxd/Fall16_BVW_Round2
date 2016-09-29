using UnityEngine;
using System.Collections;

using UsefulThings;

public class InitGame : MonoBehaviour {
	void Start () {
        StartCoroutine(StartSequence());
	}

    private IEnumerator StartSequence()
    {
        SfxManager.PlayLoop(0); // Start ambient noise
        SfxManager.PlaySfx(14); // Phone ringing
        yield return new WaitForSeconds(3);
        // SfxManager.PlaySfx(8); // Party invite
    }
}
