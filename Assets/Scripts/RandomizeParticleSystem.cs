using UnityEngine;
using System.Collections;

public class RandomizeParticleSystem : MonoBehaviour {

    public bool playOnStart = false;

	void Start () {
        ParticleSystem p = GetComponent<ParticleSystem>();
        p.randomSeed = (uint)Random.Range(int.MinValue, int.MaxValue);
        p.Simulate(0);

        if(playOnStart)
            p.Play();
	}
}
