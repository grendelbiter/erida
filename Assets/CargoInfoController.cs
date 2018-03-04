using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Wulfram3
{
    public class CargoInfoController : Photon.PunBehaviour
    {

        public Text cargoName;
        public Camera cargoCamera;
        public Transform cargoDisplayTransform;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (gameObject.GetActive())
            {
                cargoDisplayTransform.Rotate(Vector3.up, 0.5f);
            }
        }
    }
}