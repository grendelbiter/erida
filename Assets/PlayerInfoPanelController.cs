﻿using Assets.Wulfram3.Scripts.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Wulfram3 {
    public class PlayerInfoPanelController : Photon.PunBehaviour {
        private GameObject target;
        private Vector3 pos;
        private GameManager gameManager;

        public Text playerNameText;


        // Use this for initialization
        void Start() {
            Canvas canvas = FindObjectOfType<Canvas>();
            transform.SetParent(canvas.transform);
            gameManager = FindObjectOfType<GameManager>();
        }

        // Update is called once per frame
        void LateUpdate() {
            if (target != null && target.GetComponentInChildren<MeshRenderer>() != null) // New (see note at second "if"
            {
                var isMeshVisable = target.GetComponentInChildren<MeshRenderer>().isVisible;
                var isMapIconVisable = target.GetComponentInChildren<KGFMapIcon>().GetIsVisible();



                if (isMeshVisable && isMapIconVisable) // target != null was here
                {
                    playerNameText.gameObject.SetActive(false);
                    pos = Camera.main.WorldToScreenPoint(target.transform.position);
                    pos.z = 0;
                    RectTransform rectTransform = GetComponent<RectTransform>();
                    pos.y += 50;

                    string playerName = target.GetComponent<PhotonView>().owner.NickName;
                    // [ASSIGNED NEVER USED] string hitpoints = target.GetComponent<Unit>().health + "/" + target.GetComponent<Unit>().maxHealth;

                    var name = gameManager.GetColoredPlayerName(playerName, target.GetComponent<PhotonView>().owner.IsMasterClient, true, target.GetComponent<Unit>().unitTeam);
                    playerNameText.text = name;

                    rectTransform.SetPositionAndRotation(pos, rectTransform.rotation);
                }
                else
                {
                    playerNameText.gameObject.SetActive(false);
                }
            }
        }

        // Update is called once per frame
        void Update() {
 
        }

        public void SetTarget(GameObject target) {
            this.target = target;
        }

    }
}