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

        private bool healthNeedsUpdate = false;
        private bool teamNeedsUpdate = false;
        private bool meshNeedsUpdate = false;

        void Start() {
            healthLast = hitPointsManager.health;
            teamLast = unitData.unitTeam;
            if (playerMovementManager != null)
                meshLast = playerMovementManager.GetMeshIndex();
        }

        // Update is called once per frame
        void Update() {
            if (hitPointsManager.health != healthLast)
            {
                healthNeedsUpdate = true;
                healthLast = hitPointsManager.health;
            }
            if (unitData.unitTeam != teamLast)
            {
                teamNeedsUpdate = true;
                teamLast = unitData.unitTeam;
            }
            if (playerMovementManager != null && playerMovementManager.GetMeshIndex() != meshLast)
            {
                meshNeedsUpdate = true;
                meshLast = playerMovementManager.GetMeshIndex();
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                if (healthNeedsUpdate)
                {
                    stream.SendNext((int)hitPointsManager.health);
                    healthNeedsUpdate = false;
                }
                if (teamNeedsUpdate)
                {
                    stream.SendNext((PunTeams.Team)unitData.unitTeam);
                    teamNeedsUpdate = false;
                }
                if (playerMovementManager != null && meshNeedsUpdate)
                {
                    stream.SendNext((int)playerMovementManager.GetMeshIndex());
                    meshNeedsUpdate = false;
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