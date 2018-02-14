using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Wulfram3;

namespace UnityStandardAssets.Effects
{
    public class ExplosionPhysicsForce : Photon.PunBehaviour {
        public float explosionForce = 4;

        private void Start()
        {
            float multiplier = 1f;
            ParticleSystemMultiplier p = GetComponent<ParticleSystemMultiplier>();
            if (p != null)
                multiplier = p.multiplier;
            float r = 10*multiplier;
            Collider[] cols = Physics.OverlapSphere(transform.position, r);
            foreach (Collider col in cols)
            {
                if (col.attachedRigidbody != null)
                    col.attachedRigidbody.AddExplosionForce(explosionForce * multiplier, transform.position, r, multiplier, ForceMode.Impulse);
            }
        }

    }
}
