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
            GameManager g = FindObjectOfType<GameManager>();
            g.unitSelector.gameObject.SetActive(true);
            g.GetComponent<MapModeManager>().ActivateMapMode(MapType.Spawn);
        }

        public SpawnStatus GetSpawnStatus()
        {
            return PlayerSpawnManager.status;
        }

        public static void SpawnPlayer(Vector3 spawnPoint)
        {
            GameManager g = FindObjectOfType<GameManager>();
            g.unitSelector.gameObject.SetActive(false);
            g.SpawnPlayer(spawnPoint + new Vector3(UnityEngine.Random.Range(-6f, 6f), 50, UnityEngine.Random.Range(-9f, 9f)), Quaternion.identity, PhotonNetwork.player.GetTeam(), g.unitSelector.SelectedIndex(), PhotonNetwork.player.ID);
            //g.photonView.RPC("SpawnPlayer", PhotonTargets.MasterClient, spawnPoint + new Vector3(UnityEngine.Random.Range(-6f, 6f), 50, UnityEngine.Random.Range(-9f, 9f)), Quaternion.identity, PhotonNetwork.player.GetTeam(), g.unitSelector.SelectedIndex(), PhotonNetwork.player.ID);
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
