using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    public class PowerCellManager : Photon.PunBehaviour {

        private int maxEnergy = 8;

        private List<Transform> powerableObjects = new List<Transform>();
        private List<Transform> poweredObjects = new List<Transform>();
        private float checkStamp = 0;
        private float checkDelay = 1.25f;

        private Unit myUnit;

        private bool started = false;

        void Awake()
        {
            myUnit = GetComponent<Unit>();
        }

        private void Start()
        {
            started = true;
        }
        // Update is called once per frame
        void Update()
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (Time.time > checkStamp)
                {
                    checkStamp = Time.time + checkDelay;
                    CleanUpPoweredObjects();

                    for (int i = powerableObjects.Count-1; i >= 0; i--)
                    {
                        if (powerableObjects[i] == null)
                        {
                            powerableObjects.RemoveAt(i);
                        }
                        else
                        {
                            Unit u = powerableObjects[i].transform.GetComponent<Unit>();
                            if (u.needsPower && !u.hasPower && poweredObjects.Count < maxEnergy)
                            {
                                PhotonView pv = u.transform.GetComponent<PhotonView>();
                                poweredObjects.Add(powerableObjects[i]);
                                object[] o = new object[1];
                                o[0] = myUnit.unitTeam;
                                pv.RPC("RecievePower", PhotonTargets.All, o);
                            }
                        }
                    }
                }
            }
        }

        private void CleanUpPoweredObjects()
        {
            for (int i = poweredObjects.Count - 1; i >= 0; i--)
            {
                if (poweredObjects[i] == null)
                    poweredObjects.RemoveAt(i);
            }
        }

        private void OnDestroy()
        {
            if (PhotonNetwork.connected)
            {
                for (int i = 0; i < poweredObjects.Count; i++)
                {
                    if (poweredObjects[i] != null)
                    {
                        PhotonView pv = poweredObjects[i].GetComponent<PhotonView>();
                        if (pv != null)
                            pv.RPC("LosePower", PhotonTargets.All);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && !other.isTrigger)
            {
                Unit u = other.transform.GetComponent<Unit>();
                if (u != null && u.needsPower && u.unitTeam == myUnit.unitTeam && !powerableObjects.Contains(other.transform))
                    powerableObjects.Add(other.transform);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (PhotonNetwork.isMasterClient && started && !other.isTrigger)
            {
                Unit u = other.transform.GetComponent<Unit>();
                if (u != null && u.needsPower && (u.unitTeam == myUnit.unitTeam || !u.hasPower) && !powerableObjects.Contains(other.transform))
                    powerableObjects.Add(other.transform);
            }
        }
    }
}