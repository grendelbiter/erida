using Assets.Wulfram3.Scripts.InternalApis;
using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Assets.Wulfram3.Scripts.InternalApis.Interfaces;
using Assets.Wulfram3.Scripts.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Wulfram3
{
    public class VehicleSelector : MonoBehaviour {

        public Transform playerPrefab;
        public Transform[] nestedModels;
        public List<int> available;
        public int current = 0;
        private int last = 0;
        private float switchStamp;
        private int rotateSpeed = 25;

        public Text typeText;
        public Text hpText;
        public Text speedText;
        public Text aText;
        public Text bText;

        void Start() {
            SwitchModel();
        }

        void Update() {
            if (current != last)
            {
                SwitchModel();
                last = current;
            }
            playerPrefab.RotateAround(playerPrefab.position, Vector3.up, rotateSpeed * Time.deltaTime);
        }

        public void SetAvailableModels(List<int> i)
        {
            available = i;
        }

        private void SwitchModel()
        {
            if (Time.time > switchStamp)
            {
                switchStamp = Time.time + 0.3f;
                for (int i = 0; i < available.Count; i++)
                {
                    if (current == i)
                    {
                        nestedModels[available[i]].gameObject.SetActive(true);
                        IVehicleSetting s = VehicleSettingFactory.GetVehicleSetting(PlayerManager.GetPlayerTypeFromMeshIndex(available[i]));
                        typeText.text = PlayerManager.GetPlayerTypeFromMeshIndex(available[i]).ToString();
                        hpText.text = "Hit Point: " + s.MaxHitPoints.ToString();
                        speedText.text = "Fwd Speed: " + s.MaxVelocityZ.ToString();
                        aText.text = "Strafe Speed: " + s.MaxVelocityX.ToString();
                        bText.text = "Weapons: " + s.AvailableWeapons.ToString();
                    }
                    else
                    {
                        nestedModels[available[i]].gameObject.SetActive(false);

                    }
                }
            }
        }

        public int SelectedIndex()
        {
            return available[current];
        }

        public void Next()
        {
            current++;
            if (available.Count > 1 && current >= available.Count)
                current = 0;
        }

        public void Previous()
        {
            current--;
            if (available.Count > 1 && current < 0)
                current = available.Count - 1;
        }
    }
}