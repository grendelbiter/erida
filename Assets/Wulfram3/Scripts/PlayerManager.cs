using Assets.Wulfram3.Scripts.InternalApis;
using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Assets.Wulfram3.Scripts.InternalApis.Interfaces;
using Assets.Wulfram3.Scripts.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Wulfram3
{
    public class PlayerManager : Photon.PunBehaviour
    {
        // Inspector Vars
        [Tooltip("Delay, in seconds, before respawn map appears")]
        public float destroyDelayWhenDead = 3;
        public Transform[] meshList;
        public List<Mesh> availableColliders;
        public List<Texture2D> myIconTextures;
        public List<Transform> cargoDropPositions;
        public List<Transform> unitPlacePositions;
        public List<Transform> gunPositions;
        public List<Transform> firstPersonCameraPositions;
        public List<Transform> thirdPersonCameraPositions;

        private int fuelPerPulse = 135; // Reduced from 180 last
        private float timeBetweenPulse = 3f;
        private float pulseShellFiringImpulse = 8f;

        // Internal vars
        [HideInInspector]
        public bool isDead = false;
        [HideInInspector]
        public bool isSpawning = false;

        private GameManager gameManager;
        public IVehicleSetting mySettings;
        private Rigidbody myRigidbody;
        private FuelManager fuelManager;
        private int meshIndex = 0;
        private float pulseStamp;
        private float timeSinceDead = 0;

        //[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        [HideInInspector]
        public static GameObject LocalPlayerInstance;
        [HideInInspector]
        public KGFMapIcon myMapIcon;
        [HideInInspector]
        public Unit myUnit;
        [HideInInspector]
        public Transform myMesh;
        [HideInInspector]
        public Transform gunEnd;
        [HideInInspector]
        public Transform firstPersonCamPos;
        [HideInInspector]
        public Transform thirdPersonCamPos;
        [HideInInspector]
        public Transform placePosition;
        [HideInInspector]
        public Transform dropPosition;

        private PlayerMotionController PMC;

        private GameObject localShell;
        private bool gotServerShell = false;

        void Start()
        {
        }

        private void Awake()
        {
            myUnit = GetComponent<Unit>();
            gameManager = FindObjectOfType<GameManager>();
            myRigidbody = GetComponent<Rigidbody>();
            if (photonView.isMine)
            {
                PlayerManager.LocalPlayerInstance = gameObject;
                fuelManager = GetComponent<FuelManager>();
                PMC = GetComponent<PlayerMotionController>();
                gameManager.GetComponent<MapModeManager>().ActivateMapMode(MapType.Mini);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                PlayerSpawnManager.status = SpawnStatus.IsAlive;
            }
        }

        public void SetMesh(PunTeams.Team t, UnitType u)
        {
            for (int n = 0; n < meshList.Length; n++)
                meshList[n].gameObject.SetActive(false);
            int i = GetPlayerMeshIndexFromType(t, u);
            meshIndex = i;
            meshList[i].gameObject.SetActive(true);
            myMesh = meshList[i];
            gunEnd = gunPositions[i];
            GetComponent<AutoCannon>().gunEnd = gunPositions[i];
            GetComponent<MeshCollider>().sharedMesh = availableColliders[i];
            mySettings = VehicleSettingFactory.GetVehicleSetting(GetPlayerTypeFromMeshIndex(i));
            if (!photonView.isMine)
                return;
            CameraManager cm = GetComponent<CameraManager>();
            cm.SetFirstPersonPosition(firstPersonCameraPositions[i]);
            cm.SetThirdPersonPosition(thirdPersonCameraPositions[i]);
            CargoManager cargoManager = GetComponent<CargoManager>();
            if (i == 0)
            {
                cargoManager.dropPosition = cargoDropPositions[0];
                cargoManager.placeObjectPoint = unitPlacePositions[0];
            }
            else if (i == 2)
            {
                cargoManager.dropPosition = cargoDropPositions[1];
                cargoManager.placeObjectPoint = unitPlacePositions[1];
            }
        }

        public int GetPlayerMeshIndexFromType(PunTeams.Team t, UnitType u)
        {
            if (t == PunTeams.Team.Blue && u == UnitType.Tank)
                return 0;
            else if (t == PunTeams.Team.Blue && u == UnitType.Scout)
                return 1;
            else if (t == PunTeams.Team.Red && u == UnitType.Tank)
                return 2;
            else if (t == PunTeams.Team.Red && u == UnitType.Scout)
                return 3;
            return -1;
        }

        public static UnitType GetPlayerTypeFromMeshIndex(int i)
        {
            if (i == 0 || i == 2)
                return UnitType.Tank;
            else if (i == 1 || i == 3)
                return UnitType.Scout;
            return UnitType.None;
        }


        public int GetMeshIndex()
        {
            return meshIndex;
        }

        [PunRPC]
        public void SetTeam(PunTeams.Team t)
        {
            myUnit.unitTeam = t;
        }

        public void CheckIsDead()
        {
            if (myUnit.isDead)
            {
                if (timeSinceDead == 0)
                {
                    isDead = true;
                    myRigidbody.freezeRotation = false;
                    myRigidbody.isKinematic = false;
                    if (PMC != null)
                        PMC.receiveInput = false;
                }
                timeSinceDead += Time.deltaTime;
                if (timeSinceDead >= destroyDelayWhenDead && photonView.isMine)
                {
                    gameManager.SpawnExplosion(transform.position, myUnit.unitType);
                    GetComponent<CameraManager>().Detach();
                    gameManager.GetComponent<PlayerSpawnManager>().StartSpawn();
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        private void CheckFireSecondary()
        {
            if (InputEx.GetAxisRaw("Fire2") != 0)
            {
                if (myUnit.unitType == UnitType.Tank && localShell == null && Time.time >= pulseStamp && fuelManager.TakeFuel(fuelPerPulse))
                    CmdFirePulseShell();
                else if (myUnit.unitType == UnitType.Scout)
                    Logger.Log("PlayerMovementManager.cs (Line: 309) Scout Secondary Firing Detected.");
            }
        }

        void Update()
        {
            CheckIsDead();


            if (!photonView.isMine || PMC == null)
            {
                if (PMC != null && PMC.receiveInput)
                    PMC.receiveInput = false;
                return;
            }

            if (!Cursor.visible)
            {
                if (!PMC.receiveInput)
                    PMC.receiveInput = true;
                CheckFireSecondary(); // Detects tank or scout, fires appropriate weapon
            }
            else
            {
                if (PMC.receiveInput)
                    PMC.receiveInput = false;
            }
            if (localShell != null && gotServerShell)
            {
                Destroy(localShell);
                localShell = null;
            }
        }

        void CmdFirePulseShell()
        {
            pulseStamp = Time.time + timeBetweenPulse;
            Vector3 v = myRigidbody.velocity + (gunEnd.forward * SplashProjectileController.PulseVelocity);
            object[] args = new object[6];
            args[0] = gunEnd.position;
            args[1] = gunEnd.rotation;
            args[2] = myUnit.unitTeam;
            args[3] = v;
            args[4] = this.photonView.viewID;
            args[5] = PhotonNetwork.time;
            localShell = (GameObject) Instantiate(Resources.Load("Prefabs/Weapons/DummyPulse_" + myUnit.unitTeam), gunEnd.position, gunEnd.rotation);
            localShell.GetComponent<Rigidbody>().velocity = v;
            gotServerShell = false;
            gameManager.photonView.RPC("SpawnPulseShell", PhotonTargets.MasterClient, args);
            if (!PMC.isGrounded)
                myRigidbody.AddForce(-transform.forward * pulseShellFiringImpulse, ForceMode.Impulse);
        }

        [PunRPC]
        public void PulseConfirmed(float t)
        {
            gotServerShell = true;
        }

        public void FixedUpdate()
        {
        }
    }
}
