using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    public class AutoDestroy : Photon.PunBehaviour {
        public float maxLifeTime = 5;
        private float destroyTime;

        void Start() {
            destroyTime = Time.time + maxLifeTime;
        }

        void Update() {
            if (photonView.isMine) {
                if (Time.time >= destroyTime)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }
}
