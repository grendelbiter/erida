﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    public class FuelManager : Photon.PunBehaviour {

        public float fuelRegenerationPerSecond = 1f;
        public float landedRegenerationBoost = 5f;
        public int maxFuel = 100;
        public float boost = 1f;
        public int fuel;
        private float fuleRegenerationCollected = 0;

        // Use this for initialization
        void Start() {
            ResetFuel();
        }

        // Update is called once per frame
        void Update() {
            if (photonView.isMine) {
                PlayerMovementManager playerMovementManager = GetComponent<PlayerMovementManager>();
                if (playerMovementManager != null && playerMovementManager.GetIsGrounded())
                    boost = landedRegenerationBoost;
                else
                    boost = 1;
                float fuel = fuelRegenerationPerSecond * boost * Time.deltaTime;
                fuleRegenerationCollected += fuel;
                if (fuleRegenerationCollected >= 1f) {
                    fuleRegenerationCollected--;
                    if (playerMovementManager == null || !playerMovementManager.isDead) {
                        TakeFuel(-1);
                    }
                }
            }
        }

        public void ResetFuel() {
            fuel = maxFuel;
        }

        private GameManager GetGameManager() {
            return FindObjectOfType<GameManager>();
        }


        public bool CanTakeFuel(int amount) {
            int newFuel = fuel - amount;
            return newFuel >= 0 && newFuel <= maxFuel;
        }

        public bool TakeFuel(int amount) {
            if (CanTakeFuel(amount)) {
                int newFuel = fuel - amount;
                fuel = newFuel;
                GetGameManager().FuelLevelUpdated(this);
                return true;
            }
            return false;
        }
    }
}