using UnityEngine;
using System.Collections;

public class ParticleScale : MonoBehaviour {
	

	public float scaleStep = 0.1f;

	private ParticleSystem _particle = null;



	public void UpdateScale(float mod) {

		if(particle == null) {
			return;
		}
        ParticleSystem.MinMaxCurve m;
        m = particle.main.startSize;
		m.constant = m.constant + m.constant * mod;
        m = particle.main.startSpeed;
		m.constant = m.constant + m.constant * mod;		
	}


	public void ReduceScale(float mod) {
		if(particle == null)
			return;
        ParticleSystem.MinMaxCurve m;
        m = particle.main.startSize;
        m.constant = m.constant - m.constant * mod;
        m = particle.main.startSpeed;
        m.constant = m.constant - m.constant * mod;

    }


    public ParticleSystem particle {
		get
        {
			if(_particle == null)
				_particle = GetComponent<ParticleSystem>();
			return _particle;
		}
	}
}
