using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3
{

    public class EridaObjectSyncView : Photon.PunBehaviour {

        public PlayerMovementManager playerMovementManager;
        public HitPointsManager hitPointsManager;
        public Unit unitData;

        private int healthLast;
        private PunTeams.Team teamLast;
        private int meshLast;

        void Start() {
            if (PhotonNetwork.isMasterClient)
            {
                healthLast = hitPointsManager.health;
                teamLast = unitData.unitTeam;
                if (playerMovementManager != null)
                    meshLast = playerMovementManager.GetMeshIndex();
            } else
            {
                this.photonView.RPC("GetDataFromMaster", PhotonTargets.MasterClient);
            }
        }

        [PunRPC]
        public void GetDataFromMaster(PhotonMessageInfo info)
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (playerMovementManager == null)
                {
                    object[] o = new object[2];
                    o[0] = hitPointsManager.health;
                    o[1] = unitData.unitTeam;
                    this.photonView.RPC("ReceiveDataFromMaster", info.sender, o);
                }
                else
                {
                    object[] o = new object[3];
                    o[0] = hitPointsManager.health;
                    o[1] = unitData.unitTeam;
                    o[2] = playerMovementManager.GetMeshIndex();
                    this.photonView.RPC("ReceiveDataFromMaster", info.sender, o);
                }
            }
        }

        [PunRPC]
        public void ReceiveDataFromMaster(object[] o)
        {
            hitPointsManager.health = (int)o[0];
            unitData.unitTeam = (PunTeams.Team)o[1];
            if (playerMovementManager != null)
            {
                playerMovementManager.SetMesh((int)o[2]);
            }
        }

        // Update is called once per frame
        void Update() {
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext((int)hitPointsManager.health);
                stream.SendNext((PunTeams.Team)unitData.unitTeam);
                if (playerMovementManager != null)
                {
                    stream.SendNext((int)playerMovementManager.GetMeshIndex());
                }
            } else
            {
                int syncHealth = (int)stream.ReceiveNext();
                hitPointsManager.health = Mathf.Clamp(syncHealth, 0, hitPointsManager.maxHealth);
                PunTeams.Team syncTeam = (PunTeams.Team)stream.ReceiveNext();
                unitData.unitTeam = syncTeam;

                if (playerMovementManager != null)
                {
                    int syncMesh = (int)stream.ReceiveNext();
                    playerMovementManager.SetMesh(syncMesh);
                }
            }
        }
    }
}