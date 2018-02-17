using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Com.Wulfram3;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Wulfram3.Scripts.Units
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        public static SpawnStatus status;
        public static float currentSpawnTime = 30f; // in seconds

        public static float spawnRandomnessX = 6f;
        public static float spawnRandomnessZ = 9f;

        public Text countdownText;
        public GameObject countdownPanel;

        private float defaultSpawnTime = 30f;

        private void Start()
        {
            if (PhotonNetwork.player.NickName.Contains("[DEV]"))
            {
                currentSpawnTime = 3f;
                defaultSpawnTime = 3f;
            }
        }

        public void StartSpawn()
        {
            status = SpawnStatus.IsSpawning;
            GameManager g = FindObjectOfType<GameManager>();
            g.unitSelector.gameObject.SetActive(false);
            countdownPanel.SetActive(true);
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
            float randomSpawnX = UnityEngine.Random.Range(-spawnRandomnessX, spawnRandomnessX);
            float randomSpawnZ = UnityEngine.Random.Range(-spawnRandomnessZ, spawnRandomnessZ);
            Vector3 randomSpawnPosition = spawnPoint + new Vector3(randomSpawnX, 50f, randomSpawnZ);
            g.SpawnPlayer(randomSpawnPosition, Quaternion.identity, PhotonNetwork.player.GetTeam(), g.unitSelector.SelectedIndex(), PhotonNetwork.player.ID);
        }

        void Update()
        {
            if(status == SpawnStatus.IsSpawning)
            {
                currentSpawnTime -= Time.deltaTime;
                if (currentSpawnTime < 0)
                {
                    status = SpawnStatus.IsReady;
                    GameManager g = FindObjectOfType<GameManager>();
                    g.unitSelector.gameObject.SetActive(true);
                    countdownPanel.SetActive(false);
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
