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
            healthLast = hitPointsManager.health;
            teamLast = unitData.unitTeam;
            if (playerMovementManager != null)
                meshLast = playerMovementManager.GetMeshIndex();
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