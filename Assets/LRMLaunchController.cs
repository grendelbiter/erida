using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3
{
    public class LRMLaunchController : Photon.PunBehaviour
    {
        private float launchStamp;
        private float launchDelay = 10f;
        private float salvoDelay = 1f;
        private int salvoCount = 3;
        private int currentCount = 0;
        public Object projectile;
        public Transform target;

        void Start()
        {
            launchStamp = Time.time + launchDelay;
        }

        void Update()
        {
            if (Time.time >= launchStamp)
            {
                GameObject p = (GameObject)Instantiate(projectile, transform.position, transform.rotation);
                p.GetComponent<MissileController>().currentTarget = target;
                currentCount++;
                if (currentCount < salvoCount)
                {
                    launchStamp = Time.time + salvoDelay;
                }
                else
                {
                    launchStamp = Time.time + launchDelay;
                    currentCount = 0;
                }
            }

        }
    }
}