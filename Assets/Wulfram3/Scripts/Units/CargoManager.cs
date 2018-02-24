using Assets.Wulfram3.Scripts.InternalApis.Classes;
using UnityEngine;

namespace Com.Wulfram3
{
    [RequireComponent(typeof(Unit))]
    [RequireComponent(typeof(PlayerManager))]
    [RequireComponent(typeof(PlayerMotionController))]
    public class CargoManager : Photon.PunBehaviour {

        private GameManager gameManager;
        private Transform currentPlaceableObject;
        private float maxPlaceDistance = 24f;
        private float minPlaceDistance = 8f;
        private float currentPlaceDistance = 12f;

        private float dropDelay = 0f;
        private float deployDelay = 0f;

        public Transform placeObjectPoint;
        public Material ghostObjectMaterial;

        public AudioClip cargoPickupSound;
        public AudioClip cargoDropSound;
        public UnitType cargoType;
        public PunTeams.Team cargoTeam;
        public bool hasCargo = false;
        public bool isDeploying = false;
        public Transform dropPosition;

        public PunTeams.Team myTeam;


        void Start() {
            gameManager = FindObjectOfType<GameManager>();
            myTeam = GetComponent<Unit>().unitTeam;
        }

        void Update() {
            if (photonView.isMine && !Cursor.visible) {
                if (Input.GetAxisRaw("DropCargo") != 0f && Time.time > dropDelay) {
                    if (currentPlaceableObject != null)
                        CancelDeployCargo();
                    dropDelay = Time.time + 0.2f;
                    gameManager.photonView.RPC("RequestDropCargo", PhotonTargets.MasterClient, photonView.viewID);
                }

                if (hasCargo) {
                    if (Input.GetAxisRaw("DeployCargo") != 0f && Time.time > deployDelay) {
                        deployDelay = Time.time + 0.2f;
                        ToggleDeployMode();
                    }
                    if (currentPlaceableObject != null)
                    {
                        currentPlaceDistance = Mathf.Clamp(currentPlaceDistance + Input.mouseScrollDelta.y, minPlaceDistance, maxPlaceDistance);
                        currentPlaceableObject.transform.position = GetBestPosition();
                        if (Input.GetMouseButtonDown(0))
                            CheckFinalPlacement();
                    }
                    //TODO: show animation of cargo in player HUD
                } else
                {
                    if (Time.time > deployDelay)
                        isDeploying = false;
                }
            }

        }

        public void Reset()
        {
            hasCargo = false;
            isDeploying = false;
            cargoTeam = myTeam;
            cargoType = UnitType.None;
            currentPlaceableObject = null;
        }

        private void ToggleDeployMode()
        {
            if (currentPlaceableObject == null)
                StartDeployCargo();
            else
                CancelDeployCargo();
        }

        [PunRPC]
        public void GetCargo(PunTeams.Team t, UnitType u, int viewID)
        {
            hasCargo = true;
            cargoType = u;
            cargoTeam = t;
            GetComponent<AudioSource>().PlayOneShot(cargoPickupSound, 1.0f);
            if (PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(PhotonView.Find(viewID).gameObject);
        }

        [PunRPC]
        public void DroppedCargo()
        {
            GetComponent<AudioSource>().PlayOneShot(cargoDropSound, 1.0f);
            hasCargo = false;
            cargoType = UnitType.None;
            cargoTeam = myTeam;
        }

        public void CancelDeployCargo() {
            if (currentPlaceableObject != null)
            {
                Destroy(currentPlaceableObject.gameObject);
                currentPlaceableObject = null;
                isDeploying = false;
            }

        }

        public void StartDeployCargo()
        {
            if (Input.GetAxis("DeployCargo") > 0f && hasCargo && currentPlaceableObject == null)
            {
                isDeploying = true;
                string prefab = Unit.GetNoScriptUnitName(cargoType, myTeam);
                GameObject newObject = Instantiate(Resources.Load(prefab), GetBestPosition(), transform.rotation) as GameObject;
                currentPlaceableObject = newObject.transform;
                ChangeMaterial(ghostObjectMaterial);
            }
        }

        private Vector3 GetBestPosition()
        {
            Vector3 bestPlacePosition = transform.position + (transform.forward * currentPlaceDistance);
            if (currentPlaceableObject != null)
            {
                Terrain terrain = Terrain.activeTerrain;
                Vector3 terrainRelativePosition = bestPlacePosition - terrain.transform.position;
                Vector2 normalizedPosition = new Vector2(terrainRelativePosition.x / terrain.terrainData.size.x, terrainRelativePosition.z / terrain.terrainData.size.z);
                float tHeight = terrain.terrainData.GetInterpolatedHeight(normalizedPosition.x, normalizedPosition.y);
                Vector3 tNormal = terrain.terrainData.GetInterpolatedNormal(normalizedPosition.x, normalizedPosition.y);
                if (Vector3.Angle(transform.up, tNormal) < 23f)
                    currentPlaceableObject.up = tNormal;
                else
                    currentPlaceableObject.up = transform.up;
                if (cargoType == UnitType.GunTurret)
                    bestPlacePosition = new Vector3(bestPlacePosition.x, tHeight + 4f, bestPlacePosition.z);
                else if (cargoType == UnitType.FlakTurret)
                    bestPlacePosition = new Vector3(bestPlacePosition.x, tHeight + 6f, bestPlacePosition.z);
                else
                    bestPlacePosition = new Vector3(bestPlacePosition.x, tHeight, bestPlacePosition.z);
            }
            return bestPlacePosition;
        }

        public void ChangeMaterial(Material newMat)
        {
            foreach (Renderer rend in currentPlaceableObject.transform.GetComponentsInChildren<Renderer>())
            {
                var mats = new Material[rend.materials.Length];
                for (var j = 0; j < rend.materials.Length; j++)
                    mats[j] = newMat;
                rend.materials = mats;
            }
        }

        [PunRPC]
        public void DeployedCargo()
        {
            if (currentPlaceableObject != null)
            {
                Destroy(currentPlaceableObject.gameObject);
                currentPlaceableObject = null;
            }
            deployDelay = Time.time + 0.4f;
            hasCargo = false;
            cargoType = UnitType.None;
            cargoTeam = myTeam;

        }

        public void CheckFinalPlacement()
        {
            if (Input.GetMouseButtonDown(0) && currentPlaceableObject != null)
            {
                object[] args = new object[5];
                args[0] = currentPlaceableObject.transform.position;
                args[1] = currentPlaceableObject.transform.rotation;
                args[2] = this.cargoType;
                args[3] = this.cargoTeam;
                args[4] = this.photonView.viewID;
                gameManager.photonView.RPC("RequestDeployCargo", PhotonTargets.MasterClient, args);
            }
        }
    }
}
