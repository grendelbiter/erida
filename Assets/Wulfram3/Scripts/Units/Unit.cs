using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Com.Wulfram3
{
    [RequireComponent(typeof(Rigidbody))]
    public class Unit : Photon.PunBehaviour {

        public PunTeams.Team unitTeam; // Set in inspector
        public UnitType unitType; // Set in inspector
        public int maxHealth; // Set in inspector (Ensure teams match)
        public float hitpointRegenPerSecond = 0.3f; // Set in inspector (Ensure teams match)
        public int targetPriority = 0; // Can be set in inspector, will be overridden by Start() for many units

        // Internal vars
        public bool needsPower = false; // Set by Start(), Unit.cs controls gaining and losing power, "external" functions should go in another script that uses this bool to control Update()
        [HideInInspector]
        public int health;
        [HideInInspector]
        public bool hasPower = false;
        [HideInInspector]
        public bool isDead = false;
        private GameManager gameManager;
        private PlayerManager playerManager;
        private float currentBoost = 1f; // Multiplier. Grabs bonuses for landed and repair pad from PlayerMotionController
        private float healthCollected = 0f; // Accumulator for regeneration
        private float damageToTake = 0f;    // ref needed by SmoothDamp used in health display
        private float displayHealth; // Smoothed value used for display
        private float syncHpStamp; // "Timer" for masterclient health updates, default 1 per second

        void Start() {
            // Force these units to require power
            if (unitType == UnitType.RepairPad || unitType == UnitType.RefuelPad || unitType == UnitType.GunTurret || unitType == UnitType.FlakTurret || unitType == UnitType.MissleLauncher)
                needsPower = true;
            // Force these units to target priority 1, 2 = projectiles, 3 = flares (flares not implemented as of 2/16/2018)
            if (unitType == UnitType.Tank || unitType == UnitType.Scout || unitType == UnitType.GunTurret || unitType == UnitType.FlakTurret || unitType == UnitType.Other || unitType == UnitType.None)
                targetPriority = 1;
        }

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            playerManager = GetComponent<PlayerManager>();
            // No team means we are a player, team will in the args passed with PhotonNetwork.Instantiate() See: PlayerSpawnManager.SpawnPlayer
            if (unitTeam == PunTeams.Team.none)
                unitTeam = (PunTeams.Team)photonView.instantiationData[1];
            if (playerManager != null) // Make really sure we're a player
            {
                int receivedIdx = (int)photonView.instantiationData[2]; // This will be the mesh index, PlayerManager will do the rest
                if (unitType == UnitType.None)
                    unitType = PlayerManager.GetPlayerTypeFromMeshIndex(receivedIdx);
                playerManager.SetMesh(unitTeam, unitType);
                maxHealth = playerManager.mySettings.MaxHitPoints; // Make sure we use hit points set by the vehicle interface
                displayHealth = maxHealth;
                gameManager.SetHullBar(1f);
            }
            if (PhotonNetwork.isMasterClient)
                SetHealth(maxHealth); // Masterclient has health and damage authority, for the most part
            SyncTeam(unitTeam);
            SyncUnit(unitType); 
        }

        void Update() {
            if (photonView.isMine && !isDead)
            {
                currentBoost = 1f;
                if (playerManager != null)
                    currentBoost = GetComponent<PlayerMotionController>().healingBoost; // Landed and repair pad boosts
                float tickHealth = hitpointRegenPerSecond * currentBoost * Time.deltaTime;
                healthCollected += tickHealth;
                if (healthCollected >= 1f)
                {
                    healthCollected--;
                    TellServerTakeDamage(-1);
                }
            }
            if (photonView.isMine && playerManager != null && displayHealth != health) // HUD update needs to happen even when dead, only on change, and only for players
            {
                displayHealth = Mathf.SmoothDamp(displayHealth, health, ref damageToTake, 5f * Time.deltaTime);
                gameManager.SetHullBar((float)displayHealth / (float)maxHealth);
            }
            if (PhotonNetwork.isMasterClient && Time.time > syncHpStamp) // Masterclient will sync health across the network 1 time per second
            {
                syncHpStamp = Time.time + 1f;
                photonView.RPC("UpdateHealth", PhotonTargets.All, health);
            }
        }

        [PunRPC]
        public void SyncTeam(PunTeams.Team t)
        {
            if (PhotonNetwork.isMasterClient)
                photonView.RPC("SyncTeam", PhotonTargets.Others, unitTeam);
            else
                unitTeam = t;
        }

        [PunRPC]
        public void SyncUnit(UnitType u)
        {
            if (PhotonNetwork.isMasterClient)
                photonView.RPC("SyncUnit", PhotonTargets.Others, unitType);
            else
                unitType = u;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {

        }

        public static string GetPrefabName(UnitType u, PunTeams.Team t)
        {
            string tf = PunTeamToTeamString(t);
            return "Prefabs/" + tf + "/" + tf + "_" + GetUnitName(u);
        }

        public static string GetNoScriptUnitName(UnitType u, PunTeams.Team t)
        {
            return "Prefabs/NoScriptUnits/" + PunTeamToTeamString(t) + "_" + GetUnitName(u);
        }

        public static string GetUnitName(UnitType u)
        {
            string s = "";
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
                    Logger.Log("UnitTypeToPrefabString(" + u.ToString() + ") ERROR: Unknown UnitType. Defaulting to cargobox!");
                    s += "Cargo";
                    break;
            }
            return s;
        }

        public static string PunTeamToTeamString(PunTeams.Team t)
        {
            if (t == PunTeams.Team.Blue)
                return "Blue";
            else if (t == PunTeams.Team.Red)
                return "Red";
            return "Grey";
        }


        public bool IsUnitFriendly()
        {
            if (PhotonNetwork.player.GetTeam() == this.unitTeam)
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

        public void SetHealth(int newHealth)
        {
            if (PhotonNetwork.isMasterClient)
                photonView.RPC("UpdateHealth", PhotonTargets.All, newHealth);
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
            if (PhotonNetwork.isMasterClient)
                SyncTeam(unitTeam);
        }

        [PunRPC]
        public void LosePower()
        {
            hasPower = false;
            unitTeam = PunTeams.Team.none;
            if (PhotonNetwork.isMasterClient)
                SyncTeam(unitTeam);
        }

        [PunRPC]
        public async void UpdateHealth(int amount)
        {
            int newHealth = Mathf.Clamp(amount, 0, maxHealth);        
            if (photonView.isMine && (health - amount) > 0)
            {
                // Hit flash
                gameManager.hitPanel.SetActive(true);
                await Task.Delay(200);
                gameManager.hitPanel.SetActive(false);
            }
            this.health = newHealth;
            if ((maxHealth != 0 && this.health <= 0) && !isDead)
            {
                isDead = true;
                gameManager.SetCurrentTarget(null);
                if (playerManager == null && PhotonNetwork.isMasterClient)
                {
                    gameManager.SpawnExplosion(transform.position, unitType);
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }
}
