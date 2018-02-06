﻿using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System;
using UnityEngine;

namespace Com.Wulfram3
{
    public class Unit : Photon.PunBehaviour {

        public PunTeams.Team unitTeam;
        public UnitType unitType;
        public int maxHealth;
        public float hitpointRegenPerSecond = 0.3f;
        public bool needsPower = false;

        //[HideInInspector]
        //public float playerLandedBoost = 2.65f;
        //[HideInInspector]
        //public float repairPadBoost = 75f;
        [HideInInspector]
        public int health;
        [HideInInspector]
        public bool hasPower = false;

        private float currentBoost = 1f;
        private float healthCollected = 0;
        private GameManager gameManager;
        private bool isPlayer = false;
        private PlayerManager playerManager;

        private float syncTeamStamp;
        private float syncUnitStamp;

        public bool needsUpdate = true;

        // Use this for initialization
        void Start() {
            //if (unitTeam == null)
            //    Debug.Log("Unit.cs is missing a team assignment.");
            //if (unitType == null)
            //    Debug.Log("Unit.cs is missing a type assignment.");
            if (unitType == UnitType.RepairPad || unitType == UnitType.RefuelPad || unitType == UnitType.GunTurret || unitType == UnitType.FlakTurret || unitType == UnitType.MissleLauncher)
                needsPower = true;
            isPlayer = GetComponent<PlayerMotionController>() != null;
        }

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            if (PhotonNetwork.isMasterClient)
                SetHealth(maxHealth);
        }

        // Update is called once per frame
        void Update() {
            if (needsUpdate)
            {

            }
            if (photonView.isMine)
            {
                currentBoost = 1f;
                if (isPlayer)
                    currentBoost = GetComponent<PlayerMotionController>().healingBoost;
                float health = hitpointRegenPerSecond * currentBoost * Time.deltaTime;
                healthCollected += health;
                if (healthCollected >= 1f)
                {
                    healthCollected--;
                    if (playerManager == null || !playerManager.isDead)
                    {
                        TellServerTakeDamage(-1);
                    }
                }
            }
            if (PhotonNetwork.isMasterClient)
            {
                if (Time.time > syncTeamStamp)
                {
                    syncTeamStamp = Time.time + 2f;
                    SyncTeam(unitTeam);
                }
                if (Time.time > syncUnitStamp)
                {
                    syncUnitStamp = Time.time + 5f;
                    SyncUnit(unitType);
                }
            }
        }

        [PunRPC]
        public void SyncTeam(PunTeams.Team t)
        {
            if (PhotonNetwork.isMasterClient)
            {
                photonView.RPC("SyncTeam", PhotonTargets.Others, unitTeam);
            } else
            {
                unitTeam = t;
            }
        }

        [PunRPC]
        public void SyncUnit(UnitType u)
        {
            if (PhotonNetwork.isMasterClient)
            {
                photonView.RPC("SyncUnit", PhotonTargets.Others, unitType);
            } else
            {
                unitType = u;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext((int)health);
                //stream.SendNext((PunTeams.Team)unitTeam);
                if (playerManager != null)
                {
                    stream.SendNext((int)playerManager.GetMeshIndex());
                }
            }
            else if (stream.isReading)
            {
                int syncHealth = (int)stream.ReceiveNext();
                health = Mathf.Clamp(syncHealth, 0, maxHealth);
                /*
                PunTeams.Team syncTeam = (PunTeams.Team)stream.ReceiveNext();
                unitTeam = syncTeam;
                */
                if (playerManager != null)
                {
                    int syncMesh = (int)stream.ReceiveNext();
                    playerManager.SetMesh(syncMesh);
                }
            }
        }

        public static string GetPrefabName(UnitType u, PunTeams.Team t)
        {
            string tf = PunTeamToTeamString(t);
            string s = "Prefabs/" + tf + "/" + tf + "_";
            switch (u)
            {
                case UnitType.Cargo: s += "Cargo"; break;
                case UnitType.Darklight: s += "Darklight"; break;
                case UnitType.FlakTurret: s += "FlakTurret"; break;
                case UnitType.GunTurret: s += "GunTurret"; break;
                case UnitType.MissleLauncher: s += "MissileLauncher"; break;
                case UnitType.PowerCell: s += "Powercell"; break;
                case UnitType.RepairPad: s += "RepairPad"; break;
                case UnitType.Skypump: s += "Skypump"; break;
                case UnitType.Tank: s += "Tank"; break;
                case UnitType.Scout: s += "Scout"; break;
                case UnitType.Uplink: s += "Uplink"; break;
                default:
                    Debug.Log("UnitTypeToPrefabString(" + u.ToString() + ", " + t.ToString() + ") ERROR: Unknown UnitType. Defaulting to cargobox!");
                    s += "Cargo";
                    break;
            }
            return s;
        }

        public static string PunTeamToTeamString(PunTeams.Team t)
        {
            if (t == PunTeams.Team.Blue)
            {
                return "Blue";
            }
            else if (t == PunTeams.Team.Red)
            {
                return "Red";
            }
            return "Grey";
        }


        public bool IsUnitFriendly()
        {
            if (PlayerManager.LocalPlayerInstance.GetComponent<Unit>().unitTeam == this.unitTeam)
                return true;
            return false;
        }

        public PunTeams.Team GetHostileTeam()
        {
            switch (unitTeam)
            {
                case PunTeams.Team.Red:
                    return PunTeams.Team.Blue;
                case PunTeams.Team.Blue:
                    return PunTeams.Team.Red;
                default:
                    return PunTeams.Team.none;
            }
        }

        public override string ToString()
        {
            return Enum.GetName(typeof(UnitType), unitType) + " " + Enum.GetName(typeof(PunTeams.Team), unitTeam) + "| |" + this.unitType.ToString() + " " + this.unitTeam.ToString();
        }

        [PunRPC]
        public void TakeDamage(int amount)
        {
            if (PhotonNetwork.isMasterClient)
            {
                int newHealth = Mathf.Clamp(health - amount, 0, maxHealth);
                photonView.RPC("UpdateHealth", PhotonTargets.All, newHealth);
            }
        }

        public void SetMaxHealth()
        {
            SetHealth(maxHealth);
        }

        public void SetHealth(int newHealth)
        {
            if (PhotonNetwork.isMasterClient)
            {
                photonView.RPC("UpdateHealth", PhotonTargets.All, newHealth);
            }
        }

        public void TellServerTakeDamage(int amount)
        {
            photonView.RPC("TakeDamage", PhotonTargets.MasterClient, amount);
        }

        [PunRPC]
        public void RecievePower(PunTeams.Team t)
        {
            hasPower = true;
            unitTeam = t;
        }

        [PunRPC]
        public void LosePower()
        {
            hasPower = false;
            unitTeam = PunTeams.Team.none;
        }

        [PunRPC]
        public void UpdateHealth(int amount)
        {
            int newHealth = Mathf.Clamp(amount, 0, maxHealth);
            health = newHealth;
            if (playerManager != null && photonView.isMine)
            {
                gameManager.SetHullBar((float)health / (float)maxHealth);
            } else if (PhotonNetwork.isMasterClient && (maxHealth != 0 && health <= 0))
            {
                PhotonNetwork.Destroy(gameObject);
                gameManager.SpawnExplosion(transform.position); 
            }
        }
    }
}
