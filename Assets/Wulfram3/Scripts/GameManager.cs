﻿using Assets.Wulfram3.Scripts.InternalApis;
using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Assets.Wulfram3.Scripts.InternalApis.Interfaces;
using Assets.Wulfram3.Scripts.Units;
using System.Collections.Generic;


using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.Wulfram3
{
    public class GameManager : Photon.PunBehaviour
    {
        public GameObject hullBar;

        public GameObject fuelBar;

        public Material redcolor;
        public Material bluecolor;
        public Material graycolor;

        public GameObject playerInfoPanelPrefab;
        public Transform[] spawnPointsBlue;
        public Transform[] spawnPointsRed;

        [HideInInspector]
        public Camera normalCamera;

        private TargetInfoController targetChangeListener;

        private bool isFirstSpawn = true;

        public VehicleSelector unitSelector;

        #region Photon Messages

        /*
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }
        */

        public override void OnPhotonPlayerConnected(PhotonPlayer other)
        {
            if (PhotonNetwork.isMasterClient)
                Debug.Log(other.NickName + " has connected as MasterClient. (GameManager.cs / OnPhotonPlayerConnected:46)");
            else
                Debug.Log(other.NickName + " has connected. (GameManager.cs / OnPhotonPlayerConnected:48)");
        }


        public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
        {
            if (PhotonNetwork.isMasterClient)
                Debug.Log("MasterClient " + other.NickName + " has disconnected. This will probably get ugly.  (GameManager.cs / OnPhotonPlayerDisconnected:55)");
            else
                Debug.Log(other.NickName + " has disconnected. (GameManager.cs / OnPhotonPlayerDisconnected:57)");
        }

        /*
        public override void OnJoinedRoom()
        {
            Debug.Log("Room Joined. (GameManager.cs / OnJoinedRoom:62)");
            if (PhotonNetwork.room.PlayerCount == 1)
            {
                Debug.Log("Loading Game Scene ('Playground' as MasterClient). (GameManager.cs / OnJoinedRoom:66)");
                PhotonNetwork.LoadLevel("Playground");
            }
            else
            {
                Debug.Log("Syncing Game Scene ('Playground' as RemoteClient). (GameManager.cs / OnJoinedRoom:69)");
            }
        }
        */
        public void LeaveRoom()
        {
            //[ASSIGNED NEVER USED] var userControler = DepenencyInjector.Resolve<IUserController>();
            //var discordApi = DepenencyInjector.Resolve<IDiscordApi>();
            PhotonNetwork.LeaveRoom();
            //StartCoroutine(discordApi.PlayerLeft(PhotonNetwork.playerName));
        }
        #endregion


        #region Public Methods
        public void Start()
        {
            Debug.Log("Starting GameManager.  (GameManager.cs / Start:86)");

            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.Log("Creating LocalPlayer. Active scene name: " + SceneManager.GetActiveScene().name + ". (GameManager.cs / Start:89)");
                GameObject g = Instantiate(Resources.Load("Prefabs/SceneBase/VehicleSelector"), new Vector3(-500, -500, -500), Quaternion.identity, transform) as GameObject;
                unitSelector = g.GetComponent<VehicleSelector>();

                PunTeams.UpdateTeamsNow();
                object[] o = new object[1];
                List<int> availableUnits = new List<int>();
                if (PunTeams.PlayersPerTeam[PunTeams.Team.Blue].Count > PunTeams.PlayersPerTeam[PunTeams.Team.Red].Count)
                {
                    o[0] = PunTeams.Team.Red;
                    availableUnits.Add(2);
                    availableUnits.Add(3);
                }
                else
                {
                    o[0] = PunTeams.Team.Blue;
                    availableUnits.Add(0);
                    availableUnits.Add(1);
                }
                Debug.Log("Assigned to " + o[0] + " team. Awaiting first spawn. (GameManager.cs / Start:107)");
                unitSelector.SetAvailableModels(availableUnits);
                PhotonNetwork.Instantiate("Prefabs/Player/Player", new Vector3(0, -100, 0), Quaternion.identity, 0, o);
                PhotonNetwork.player.SetTeam((PunTeams.Team)o[0]);
            }
            else
            {
                Debug.Log("Local Player Was Null.... Investigate. (GameManager.cs / Start:113)" + SceneManager.GetActiveScene().name);
            }
            GetComponent<PlayerSpawnManager>().StartSpawn();
            GetComponent<MapModeManager>().ActivateMapMode(MapType.Spawn);
        }

        private void Update() { }

        [PunRPC]
        public void SpawnPulseShell(Vector3 pos, Quaternion rot, PunTeams.Team team, Vector3 vel, int senderID, PhotonMessageInfo info)
        {
            if (PhotonNetwork.isMasterClient)
            {
                object[] instanceData = new object[3];
                instanceData[0] = team;
                instanceData[1] = UnitType.Tank;
                instanceData[2] = vel;
                PhotonView senderPV = PhotonView.Find(senderID);
                GameObject shell = PhotonNetwork.InstantiateSceneObject("Prefabs/Weapons/PulseShell", pos, rot, 0, instanceData);
                senderPV.RPC("DestroyLocalShell", info.sender, shell.GetPhotonView().viewID);
            }
        }

        public void SpawnFlakShell(Vector3 pos, Quaternion rotation, PunTeams.Team team, float fuse)
        {
            if (PhotonNetwork.isMasterClient)
            {
                object[] instanceData = new object[3];
                instanceData[0] = team;
                instanceData[1] = UnitType.FlakTurret;
                instanceData[2] = fuse;
                PhotonNetwork.InstantiateSceneObject("Prefabs/Weapons/PulseShell", pos, rotation, 0, instanceData);
            }
        }

        public void SpawnExplosion(Vector3 pos)
        {
            if (PhotonNetwork.isMasterClient)
                PhotonNetwork.InstantiateSceneObject("Effects/Explosion_01", pos, Quaternion.identity, 0, null);
        }

        /*
    public void UnitsHealthUpdated(HitPointsManager hitpointsManager)
    {
        if (hitpointsManager.tag.Equals("Player") && hitpointsManager.photonView.isMine)
        {
            SetHullBar((float)hitpointsManager.health / (float)hitpointsManager.maxHealth);
        }
        if (PhotonNetwork.isMasterClient && hitpointsManager.health <= 0 && !hitpointsManager.tag.Equals("Player"))
        {
            PhotonNetwork.Destroy(hitpointsManager.gameObject);
            SpawnExplosion(hitpointsManager.transform.position);
        }
    }*/

        public void SetHullBar(float level)
        {
            hullBar.GetComponent<LevelController>().SetLevel(level);
        }

        public void FuelLevelUpdated(FuelManager fuelManager)
        {
            SetFuelBar((float)fuelManager.fuel / (float)fuelManager.maxFuel);
        }

        public void SetFuelBar(float level)
        {
            fuelBar.GetComponent<LevelController>().SetLevel(level);
        }

        public void SetCurrentTarget(GameObject go)
        {
            if (targetChangeListener != null)
            {
                targetChangeListener.TargetChanged(go);
            }
        }

        public void AddTargetChangeListener(TargetInfoController tic)
        {
            targetChangeListener = tic;
        }

        public void DestroyNow(GameObject go)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.Destroy(go);
                SpawnExplosion(go.transform.position);
            }
        }

        public string GetColoredPlayerName(string name, bool isMaster, bool colorFull = false, PunTeams.Team team = PunTeams.Team.none)
        {
            string masterClient = "";
            string modTag = "";
            string username = "";
            string teamColor = "";
            switch (team)
            {
                case PunTeams.Team.none:
                    teamColor = this.graycolor.color.ToHex();
                    break;
                case PunTeams.Team.Red:
                    teamColor = this.redcolor.color.ToHex();
                    break;
                case PunTeams.Team.Blue:
                    teamColor = this.bluecolor.color.ToHex();
                    break;
                default:
                    teamColor = this.graycolor.color.ToHex();
                    break;
            }


            if (isMaster)
            {
                masterClient = "<color=magenta>*</color>";
            }

            if (name.Contains("[MOD]"))
            {
                var names = name.Split(' ');

                modTag = "<color=yellow>" + names[0] + "</color>";
                if (colorFull == true)
                {
                    username = modTag + " " + "<color=" + teamColor + ">" + names[1] + "</color>";
                }
                else
                {
                    username = modTag + " " + "<color=white>" + names[1] + "</color>";
                }
            }
            else if (name.Contains("[DEV]"))
            {
                var names = name.Split(' ');

                modTag = "<color=#e0950b>" + names[0] + "</color>";
                if (colorFull == true)
                {
                    username = modTag + " " + "<color=" + teamColor + ">" + names[1] + "</color>";
                }
                else
                {
                    username = modTag + " " + "<color=white>" + names[1] + "</color>";
                }
            }
            else
            {
                if (colorFull == true)
                {
                    username = "<color=" + teamColor + ">" + name + "</color>";
                }
                else
                {
                    username = "<color=white>" + name + "</color>";
                }
            }

            return masterClient + username;
        }

        /*
         * 
         * MOVED: To Unit.cs
         * 
         * 
        public string PunTeamToTeamString(PunTeams.Team t)
        {
            if (t == PunTeams.Team.blue)
            {
                return "Blue";
            } else if (t == PunTeams.Team.red)
            {
                return "Red";
            } else
            {
                return "Grey";
            }
        }

        public string UnitTypeToPrefabString(UnitType u, PunTeams.Team t)
        {
            string tf = PunTeamToTeamString(t);
            string s = "Unit_Prefabs/" + tf + "/" + tf + "_";
            switch(u)
            {
                case UnitType.Cargo: s += "Cargo";  break;
                case UnitType.Darklight: s += "Darklight";  break;
                case UnitType.FlakTurret: s += "FlakTurret";  break;
                case UnitType.GunTurret: s += "GunTurret";  break;
                case UnitType.MissleLauncher: s += "Launcher"; break;
                case UnitType.PowerCell: s += "Powercell";  break;
                case UnitType.RepairPad: s += "RepairPad";  break;
                case UnitType.Skypump: s += "Skypump";  break;
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
        */

        public void Respawn(PlayerManager player)
        {
            if (PlayerSpawnManager.status == SpawnStatus.IsAlive)
            {

                GetComponent<PlayerSpawnManager>().StartSpawn();
                GetComponent<MapModeManager>().ActivateMapMode(MapType.Spawn);
            }
        }

        [PunRPC]
        public void RequestDropCargo(int senderID)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonView pv = PhotonView.Find(senderID);
                if (pv == null)
                {
                    Debug.Log("GameManager.cs --- RPC --- RequestDropCargo() Failed to find Photon View with ID: " + senderID);
                    return;
                }
                CargoManager cargoManager = pv.transform.GetComponent<CargoManager>();
                if (cargoManager != null && cargoManager.hasCargo)
                {
                    Unit u = cargoManager.transform.GetComponent<Unit>();
                    if (u != null)
                    {
                        object[] o = new object[2];
                        o[0] = cargoManager.cargoType;
                        o[1] = cargoManager.cargoTeam;
                        PhotonNetwork.InstantiateSceneObject(Unit.GetPrefabName(UnitType.Cargo, u.unitTeam), cargoManager.dropPosition.position, cargoManager.dropPosition.rotation, 0, o);
                        pv.RPC("DroppedCargo", PhotonTargets.All, null);
                    }
                }
            }
        }

        [PunRPC]
        public void RequestDeployCargo(object[] args)
        {
            if (PhotonNetwork.isMasterClient)
            {
                Vector3 desiredPosition = (Vector3)args[0];
                Quaternion desiredRotation = (Quaternion)args[1];
                UnitType cargoType = (UnitType)args[2];
                PunTeams.Team cargoTeam = (PunTeams.Team)args[3];
                object[] o = new object[2];
                o[0] = cargoType;
                o[1] = cargoTeam;
                PhotonNetwork.InstantiateSceneObject(Unit.GetPrefabName(cargoType, cargoTeam), desiredPosition, desiredRotation, 0, o);
                PhotonView.Find((int)args[4]).RPC("DeployedCargo", PhotonTargets.All, null);

            }

        }

        #endregion
    }
}