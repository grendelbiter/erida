using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3
{
    public class MissileController : Photon.PunBehaviour
    {
        public Transform currentTarget;
        private float startStamp;
        private float engageStamp;
        public float turnSpeed;
        public float baseThrust;
        private Rigidbody rigidBody;
        public UnityEngine.Object explosionEffect;
        public GameObject engines;

		void Start()
        {
            startStamp = Time.time;
            engageStamp = Time.time + 1f;
            turnSpeed = 0.6f;
            baseThrust = 25f;
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.AddForce(transform.forward * (baseThrust * 0.5f), ForceMode.Impulse);
        }

        void Update()
        {
            if (Time.time >= engageStamp)
            {
                if (!engines.GetActive())
                    engines.SetActive(true);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentTarget.transform.position - (transform.position + rigidBody.velocity)), turnSpeed * Time.deltaTime);
                rigidBody.velocity = transform.forward * baseThrust;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
