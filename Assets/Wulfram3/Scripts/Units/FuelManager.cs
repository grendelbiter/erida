using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3 {
    public class FuelManager : Photon.PunBehaviour {

        public int fuel = 500;
        public int maxFuel = 500;
        private float fuelRegenerationPerSecond = 0.75f;
        private float landedRegenerationBoost = 5f;
        private float pcMultiplier = 1.25f;
        private float pcBoost = 1f;
        private float landedBoost = 1f;
        private float fuelRegenerationCollected = 0;

        void Start() { }

        void Update() {
            if (photonView.isMine) {
                fuelRegenerationCollected += (fuelRegenerationPerSecond + landedBoost) * pcBoost * Time.deltaTime;
                if (fuelRegenerationCollected >= 1f) {
                    fuelRegenerationCollected--;
                    TakeFuel(-1);
                }
            }
        }

        public void ResetFuel() {
            fuel = maxFuel;
        }

        public void SetLandedBoost(bool t)
        {
            if (t)
                landedBoost = landedRegenerationBoost;
            else
                landedBoost = 1f;
        }

        public void SetPowerCellBoost(bool t)
        {
            if (t)
                pcBoost = pcMultiplier;
            else
                pcBoost = 1f;
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
