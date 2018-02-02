using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Com.Wulfram3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Wulfram3.Scripts.Units
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        public static SpawnStatus status;

        public static float currentSpawnTime = 30f; // in seconds

        public Text countdownText;

        private float defaultSpawnTime = 30f;

        public void StartSpawn()
        {
            status = SpawnStatus.IsSpawning;
            GameManager gm = FindObjectOfType<GameManager>();
            gm.unitSelector.gameObject.SetActive(true);
        }

        public SpawnStatus GetSpawnStatus()
        {
            return PlayerSpawnManager.status;
        }

        public static void SpawnPlayer(Vector3 spawnPoint)
        {
            PlayerMovementManager player = PlayerMovementManager.LocalPlayerInstance.GetComponent<PlayerMovementManager>();
            GameManager gm = FindObjectOfType<GameManager>();
            player.photonView.RPC("SetSelectedVehicle", PhotonTargets.All, gm.unitSelector.SelectedIndex());
            gm.unitSelector.gameObject.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            float rXp = UnityEngine.Random.Range(-6f, 6f);
            float rZp = UnityEngine.Random.Range(-9f, 9f);

            player.photonView.RPC("SetPosAndRotation", PhotonTargets.All, spawnPoint + new Vector3(rXp, 50, rZp), Quaternion.identity);

            player.GetComponent<Unit>().SetMaxHealth();
            player.GetComponent<CargoManager>().Reset();
            player.GetComponent<FuelManager>().ResetFuel();
            PlayerSpawnManager.status = SpawnStatus.IsAlive;
        }


        void Update()
        {
            if(status == SpawnStatus.IsSpawning)
            {
                currentSpawnTime -= Time.deltaTime;
                if (currentSpawnTime < 0)
                {
                    status = SpawnStatus.IsReady;
                    currentSpawnTime = defaultSpawnTime; // in seconds
                    countdownText.text = "READY FOR DEPLOYMENT!";
                }
                else
                {
                    countdownText.text = Math.Round(currentSpawnTime, 0).ToString();
                }
            }
            
        }
    }


}
